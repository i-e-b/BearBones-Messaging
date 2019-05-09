using System.Configuration;
using BearBonesMessaging.RabbitMq;
using BearBonesMessaging.RabbitMq.RabbitMqManagement;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Integration.Tests.Helpers
{
	public class ConfigurationHelpers
	{
		public static RabbitMqQuery RabbitMqQueryWithConfigSettings()
		{
			var parts = ConfigurationManager.AppSettings["Messaging.Host"].Split('/');
			var hostUri = (parts.Length >= 1) ? (parts[0]) : ("localhost");
			var username = ConfigurationManager.AppSettings["ApiUsername"];
			var password = ConfigurationManager.AppSettings["ApiPassword"];
			var port = ConfigurationManager.AppSettings["Messaging.Port"];
			var vhost = (parts.Length >= 2) ? (parts[1]) : ("/");

            if (port == "15672")
            {
                return new RabbitMqQuery("http://" + hostUri + ":" + port, username, password, "testSalt", vhost);
            }
            return new RabbitMqQuery("https://" + hostUri + ":" + port, username, password, "testSalt", vhost);
        }

        public static RabbitMqConnection RabbitMqConnectionWithConfigSettings()
		{
			var parts = ConfigurationManager.AppSettings["Messaging.Host"].Split('/');
			var hostUri = (parts.Length >= 1) ? (parts[0]) : ("localhost");
			var vhost = (parts.Length >= 2 && parts[1].Length > 0) ? (parts[1]) : ("/");
            var username = ConfigurationManager.AppSettings["ApiUsername"];
            var password = ConfigurationManager.AppSettings["ApiPassword"];
            
            var port = ConfigurationManager.AppSettings["Messaging.Port"];
            var useSecure = (port != "15672");

            return new RabbitMqConnection(hostUri, 0, username, password, vhost, useSecure);
		}
        
        public static RabbitMqConnection RabbitMqConnectionWithConfigSettingsAndCustomCredentials(string username, string password)
        {
            var parts = ConfigurationManager.AppSettings["Messaging.Host"].Split('/');
            var hostUri = (parts.Length >= 1) ? (parts[0]) : ("localhost");
            var vhost = (parts.Length >= 2 && parts[1].Length > 0) ? (parts[1]) : ("/");
            
            var port = ConfigurationManager.AppSettings["Messaging.Port"];
            var useSecure = (port != "15672");

            return new RabbitMqConnection(hostUri, 0, username, password, vhost, useSecure);
        }

		static readonly IRabbitMqConnection conn;
		static ConfigurationHelpers()
		{
			conn = FreshConnectionFromAppConfig();
		}

		public static IRabbitMqConnection FreshConnectionFromAppConfig()
		{
			var parts = ConfigurationManager.AppSettings["Messaging.Host"].Split('/');
			var hostUri = (parts.Length >= 1) ? (parts[0]) : ("localhost");
			var vhost = (parts.Length >= 2 && parts[1].Length > 0) ? (parts[1]) : ("/");
            var username = ConfigurationManager.AppSettings["ApiUsername"];
            var password = ConfigurationManager.AppSettings["ApiPassword"];
            
            var port = ConfigurationManager.AppSettings["Messaging.Port"];
            var useSecure = (port != "15672");

			return new RabbitMqConnection(hostUri, 0, username, password, vhost, useSecure);
		}

		public static IChannelAction ChannelWithAppConfigSettings()
		{
			return new LongTermRabbitConnection(conn);
		}
	}
}
