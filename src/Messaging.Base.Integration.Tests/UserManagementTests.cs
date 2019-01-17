using System;
using System.Linq;
using System.Net;
using BearBonesMessaging;
using BearBonesMessaging.RabbitMq.RabbitMqManagement;
using BearBonesMessaging.Routing;
using Example.Types;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute

namespace Messaging.Base.Integration.Tests
{
    [TestFixture]
    public class UserManagementTests
    {
        private IMessagingBase _messaging;
        private RabbitMqQuery _query;
        private MessagingBaseConfiguration _config;

        [SetUp]
        public void setup()
        {
            _config = new MessagingBaseConfiguration()
                .WithDefaults()
                .UsesSecureConnections() // If you don't have HTTPS set up, comment out this line
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings());

            _messaging = _config.GetMessagingBase();
            
            _query = ConfigurationHelpers.RabbitMqQueryWithConfigSettings();
        }

        [Test, Explicit("_Requires a clean VHost")]
        public void can_add_and_remove_users__and_query_for_their_existence ()
        {
            var baseUsers = _query.ListUsers().Where(u => !u.tags.Contains("administrator")).ToList();
            Assert.That(baseUsers.Count, Is.Zero, "There were existing non-admin users. This test can't run on production systems. Delete old users if this is a test system.");

            var appGroup = "MySampleGroup";
            var credentials = _query.GetLimitedUser(appGroup);
            Assert.That(credentials, Is.Not.Null, "Failed to get a temporary user");
            Assert.That(credentials.UserName, Is.Not.Null.Or.Empty, "User name was invalid");
            Assert.That(credentials.Password, Is.Not.Null.Or.Empty, "Password hash was invalid");
            
            baseUsers = _query.ListUsers().Where(u => !u.tags.Contains("administrator")).ToList();
            Assert.That(baseUsers.Count, Is.Not.Zero, "I thought I created a user, but I can't find it in the list");

            var deleted = _query.DeleteUser(credentials);
            Assert.That(deleted, Is.True, "Failed to delete the user");
            
            baseUsers = _query.ListUsers().Where(u => !u.tags.Contains("administrator")).ToList();
            Assert.That(baseUsers.Count, Is.Zero, "I deleted my test user, but it's still on the list");
        }

        [Test]
        public void can_NOT_delete_the_user_specified_for_management ()
        {
            try {
                _query.DeleteUser(_query.ManagementCredentials);
                Assert.Fail();
            } catch {
                Assert.Pass();
            }
        }

        [Test, Explicit("Needs a special 'test' administrator user to be set up")]
        public void can_NOT_delete_administrator_users ()
        {
            try {
                _query.DeleteUser(new NetworkCredential("test", "whatever"));
                Assert.Fail();
            } catch {
                Assert.Pass();
            }
        }

        [Test]
        public void a_limited_user_can_NOT_add_routing ()
        {
            var credentials = _query.GetLimitedUser("test_permissions");
            
            var config = new MessagingBaseConfiguration()
                .WithDefaults()
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettingsAndCustomCredentials(
                    credentials.UserName,
                    credentials.Password
                    ));

            var limitedConn = config.GetMessagingBase();

            bool ok = false;
            try {
                // Should not be able to do this:
                limitedConn.CreateDestination<IMetadataFile>("test_permissions", Expires.Never);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                ok = ex.Message.Contains("ACCESS_REFUSED");
            }
            
            _query.DeleteUser(credentials);
            Assert.That(ok, Is.True, "Create destination passed, but should have been blocked");
        }

        [Test]
        public void a_limited_user_can_NOT_add_a_new_destination ()
        {
            // Limited user
            var credentials = _query.GetLimitedUser("test_permissions");

            // connect using the limited user
            var config = new MessagingBaseConfiguration()
                .WithDefaults()
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettingsAndCustomCredentials(
                    credentials.UserName,
                    credentials.Password
                ));

            var limitedConn = config.GetMessagingBase();
            
            bool ok = false;
            try
            {
                limitedConn.CreateDestination<IMetadataFile>("test_permissions", Expires.Never);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ok = ex.Message.Contains("ACCESS_REFUSED");
            }

            _query.DeleteUser(credentials);
            Assert.That(ok, Is.True, "QueueDelete passed, but should have been blocked");
        }

        [Test]
        public void a_limited_user_can_NOT_delete_an_existing_destination ()
        {
            // Admin connection to make the routing
            _messaging.CreateDestination<IMetadataFile>("test_permissions", Expires.Never);
            
            // Limited user to use it
            var credentials = _query.GetLimitedUser("test_permissions");
            var limitedConn = ConfigurationHelpers.RabbitMqConnectionWithConfigSettingsAndCustomCredentials(
                credentials.UserName,
                credentials.Password
            );

            bool ok = false;
            try
            {
                limitedConn.WithChannel(conn => conn.QueueDelete("test_permissions", false, false));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ok = ex.Message.Contains("ACCESS_REFUSED");
            }

            _query.DeleteUser(credentials);
            Assert.That(ok, Is.True, "QueueDelete passed, but should have been blocked");
        }

        [Test]
        public void a_limited_user_CAN_send_and_receive_messages_on_existing_routes ()
        {
            // Admin connection to make the routing
            _messaging.CreateDestination<IMetadataFile>("test_permissions", Expires.Never);

            // Limited user to use it
            var credentials = _query.GetLimitedUser("test_permissions");

            // connect using the limited user
            var config = new MessagingBaseConfiguration()
                .WithDefaults()
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettingsAndCustomCredentials(
                    credentials.UserName,
                    credentials.Password
                ));

            var limitedConn = config.GetMessagingBase();
            
            // Do a round-trip on the limited account with the admin's routing
            limitedConn.SendMessage(new SuperMetadata{ Contents = "Hello" });
            var result = limitedConn.GetMessage<IMetadataFile>("test_permissions");

            // Check it worked
            Assert.That(result, Is.Not.Null, "Message did not get through");
            Assert.That(result.Contents, Is.EqualTo("Hello"), "Got the wrong message");
            
            _query.DeleteUser(credentials);
        }

        [TearDown]
        public void teardown()
        {
            _config.Get<IMessageRouter>().RemoveRouting(n=>true);
        }
    }
}