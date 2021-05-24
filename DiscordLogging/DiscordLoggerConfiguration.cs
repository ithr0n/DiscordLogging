using System;

namespace DiscordLogging
{
    public class DiscordLoggerConfiguration
    {
        public string WebhookUrl { get; }

        public DiscordLoggerConfiguration(string webhookUrl)
        {
            if (string.IsNullOrEmpty(webhookUrl))
                throw new ArgumentNullException(nameof(webhookUrl), "The Discord webhook URL cannot be null or empty.");

            if (!(Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uriResult)
                  && uriResult.Scheme is "http" or "https"))
            {
                throw new ArgumentException($"Invalid Discord webhook URL: {webhookUrl}");
            }

            WebhookUrl = webhookUrl;
        }
    }
}
