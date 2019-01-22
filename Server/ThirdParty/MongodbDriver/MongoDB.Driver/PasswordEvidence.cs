/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver
{
    /// <summary>
    /// Evidence of a MongoIdentity via a shared secret.
    /// </summary>
    public sealed class PasswordEvidence : MongoIdentityEvidence
    {
        // private fields
        private readonly SecureString _securePassword;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordEvidence" /> class.
        /// Less secure when used in conjunction with SCRAM-SHA-256, due to the need to store the password in a managed
        /// string in order to SaslPrep it.
        /// See <a href="https://github.com/mongodb/specifications/blob/master/source/auth/auth.rst#scram-sha-256">Driver Authentication: SCRAM-SHA-256</a>
        /// for additional details.
        /// </summary>
        /// <param name="password">The password.</param>
        public PasswordEvidence(SecureString password)
        {
            Ensure.IsNotNull(password, nameof(password));
            _securePassword = password.Copy();
            _securePassword.MakeReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordEvidence" /> class.
        /// </summary>
        /// <param name="password">The password.</param>
        public PasswordEvidence(string password)
        {
            Ensure.IsNotNull(password, nameof(password));
            _securePassword = CreateSecureString(password);
        }

        // public properties
        /// <summary>
        /// Gets the password.
        /// </summary>
        public SecureString SecurePassword
        {
            get { return _securePassword; }
        }

        // public methods
        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="rhs">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }

            using (var lhsDecryptedPassword = new DecryptedSecureString(_securePassword))
            using (var rhsDecryptedPassword = new DecryptedSecureString(((PasswordEvidence)rhs)._securePassword))
            {
                return lhsDecryptedPassword.GetChars().SequenceEqual(rhsDecryptedPassword.GetChars());
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            using (var decryptedPassword = new DecryptedSecureString(_securePassword))
            {
                return new Hasher().HashStructElements(decryptedPassword.GetChars()).GetHashCode();
            }
        }

        // internal methods
        /// <summary>
        /// Computes the MONGODB-CR password digest.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The MONGODB-CR password digest.</returns>
        [Obsolete("MONGODB-CR was replaced by SCRAM-SHA-1 in MongoDB 3.0, and is now deprecated.")]
        internal string ComputeMongoCRPasswordDigest(string username)
        {
            using (var md5 = MD5.Create())
            using (var decryptedPassword = new DecryptedSecureString(_securePassword))
            {
                var encoding = Utf8Encodings.Strict;
                var prefixBytes = encoding.GetBytes(username + ":mongo:");
                var hash = ComputeHash(md5, prefixBytes, decryptedPassword.GetUtf8Bytes());
                return BsonUtils.ToHexString(hash);
            }
        }

        // private static methods
        private static SecureString CreateSecureString(string value)
        {
            var secureString = new SecureString();
            foreach (var c in value)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();
            return secureString;
        }

        private static byte[] ComputeHash(HashAlgorithm algorithm, byte[] prefixBytes, byte[] passwordBytes)
        {
            var buffer = new byte[prefixBytes.Length + passwordBytes.Length];
            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Buffer.BlockCopy(prefixBytes, 0, buffer, 0, prefixBytes.Length);
                Buffer.BlockCopy(passwordBytes, 0, buffer, prefixBytes.Length, passwordBytes.Length);

                return algorithm.ComputeHash(buffer);
            }
            finally
            {
                Array.Clear(buffer, 0, buffer.Length);
                bufferHandle.Free();
            }
        }
    }
}
