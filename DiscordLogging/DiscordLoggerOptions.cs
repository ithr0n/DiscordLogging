using System;

namespace DiscordLogging
{
    public class DiscordLoggerOptions
    {
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Specify the interval between posting messages to Discord. As default messages are stored every 2 seconds.
        /// </summary>
        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Specify the max length of the Discord message. As default the limit is set to 2000.
        /// </summary>
        public int MessageLimit { get; set; } = 2000;

        /// <summary>
        /// Specify the size of the queue storing messages to be logged. As default the queue size is 100. If you log a lot of messages in your
        /// application, you can increase the queue size. But that will cause delays in transmitting the messages (rate limits by Discord API).
        /// </summary>
        public int BackgroundQueueSize { get; set; } = 100;
    }
}
