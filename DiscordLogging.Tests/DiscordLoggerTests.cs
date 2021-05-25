using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiscordLogging.Tests
{
    [TestClass]
    public class DiscordLoggerTests
    {
        private readonly ILoggerFactory _defaultFactory;
        private readonly ILogger _logger;

        private static IConfiguration _configuration;


        public DiscordLoggerTests()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<DiscordLoggerTests>();

            _configuration = builder.Build();

            _defaultFactory = GetLoggerFactoryWithDiscord(_configuration["DiscordWebhookUrl"]);
            _logger = _defaultFactory.CreateLogger<DiscordLoggerTests>();
        }
        
        private static ILoggerFactory GetLoggerFactoryWithDiscord(string webhookUrl, LogLevel logLevel = LogLevel.Trace)
        {
            var loggerFactory = LoggerFactory.Create(configure =>
            {
                configure.AddDiscord(options =>
                {
                    options.WebhookUrl = webhookUrl;
                });
                configure.AddFilter(null, logLevel);
            });

            return loggerFactory;
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            // required, because webhooks are slow
            System.Threading.Thread.Sleep(20000);
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
            for (var i = 0; i < 200; i++)
            {
                _logger.LogInformation($"Message {i}");
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Send_Many_Ordered_Discord_Information_Message_From_Different_Loggers()
        {
            var logA = _defaultFactory.CreateLogger("A");
            var logB = _defaultFactory.CreateLogger("B");
            var logC = _defaultFactory.CreateLogger("C");
            var logD = _defaultFactory.CreateLogger("D");

            for (var i = 0; i < 100; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        logA.LogInformation($"(A) Message {i}");
                        break;
                    case 1:
                        logB.LogInformation($"(B) Message {i}");
                        break;
                    case 2:
                        logC.LogInformation($"(C) Message {i}");
                        break;
                    case 3:
                        logD.LogInformation($"(D) Message {i}");
                        break;
                }
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Send_A_Discord_Message_As_File_If_Too_Long()
        {
            var rand = new Random();
            var sb = new StringBuilder();

            do
            {
                sb.Append((char) rand.Next('a', 'z'));

                if (rand.NextDouble() > 0.8)
                {
                    sb.Append(' ');
                }

                if (rand.NextDouble() > 0.96)
                {
                    sb.AppendLine();
                }
            } while (sb.Length < 2000);

            _logger.LogWarning(sb.ToString());

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
                var logger = GetLoggerFactoryWithDiscord("");

                Assert.IsTrue(false);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.GetType() == typeof(ArgumentNullException), ex.Message);
            }
        }

        [TestMethod]
        public void Should_Not_Log_Filtered_LogLevel()
        {
            try
            {
                var logger = GetLoggerFactoryWithDiscord(_configuration["DiscordWebhookUrl"], LogLevel.Warning)
                    .CreateLogger("FilteredLogger");

                logger.LogTrace("trace");
                logger.LogInformation("information");
                logger.LogWarning("warning");
                logger.LogError("error");
                logger.LogCritical("critical");

                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.GetType() == typeof(ArgumentNullException), ex.Message);
            }
        }
    }
}
