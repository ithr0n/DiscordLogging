using System;
using System.Collections;
using System.IO;
using System.Text;
using Discord;
using Discord.Webhook;
using Microsoft.Extensions.Logging;

namespace DiscordLogging
{
    public class DiscordLogger
        : ILogger
    {
        private readonly DiscordLoggerConfiguration _config;

        public DiscordLogger(DiscordLoggerConfiguration config)
        {
            _config = config;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var formattedMessage = formatter(state, exception);
            if (string.IsNullOrEmpty(formattedMessage))
            {
                return;
            }

            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "DiscordLogger"
                },
                Title = logLevel.ToString(),
                Description = formattedMessage
            };

            switch (logLevel)
            {
                case LogLevel.None:
                case LogLevel.Trace:
                    break;
                case LogLevel.Debug:
                    embed.Title = $":spider_web: {embed.Title}";
                    break;
                case LogLevel.Information:
                    embed.Title = $":information_source: {embed.Title}";
                    embed.Color = Color.Teal;
                    break;
                case LogLevel.Warning:
                    embed.Title = $":warning: {embed.Title}";
                    embed.Color = Color.Orange;
                    break;
                case LogLevel.Error:
                    embed.Title = $":skull: {embed.Title}";
                    embed.Color = Color.Red;
                    break;
                case LogLevel.Critical:
                    embed.Title = $":radioactive: {embed.Title}";
                    embed.Color = Color.DarkRed;
                    break;
                default:
                    return;
            }

            var client = new DiscordWebhookClient(_config.WebhookUrl);

            if (exception == null)
            {
                client.SendMessageAsync(null, false, new[] { embed.Build() });
                return;
            }

            #region build details text

            embed.AddField("Exception type", exception.GetType().ToString());
            embed.AddField("Source", exception.Source);

            var exceptionInfoText = new StringBuilder();

            exceptionInfoText.AppendFormat("Message: {0}\r\n", exception.Message);
            exceptionInfoText.AppendFormat("Exception type: {0}\r\n", exception.GetType());
            exceptionInfoText.AppendFormat("Source: {0}\r\n", exception.Source);

            var compareException = exception;
            var innerException = compareException.GetBaseException();
            while (compareException != innerException)
            {
                exceptionInfoText.AppendFormat("Base exception: {0}\r\n", innerException.Message);

                compareException = innerException;
                innerException = compareException.GetBaseException();
            }

            foreach (DictionaryEntry data in exception.Data)
            {
                exceptionInfoText.Append($"{data.Key}: {data.Value}\r\n");
            }

            exceptionInfoText.AppendFormat("Stack trace: {0}\r\n", exception.StackTrace);

            var exceptionDetails = Encoding.UTF8.GetBytes(exceptionInfoText.ToString());

            #endregion

            using var stream = new MemoryStream(exceptionDetails);

            client.SendFileAsync(stream, "exception-details.txt", null, false, new[] { embed.Build() });
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _config.WebhookUrl.StartsWith("https://discord.com/api/webhooks/");
        }

        public IDisposable BeginScope<TState>(TState state) => default;
    }
}
