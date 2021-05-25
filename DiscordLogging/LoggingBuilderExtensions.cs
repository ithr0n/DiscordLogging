using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordLogging
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddDiscord(
            this ILoggingBuilder builder,
            Action<DiscordLoggerOptions> configure)
        {
            builder.AddDiscord();
            builder.Services.Configure(configure);

            return builder;
        }

        public static ILoggingBuilder AddDiscord(
            this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, DiscordLoggerProvider>(services =>
            {
                var options = services.GetService<IOptions<DiscordLoggerOptions>>();
                return new DiscordLoggerProvider(options);
            });

            return builder;
        }
    }
}
