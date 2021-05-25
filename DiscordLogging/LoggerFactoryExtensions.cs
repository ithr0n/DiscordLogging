using Microsoft.Extensions.Logging;

namespace DiscordLogging
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddDiscord(
            this ILoggerFactory factory,
            string webhookUrl)
        {
            factory.AddProvider(new DiscordLoggerProvider(webhookUrl));
            return factory;
        }

        public static ILoggerFactory AddDiscord(
            this ILoggerFactory factory,
            string webhookUrl,
            DiscordLoggerOptions options)
        {
            factory.AddProvider(new DiscordLoggerProvider(webhookUrl, options));
            return factory;
        }
    }
}
