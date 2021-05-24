using System;
using System.Collections;
using System.Text;
using Discord;
using Microsoft.Extensions.Logging;

namespace DiscordLogging
{
    public sealed class DiscordLogger
        : ILogger
    {
        private readonly AsyncWorker _asyncWorker;

        public DiscordLogger(AsyncWorker asyncWorker)
        {
            _asyncWorker = asyncWorker;
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

            var icon = logLevel switch
            {
                LogLevel.Debug => ":spider_web:",
                LogLevel.Information => ":information_source:",
                LogLevel.Warning => ":warning:",
                LogLevel.Error => ":skull:",
                LogLevel.Critical => ":radioactive:",
                _ => ":black_large_square:"
            };

            var model = new DiscordLogModel()
            {
                Message = $"{icon} **[{logLevel}]**   {formattedMessage}"
            };

            if (exception == null)
            {
                _asyncWorker.AddToQueue(model);
                return;
            }

            #region build details text

            var embed = new EmbedBuilder()
            {
                Title = "Exception Details",
                Color = Color.DarkRed
            };

            embed.AddField("Message", exception.Message);
            embed.AddField("Exception type", exception.GetType().ToString());
            embed.AddField("Source", exception.Source);

            /*var exceptionInfoText = new StringBuilder();

            exceptionInfoText.Append($"Message: {exception.Message}\r\n");
            exceptionInfoText.Append($"Exception type: {exception.GetType()}\r\n");
            exceptionInfoText.Append($"Source: {exception.Source}\r\n");

            var compareException = exception;
            var innerException = compareException.GetBaseException();
            while (compareException != innerException)
            {
                exceptionInfoText.Append($"\r\nBase exception: {innerException.Message}\r\n");

                compareException = innerException;
                innerException = compareException.GetBaseException();
            }

            if (exception.Data.Count > 0)
            {
                exceptionInfoText.Append("\r\n");
            }

            foreach (DictionaryEntry data in exception.Data)
            {
                exceptionInfoText.Append($"{data.Key}: {data.Value}\r\n");
            }

            exceptionInfoText.Append($"\r\nStack trace: {exception.StackTrace}\r\n");

            var exceptionDetails = Encoding.UTF8.GetBytes(exceptionInfoText.ToString());


            using var stream = new MemoryStream(exceptionDetails);

            client.SendFileAsync(stream, "exception-details.txt", formattedMessage, embeds: new[] { embed.Build() });
            */

            #endregion

            var compareException = exception;
            var innerException = compareException.GetBaseException();
            var counter = 0;
            while (compareException != innerException)
            {
                embed.AddField($"Inner Exception {++counter}", innerException.Message);

                compareException = innerException;
                innerException = compareException.GetBaseException();
            }

            if (exception.Data.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (DictionaryEntry data in exception.Data)
                {
                    sb.AppendLine($"{data.Key}: {data.Value}");
                }

                embed.AddField("Exception Data", sb.ToString());
            }

            embed.AddField("Stack Trace", exception.StackTrace);

            model.Embeds = new [] { embed.Build() };

            _asyncWorker.AddToQueue(model);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => default;
    }
}
