using System;

namespace DiscordLogging
{
    public class DiscordLoggerConfiguration
    {
        public string WebhookUrl { get; }

        public string ApplicationName { get; set; }

        public string UserName { get; set; }

        public string EnvironmentName { get; set; }

        public DiscordLoggerConfiguration(string webhookUrl)
        {
            if (string.IsNullOrEmpty(webhookUrl))
                throw new ArgumentNullException(nameof(webhookUrl), "The Discord webhook URL cannot be null or empty.");

            if (!(Uri.TryCreate(webhookUrl, UriKind.Absolute, out Uri uriResult)
                  && (uriResult.Scheme == "http" || uriResult.Scheme == "https")))
            {
                throw new ArgumentException($"Invalid Discord webhook URL: {webhookUrl}");
            }

            WebhookUrl = webhookUrl;
        }
    }
}
