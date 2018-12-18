using System;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
    /// <summary>
    /// Generates password hashes that RMQ will understand.
    /// This means we don't need to send passwords over the wire.
    /// </summary>
    public static class RabbitMqPasswordHelper
    {
        /// <summary>
        /// Encode a password for storage in RabbitMQ. Uses SHA256.
        /// </summary>
        public static string EncodePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            using (var rand = RandomNumberGenerator.Create())
            using (var sha256 = SHA256.Create())
            {
                if (rand == null) throw new Exception("System error generating cPRNG");
                if (sha256 == null) throw new Exception("System error generating SHA256 hasher");
                var salt = new byte[4];

                rand.GetBytes(salt);

                var saltedPassword = MergeByteArray(salt, Encoding.UTF8.GetBytes(password));
                var saltedPasswordHash = sha256.ComputeHash(saltedPassword);

                return Convert.ToBase64String(MergeByteArray(salt, saltedPasswordHash));
            }
        }

        [NotNull]private static byte[] MergeByteArray([NotNull]byte[] array1, [NotNull]byte[] array2)
        {
            var merge = new byte[array1.Length + array2.Length];
            array1.CopyTo(merge, 0);
            array2.CopyTo(merge, array1.Length);
            return merge;
        }
    }
}