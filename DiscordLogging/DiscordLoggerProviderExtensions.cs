using Microsoft.Extensions.Logging;

namespace DiscordLogging
{
    public static class DiscordLoggerProviderExtensions
    {
        public static ILoggingBuilder AddDiscord(this ILoggingBuilder builder, DiscordLoggerConfiguration config)
        {
            builder.AddProvider(new DiscordLoggerProvider(config));

            return builder;
        }
    }
}
