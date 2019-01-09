using BearBonesMessaging;
using BearBonesMessaging.RabbitMq.RabbitMqManagement;
using BearBonesMessaging.Routing;
using Example.Types;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Integration.Tests
{
    [TestFixture]
    public class RoutingInspectionTests
    {
        private IMessagingBase _messaging;
        private RabbitMqQuery _query;
        private MessagingBaseConfiguration _config;

        [SetUp]
        public void setup()
        {
            _config = new MessagingBaseConfiguration()
                .WithDefaults()
                .UsesSecureConnections()
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings());

            _messaging = _config.GetMessagingBase();
            
            _query = ConfigurationHelpers.RabbitMqQueryWithConfigSettings();
        }

        [Test]
        public void can_inspect_all_exchanges_queues_and_bindings_of_a_running_system ()
        {
            _messaging.CreateDestination<IMetadataFile>("Test_Destination_1", Expires.Never);
            _messaging.CreateDestination<IMetadataFile>("Test_Destination_2", Expires.Never);

            var bindings = _query.ListBindings();
            Assert.That(bindings.Length, Is.GreaterThanOrEqualTo(6), "Too few bindings");
            
            var destinations = _query.ListDestinations();
            Assert.That(destinations.Length, Is.GreaterThanOrEqualTo(2), "Too few bindings");
            
            var sources = _query.ListSources();
            Assert.That(sources.Length, Is.GreaterThanOrEqualTo(4), "Too few exchanges");
        }
        

        [TearDown]
        public void teardown()
        {
            _config.Get<IMessageRouter>().RemoveRouting(n=>true);
        }
    }
}