using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiscordLogging.Tests
{
    [TestClass]
    public class DiscordLoggerTests
    {
        private readonly ILogger _logger;

        public IConfiguration Configuration { get; set; }

        public DiscordLoggerTests()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<DiscordLoggerTests>();

            Configuration = builder.Build();

            _logger = GetDiscordLogger(Configuration["DiscordWebhookUrl"]);
        }
        
        private static ILogger GetDiscordLogger(string webhookUrl)
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddDiscord(new DiscordLoggerConfiguration(webhookUrl));
            });

            return factory.CreateLogger<DiscordLoggerTests>();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            // required, because webhooks are slow
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod]
        public void Should_Send_A_Discord_Trace_Message()
        {
            _logger.LogTrace("My trace message is here!");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Send_A_Discord_Debug_Message()
        {
            _logger.LogDebug("My debug message is here!");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Send_A_Discord_Information_Message()
        {
            _logger.LogInformation("My info message is here!");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Send_Many_Ordered_Discord_Information_Message()
        {
            for (var i = 0; i < 6; i++)
            {
                _logger.LogInformation($"Message {i}");
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Send_A_Discord_Warning_Message()
        {
            _logger.LogWarning("My warning message is here!");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Send_A_Discord_Error_Message()
        {
            _logger.LogError("My error message is here!");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Send_A_Discord_Critical_Message()
        {
            _logger.LogCritical("My critical message is here!");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Send_A_Exception_Message()
        {
            try
            {
                var i = 0;

                var x = 5 / i;
            }
            catch (Exception ex)
            {
                ex.Data["Extra info 1"] = "Extra info 1 value";
                ex.Data["Extra info 2"] = "Extra info 2 value";

                _logger.LogError(ex, "A exception is handled!");

                System.Threading.Thread.Sleep(1000);

                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Should_Not_Allow_Empty_Token()
        {
            try
            {
                var logger = GetDiscordLogger(string.Empty);

                Assert.IsTrue(false);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.GetType() == typeof(ArgumentNullException), ex.Message);
            }
        }
    }
}
