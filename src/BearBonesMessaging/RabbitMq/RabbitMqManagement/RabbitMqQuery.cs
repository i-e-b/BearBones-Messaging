using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using SkinnyJson;

namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
    /// <summary>
	/// Default RMQ query
	/// </summary>
	public class RabbitMqQuery : IRabbitMqQuery
	{
		/// <summary>
		/// RabbitMQ cluster's management uri
		/// </summary>
		public Uri HostUri { get; }

		/// <summary>
		/// Virtual host to use, where applicable
		/// </summary>
		public string VirtualHost { get; }

		/// <summary>
		/// Log-in credentials for RabbitMQ management API
		/// </summary>
		public NetworkCredential ManagementCredentials { get; }

        /// <summary>
        /// A private key used for generating one-time limited user accounts
        /// </summary>
        public string CredentialSalt { get; }

		/// <summary>
		/// Use `MessagingBaseConfiguration` and get an IRabbitMqQuery from ObjectFactory.
		/// </summary>
		public RabbitMqQuery(Uri managementApiHost, NetworkCredential credentials, string credentialSalt)
		{
			HostUri = managementApiHost;
			ManagementCredentials = credentials;
            CredentialSalt = credentialSalt ?? throw new ArgumentNullException(nameof(credentialSalt));
		}
		
		/// <summary>
		/// Use `MessagingBaseConfiguration` and get an IRabbitMqQuery from ObjectFactory.
		/// </summary>
		public RabbitMqQuery(string hostUri, string username, string password, string credentialSalt, string virtualHost)
			: this(
                new Uri(hostUri ?? throw new ArgumentNullException(nameof(hostUri))),
                new NetworkCredential(username, password),
                credentialSalt
            )
        {
            if (virtualHost == null) virtualHost = "/";
			VirtualHost = virtualHost.StartsWith("/") ? virtualHost : ("/" + virtualHost);
		}

		/// <summary>
		/// List all Destination queue in the given virtual host.
		/// Equivalent to /api/queues/vhost
		/// </summary>
		public IRMQueue[] ListDestinations()
		{
            return Json.Defrost<IRMQueue[]>(Get("/api/queues" + VirtualHost));
		}
        
        /// <summary>
        /// List all dead-letter-queues on the system
        /// </summary>
        public IRMQueue[] GetDeadLetterStatus()
        {
            return Json.Defrost<IRMQueue[]>(
                    Get("/api/queues" + VirtualHost)
                )?.Where(q =>
                    q?.name?.StartsWith(MessagingBaseConfiguration.DeadLetterPrefix) == true
                ).ToArray();
        }

        /// <summary>
        /// List all nodes attached to the cluster.
        /// Equivalent to /api/nodes
        /// </summary>
        public IRMNode[] ListNodes()
		{
			return Json.Defrost<IRMNode[]>(Get("/api/nodes"));
		}

		/// <summary>
		/// List all Source exchanges in the given virtual host
		/// Equivalent to /api/exchanges/vhost
		/// </summary>
		public IRMExchange[] ListSources()
		{
			return Json.Defrost<IRMExchange[]>(Get("/api/exchanges" + VirtualHost));
		}

        /// <inheritdoc />
        public IRMBinding[] ListBindings()
        {
            var raw = Get("/api/bindings" + VirtualHost);
            return Json.Defrost<IRMBinding[]>(raw);
        }

        /// <summary>
        /// List all users in the system
        /// </summary>
        public IRMUser[] ListUsers()
        {
            return Json.Defrost<IRMUser[]>(Get("/api/users/"));
        }
        
        /// <summary>
        /// List all users in the system
        /// </summary>
        public IRMUser TryGetUser(string userName)
        {
            var raw = Get("/api/users/"+userName);
            if (raw == null) return null;
            return Json.Defrost<IRMUser>(raw);
        }
        
        /// <summary>
        /// Ensure that a user exists for the given app group, and return the authentication details for it.
        /// </summary>
        public NetworkCredential GetLimitedUser(string appGroup)
        {
            if (appGroup == null) throw new ArgumentNullException(nameof(appGroup));
            if (CredentialSalt == null) throw new Exception("Credential salt was not set. Refusing to create users.");

            var csalt = CredentialSalt;
            var expectedUser = HashName(appGroup, csalt);
            var expectedPass = GeneratePassword(appGroup, csalt);

            var existing = TryGetUser(expectedUser);
            if (existing == null)
            {
                CreateLimitedUserWithCreds(expectedPass, expectedUser);
            }

            return new NetworkCredential(expectedUser, expectedPass);
        }

        /// <summary>
        /// Delete a user from the system
        /// </summary>
        public bool DeleteUser(NetworkCredential credentials)
        {
            if (credentials == null || credentials.UserName == null) throw new Exception("Invalid user credentials");
            if (credentials.UserName == ManagementCredentials?.UserName) throw new Exception("Unacceptable user credentials");

            var existing = TryGetUser(credentials.UserName);
            if (existing == null) return false;

            if (existing.tags?.Contains("administrator") == true) throw new Exception("Unacceptable user credentials");

            return DeleteRequest("/api/users/" + credentials.UserName) != null;
        }

        private string GeneratePassword([NotNull]string appGroup, [NotNull]string credentialSalt)
        {
            var left = prospector32s(appGroup.ToCharArray(), (uint)appGroup.Length).ToString("X");
            var right = prospector32s(credentialSalt.ToCharArray(), (uint)credentialSalt.Length).ToString("X");
            return left + right;
        }

        private void CreateLimitedUserWithCreds(string expectedPass, string expectedUser)
        {
            // Create a user
            var pwHash = RabbitMqPasswordHelper.EncodePassword(expectedPass);
            var body = Json.Freeze(new {password_hash = pwHash, tags = ""});
            var response = Put("/api/users/" + expectedUser, body);
            if (response == null) throw new Exception("Failed to create new user");

            // Set write + read permissions, but no configure for the current VHost
            var encVhost = Uri.EscapeDataString(VirtualHost ?? "/");
            body = Json.Freeze(new {configure = "", write = ".*", read = ".*"}); // these are regexes to match resources
            response = Put("/api/permissions/" + encVhost + "/" + expectedUser, body);
            if (response == null) throw new Exception("Failed to give permissions to new user");

            var existing = TryGetUser(expectedUser);
            if (existing == null) throw new Exception("Failed to read new user");
        }

        string DeleteRequest(string endpoint)
        {
            if ( ! Uri.TryCreate(HostUri, endpoint, out var target)) return null;

            if (target == null) throw new ArgumentNullException(nameof(target));

            var request = (HttpWebRequest) WebRequest.Create(target);
            request.Method = "DELETE";
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Credentials = ManagementCredentials;

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null) throw new Exception("Failed to read response");
                        return new StreamReader(responseStream).ReadToEnd();
                    }
                }
            }
            catch (WebException)
            {
                return null;
            }
        }

		string Get(string endpoint)
		{
			Uri result;

			return Uri.TryCreate(HostUri, endpoint, out result) ? GetResponseString(result) : null;
		}

		string GetResponseString([NotNull] Uri target)
		{
            if (target == null) throw new ArgumentNullException(nameof(target));

			var request = (HttpWebRequest) WebRequest.Create(target);
			request.AutomaticDecompression = DecompressionMethods.GZip;
			request.Credentials = ManagementCredentials;

            // To test remotely, with test TLS certificates:
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null) throw new Exception("Failed to read response");
                        return new StreamReader(responseStream).ReadToEnd();
                    }
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("err: "+wex);
                return null;
            }
        }
        
        string Put(string endpoint, string body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            Uri result;

            return Uri.TryCreate(HostUri, endpoint, out result) ? PutString(result, body) : null;
        }

        string PutString([NotNull] Uri target, [NotNull]string body)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var request = (HttpWebRequest) WebRequest.Create(target);
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Credentials = ManagementCredentials;

            try
            {
                var buf = Encoding.UTF8.GetBytes(body);
                request.GetRequestStream().Write(buf, 0, buf.Length);

                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null) throw new Exception("Failed to read response");
                        return new StreamReader(responseStream).ReadToEnd();
                    }
                }
            }
            catch (WebException)
            {
                return null;
            }
        }


        private string HashName([NotNull] string name, [NotNull] string salt)
        {
            var saltedName = name + salt;
            return "OneTime_" + name + "_" + prospector32s(saltedName.ToCharArray(), (uint)saltedName.Length).ToString("X");
        }

        /// <summary>
        /// Low bias 32 bit hash
        /// </summary>
        static uint prospector32s([NotNull]char[] buf, uint key)
        {
            unchecked
            {
                uint hash = key;
                for (int i = 0; i < buf.Length; i++)
                {
                    hash += buf[i];
                    hash ^= hash >> 16;
                    hash *= 0x7feb352d;
                    hash ^= hash >> 15;
                    hash *= 0x846ca68b;
                    hash ^= hash >> 16;
                }
                hash ^= (uint)buf.Length;
                hash ^= hash >> 16;
                hash *= 0x7feb352d;
                hash ^= hash >> 15;
                hash *= 0x846ca68b;
                hash ^= hash >> 16;
                return hash + key;
            }
        }
    }
}