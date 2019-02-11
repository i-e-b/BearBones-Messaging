using BearBonesMessaging;
using BearBonesMessaging.RabbitMq.RabbitMqManagement;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;

namespace Messaging.Base.Integration.Tests
{
    [TestFixture]
    public class ConfigurationTests
    {
        private MessagingBaseConfiguration _config;

        [Test]
        public void ssl_relaxed_strictness_settings_are_persistent ()
        {
            _config = new MessagingBaseConfiguration()
                .WithDefaults()
                .UsesSecureConnections(SslConnectionStrictness.Relaxed)
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings())
                .WithRabbitManagement("localhost", 0, "", "", "/", "");


            var firstInstance = (RabbitMqQuery)(_config.GetManagement());
            var secondInstance = (RabbitMqQuery)(_config.GetManagement());

            Assert.That(firstInstance?.AcceptInvalidSsl, Is.True, "First instance was strict, but expected relaxed");
            Assert.That(secondInstance?.AcceptInvalidSsl, Is.True, "First instance was strict, but expected relaxed");
        }

        [Test]
        public void ssl_strict_strictness_settings_are_persistent ()
        {
            _config = new MessagingBaseConfiguration()
                .WithDefaults()
                .UsesSecureConnections(SslConnectionStrictness.Strict)
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings())
                .WithRabbitManagement("localhost", 0, "", "", "/", "");

            var firstInstance = (RabbitMqQuery)_config.GetManagement();
            var secondInstance = (RabbitMqQuery)_config.GetManagement();

            Assert.That(firstInstance?.AcceptInvalidSsl, Is.False, "First instance was relaxed, but expected strict");
            Assert.That(secondInstance?.AcceptInvalidSsl, Is.False, "First instance was relaxed, but expected strict");
        }

        [Test]
        public void ssl_defaul_strictness_settings_are_set_to_strict ()
        {
            _config = new MessagingBaseConfiguration()
                .WithDefaults()
                .UsesSecureConnections() // default
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings())
                .WithRabbitManagement("localhost", 0, "", "", "/", "");

            var firstInstance = (RabbitMqQuery)_config.GetManagement();
            var secondInstance = (RabbitMqQuery)_config.GetManagement();
            
            Assert.That(firstInstance?.AcceptInvalidSsl, Is.False, "First instance was relaxed, but expected strict");
            Assert.That(secondInstance?.AcceptInvalidSsl, Is.False, "First instance was relaxed, but expected strict");
        }
    }
}