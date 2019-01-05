/* Copyright 2013-present MongoDB Inc.
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
using System.Runtime.InteropServices;
using System.Security;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// Represents a username/password credential.
    /// </summary>
    public sealed class UsernamePasswordCredential
    {
        // fields
        private readonly Lazy<SecureString> _saslPreppedPassword;
        private string _source;
        private SecureString _password;
        private string _username;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UsernamePasswordCredential"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public UsernamePasswordCredential(string source, string username, string password)
            : this(source, username, ConvertPasswordToSecureString(password))
        {
            // Compute saslPreppedPassword immediately and store it securely while the password is already in
            // managed memory. We don't create a closure over the password so that it will hopefully get 
            // garbage-collected sooner rather than later.
            var saslPreppedPassword = ConvertPasswordToSecureString(SaslPrepHelper.SaslPrepStored(password));
            _saslPreppedPassword = new Lazy<SecureString>(() => saslPreppedPassword);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsernamePasswordCredential"/> class.
        /// Less secure when used in conjunction with SCRAM-SHA-256, due to the need to store the password in a managed
        /// string in order to SaslPrep it.
        /// See <a href="https://github.com/mongodb/specifications/blob/master/source/auth/auth.rst#scram-sha-256">Driver Authentication: SCRAM-SHA-256</a>
        /// for additional details.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public UsernamePasswordCredential(string source, string username, SecureString password)
        {
            _source = Ensure.IsNotNullOrEmpty(source, nameof(source));
            _username = Ensure.IsNotNullOrEmpty(username, nameof(username));
            _password = Ensure.IsNotNull(password, nameof(password));
            // defer computing the saslPreppedPassword until we need to since this will leak the password into managed
            // memory
            _saslPreppedPassword = new Lazy<SecureString>(
                () => ConvertPasswordToSecureString(SaslPrepHelper.SaslPrepStored(GetInsecurePassword())));
        }

        // properties
        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public SecureString Password
        {
            get { return _password; }
        }

        /// <summary>
        /// Gets the the SASLprepped password.
        /// May create a cleartext copy of the password in managed memory the first time it is accessed.
        /// Use only as needed e.g. for SCRAM-SHA-256.
        /// </summary> 
        /// <returns>The SASLprepped password.</returns>
        public SecureString SaslPreppedPassword
        {
            get { return _saslPreppedPassword.Value; }
        }
        
        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source
        {
            get { return _source; }
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string Username
        {
            get { return _username; }
        }

        // methods
        /// <summary>
        /// Gets the password (converts the password from a SecureString to a regular string).
        /// </summary>
        /// <returns>The password.</returns>
        public string GetInsecurePassword()
        {
            if (_password.Length == 0)
            {
                return "";
            }
            else
            {
#if NET452
                var passwordIntPtr = Marshal.SecureStringToGlobalAllocUnicode(_password);
#else
                var passwordIntPtr = SecureStringMarshal.SecureStringToGlobalAllocUnicode(_password);
#endif
                try
                {
                    return Marshal.PtrToStringUni(passwordIntPtr, _password.Length);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(passwordIntPtr);
                }
            }
        }

        private static SecureString ConvertPasswordToSecureString(string password)
        {
            var secureString = new SecureString();
            foreach (var c in password)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();
            return secureString;
        }
    }
}
