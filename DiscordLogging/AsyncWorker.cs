using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord.Webhook;

namespace DiscordLogging
{
    public sealed class AsyncWorker
    {
        private readonly DiscordLoggerConfiguration _config;
        private readonly BlockingCollection<DiscordLogModel> _queue;

        public AsyncWorker(DiscordLoggerConfiguration config, CancellationToken cancelToken)
        {
            _config = config;
            _queue = new BlockingCollection<DiscordLogModel>();

            Task.Factory
                .StartNew(ProcessLogQueue, cancelToken, TaskCreationOptions.LongRunning)
                .PerformAsyncTaskWithoutAwait();
        }

        public void AddToQueue(DiscordLogModel model)
        {
            _queue.Add(model);
        }

        private async Task ProcessLogQueue(object state)
        {
            var client = new DiscordWebhookClient(_config.WebhookUrl);
            
            foreach (var logEntry in _queue.GetConsumingEnumerable((CancellationToken)state))
            {
                // send current log entry
                if (logEntry.File != null)
                {
                    await client
                        .SendFileAsync(logEntry.File, logEntry.FileName ?? "file.txt", logEntry.Message,
                            embeds: logEntry.Embeds);
                }
                else
                {
                    await client
                        .SendMessageAsync(logEntry.Message, embeds: logEntry.Embeds);
                }
            }
        }
    }
}
