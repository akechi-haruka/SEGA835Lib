using System;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Logging.MEXNet35;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Net35Test {
    public class Net35Test {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void T01_TestFramework() {
            Assert.Pass(Environment.Version.ToString());
        }

        private int logMessageCount;

        [Test]
        public void T02_TestLogging() {
            LogManager.Initialize(LoggerFactory.Create(_ => { }));
            LogManager.AddLegacyCallback(OnLogMessage);

            ILogger log = LogManager.GetOrCreate("test");
            log.LogInformation("test1");
            log.LogTrace("test2");
            log.LogError("test3");

            Assert.That(logMessageCount, Is.EqualTo(3));
        }

        private void OnLogMessage(string key, string formattedMessage, LogLevel logLevel, EventId eventId, Exception exception) {
            Console.WriteLine(formattedMessage);
            logMessageCount++;
        }
    }
}