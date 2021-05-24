using System.Threading;
using Microsoft.Extensions.Logging;

namespace DiscordLogging
{
    public sealed class DiscordLoggerProvider 
        : ILoggerProvider
    {
        private readonly CancellationTokenSource _cancelToken;
        private readonly AsyncWorker _asyncWorker;

        private DiscordLogger _logger;

        public DiscordLoggerProvider(DiscordLoggerConfiguration config)
        {
            _cancelToken = new CancellationTokenSource();
            _asyncWorker = new AsyncWorker(config, _cancelToken.Token);
        }

        public ILogger CreateLogger(string name)
        {
            _logger = new DiscordLogger(_asyncWorker);

            return _logger;
        }

        public void Dispose()
        {
            _cancelToken.Cancel(false);
        }
    }
}
