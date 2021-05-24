using Microsoft.Extensions.Logging;

namespace DiscordLogging
{
    public sealed class DiscordLoggerProvider 
        : ILoggerProvider
    {
        private readonly DiscordLoggerConfiguration _config;

        private DiscordLogger _logger;

        public DiscordLoggerProvider(DiscordLoggerConfiguration config)
        {
            _config = config;
        }

        public ILogger CreateLogger(string name)
        {
            _logger = new DiscordLogger(_config);

            return _logger;
        }

        public void Dispose()
        {
        }
    }
}
