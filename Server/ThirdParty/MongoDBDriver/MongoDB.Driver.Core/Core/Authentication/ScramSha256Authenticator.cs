/* Copyright 2018–present MongoDB Inc.
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Authentication.Vendored;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

// use our vendored version of Rfc2898DeriveBytes because .NET Standard 1.5 and .NET Framework 4.5 do not support
// a version of Rfc2898DeriveBytes that allows us to specify to hash algorithm to be used
using Rfc2898DeriveBytes = MongoDB.Driver.Core.Authentication.Vendored.Rfc2898DeriveBytes;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// A SCRAM-SHA256 SASL authenticator.
    /// In .NET Standard, this class does not normalize the password in the credentials, so non-ASCII
    /// passwords may not work unless they are normalized into Unicode Normalization Form KC beforehand.
    /// </summary>
    public sealed class ScramSha256Authenticator : ScramShaAuthenticator
    {
        // static properties
        /// <summary>
        /// Gets the name of the mechanism.
        /// </summary>
        /// <value>
        /// The name of the mechanism.
        /// </value>
        public static string MechanismName => "SCRAM-SHA-256";

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ScramSha256Authenticator"/> class.
        /// In .NET Standard, this class does not normalize the password in <paramref name="credential"/>, so non-ASCII
        /// passwords may not work unless they are normalized into Unicode Normalization Form KC beforehand.
        /// </summary>
        /// <param name="credential">The credential.</param>
        public ScramSha256Authenticator(UsernamePasswordCredential credential)
            : this(credential, new DefaultRandomStringGenerator())
        {
        }

        internal ScramSha256Authenticator(UsernamePasswordCredential credential, IRandomStringGenerator randomStringGenerator)
            : base(credential, HashAlgorithmName.SHA256, randomStringGenerator, H256, Hi256, Hmac256)
        {
        }

        private static byte[] H256(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        private static byte[] Hi256(UsernamePasswordCredential credential, byte[] salt, int iterations)
        {
#if NET452
            var passwordIntPtr = Marshal.SecureStringToGlobalAllocUnicode(credential.SaslPreppedPassword);
#else
            var passwordIntPtr = SecureStringMarshal.SecureStringToGlobalAllocUnicode(credential.SaslPreppedPassword);
#endif
            try
            {
                var passwordChars = new char[credential.SaslPreppedPassword.Length];
                var passwordCharsHandle = GCHandle.Alloc(passwordChars, GCHandleType.Pinned);
                try
                {
                    Marshal.Copy(passwordIntPtr, passwordChars, 0, credential.SaslPreppedPassword.Length);
                    return Hi256(passwordChars, salt, iterations);
                }
                finally
                {
                    Array.Clear(passwordChars, 0, passwordChars.Length);
                    passwordCharsHandle.Free();
                }
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(passwordIntPtr);
            }
        }

        private static byte[] Hi256(char[] passwordChars, byte[] salt, int iterations)
        {
            var passwordBytes = new byte[Utf8Encodings.Strict.GetByteCount(passwordChars)];
            var passwordBytesHandle = GCHandle.Alloc(passwordBytes, GCHandleType.Pinned);
            try
            {
                Utf8Encodings.Strict.GetBytes(passwordChars, 0, passwordChars.Length, passwordBytes, 0);

                // 32 is the length of output of a sha-256 hmac
                return new Rfc2898DeriveBytes(passwordBytes, salt, iterations, HashAlgorithmName.SHA256).GetBytes(32); 				
            }
            finally
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
                passwordBytesHandle.Free();
            }
        }

        private static byte[] Hmac256(UTF8Encoding encoding, byte[] data, string key)
        {
            using (var hmac = new HMACSHA256(data))
            {
                return hmac.ComputeHash(encoding.GetBytes(key));
            }
        }
    }
}
