using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Webhook;

namespace DiscordLogging
{
    internal sealed class MessageQueue
        : ICanHandleMessages
    {
        private readonly DiscordLoggerOptions _options;
        private readonly List<DiscordLogMessage> _currentBatch = new();

        private DiscordWebhookClient _client;
        private BlockingCollection<DiscordLogMessage> _messages;
        private CancellationTokenSource _cancellationTokenSource;
        private int _messagesDropped;

        // can be used for unit tests
        internal MessageQueue(DiscordLoggerOptions options, DiscordWebhookClient client)
        {
            _options = options;
            _client = client;
        }

        public MessageQueue(DiscordLoggerOptions options)
        {
            _options = options;
        }

        internal void Start()
        {
            _messages = new BlockingCollection<DiscordLogMessage>(new ConcurrentQueue<DiscordLogMessage>(), _options.BackgroundQueueSize);
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(ProcessLogQueue, CancellationToken.None, TaskCreationOptions.LongRunning);
        }

        internal void Stop()
        {
            try
            {
                if (_messages.Count > 0)
                {
                    // Remaining messages in queue. Flush them if possible.
                    ProcessMessages().GetAwaiter().GetResult();
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
            {
            }

            _cancellationTokenSource.Cancel();
            _messages.CompleteAdding();
        }

        public void AddMessage(DiscordLogMessage message)
        {
            if (_messages.IsAddingCompleted)
            {
                // should never happen, because we do not complete adding explicity
                return;
            }

            try
            {
                if (!_messages.TryAdd(message, millisecondsTimeout: 0, cancellationToken: _cancellationTokenSource.Token))
                {
                    Interlocked.Increment(ref _messagesDropped);
                }
            }
            catch
            {
                // Cancellation token canceled or CompleteAdding called
            }
        }

        private async Task WriteMessagesAsync(IEnumerable<DiscordLogMessage> messages)
        {
            _client ??= new DiscordWebhookClient(_options.WebhookUrl);

            var bulkMessages = GetBulkMessagesFromMany(messages);

            foreach (var message in bulkMessages)
            {
                if (message.File != null)
                {
                    await _client.SendFileAsync(message.File, "details.txt", message.Message,
                        embeds: message.Embeds);
                }
                else
                {
                    await _client.SendMessageAsync(message.Message, embeds: message.Embeds);
                }

                await Task.Delay(_options.Period, _cancellationTokenSource.Token);
            }
        }

        private IEnumerable<DiscordLogMessage> GetBulkMessagesFromMany(IEnumerable<DiscordLogMessage> messages)
        {
            var result = new List<DiscordLogMessage>();

            var sb = new StringBuilder();

            foreach (var msg in messages)
            {
                if (msg.File != null || msg.Embeds is { Length: > 0 })
                {
                    // add previous parsed messages as bulk message
                    if (sb.Length > 0)
                    {
                        result.Add(new DiscordLogMessage { Message = sb.ToString() });
                        sb = new StringBuilder();
                    }

                    // embeds and files must be a single messages
                    result.Add(msg);
                    continue;
                }

                if (sb.Length + msg.Message.Length > _options.BulkMessageLimit)
                {
                    // message limit is reached, add previous parsed messages as bulk message
                    result.Add(new DiscordLogMessage { Message = sb.ToString() });
                    sb = new StringBuilder();
                }

                // append message for next bulk message
                sb.AppendLine(msg.Message);
            }

            if (sb.Length > 0)
            {
                // no more messages, add previous parsed messages as bulk message
                result.Add(new DiscordLogMessage { Message = sb.ToString() });
            }

            return result;
        }

        private async Task ProcessLogQueue(object state)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                // Process messages
                await ProcessMessages();

                // Then wait until next check
                await Task.Delay(_options.Period, _cancellationTokenSource.Token);
            }
        }
        
        private async Task ProcessMessages()
        {
            while (_messages.TryTake(out var message))
            {
                _currentBatch.Add(message);
            }

            var messagesDropped = Interlocked.Exchange(ref _messagesDropped, 0);
            if (messagesDropped != 0)
            {
                _currentBatch.Add(new DiscordLogMessage
                {
                    Message =
                        $"{messagesDropped} message(s) dropped because of queue size limit. Increase the queue size or decrease logging verbosity to avoid this."
                });
            }

            if (_currentBatch.Count > 0)
            {
                try
                {
                    await WriteMessagesAsync(_currentBatch);
                }
                catch
                {
                    // ignored
                }

                _currentBatch.Clear();
            }
        }
    }
}
