/* Copyright 2013-2015 MongoDB Inc.
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsernamePasswordCredential"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public UsernamePasswordCredential(string source, string username, SecureString password)
        {
            _source = Ensure.IsNotNullOrEmpty(source, nameof(source));
            _username = Ensure.IsNotNullOrEmpty(username, nameof(username));
            _password = Ensure.IsNotNull(password, nameof(password));
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
#if NET45
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
