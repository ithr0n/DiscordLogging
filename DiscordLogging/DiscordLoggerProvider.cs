using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordLogging
{
    [ProviderAlias("Discord")]
    public sealed class DiscordLoggerProvider 
        : ILoggerProvider
    {
        private readonly MessageQueue _messageQueue;
        private readonly DiscordLoggerOptions _options;

        public DiscordLoggerProvider(IOptions<DiscordLoggerOptions> options)
            : this(options.Value.WebhookUrl, options.Value)
        {
        }
        
        public DiscordLoggerProvider(
            string webhookUrl,
            DiscordLoggerOptions options = null)
        {
            _options = options ?? new DiscordLoggerOptions();
            _options.WebhookUrl = webhookUrl;
            
            if (string.IsNullOrEmpty(_options.WebhookUrl))
            {
                throw new ArgumentNullException(nameof(_options.WebhookUrl), "The Discord webhook URL cannot be null or empty.");
            }

            if (!(Uri.TryCreate(_options.WebhookUrl, UriKind.Absolute, out var uriResult)
                  && uriResult.Scheme is "http" or "https"))
            {
                throw new ArgumentException($"Invalid Discord webhook URL: {_options.WebhookUrl}");
            }

            if (_options.MessageLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.MessageLimit), $"{nameof(_options.MessageLimit)} must be a positive number.");
            }

            if (_options.Period <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.Period), $"{nameof(_options.Period)} must be longer than zero.");
            }
            
            _messageQueue = new MessageQueue(_options);
            _messageQueue.Start();
        }

        public ILogger CreateLogger(string name)
        {
            return new DiscordLogger(_options, _messageQueue);
        }

        public void Dispose()
        {
            _messageQueue?.Stop();
        }
    }
}
