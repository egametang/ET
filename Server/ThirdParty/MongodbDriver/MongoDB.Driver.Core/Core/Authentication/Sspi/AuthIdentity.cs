/* Copyright 2010-2016 MongoDB Inc.
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

namespace MongoDB.Driver.Core.Authentication.Sspi
{
    /// <summary>
    /// SEC_WINNT_AUTH_IDENTITY
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal sealed class AuthIdentity : IDisposable
    {
        // public fields
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Username;
        public int UsernameLength;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Domain;
        public int DomainLength;
        public IntPtr Password;
        public int PasswordLength;
        public AuthIdentityFlag Flags;

        // constructors
        public AuthIdentity(string username, SecureString password)
        {
            Username = null;
            UsernameLength = 0;
            if (!string.IsNullOrEmpty(username))
            {
                Username = username;
                UsernameLength = username.Length;
            }

            Password = IntPtr.Zero;
            PasswordLength = 0;
            
            if (password != null && password.Length > 0)
            {
#if NET45
                Password = Marshal.SecureStringToGlobalAllocUnicode(password);
#else
                Password = SecureStringMarshal.SecureStringToGlobalAllocUnicode(password);
#endif
                PasswordLength = password.Length;
            }

            Domain = null;
            DomainLength = 0;

            Flags = AuthIdentityFlag.Unicode;
        }

        ~AuthIdentity()
        {
            Dispose(false);
        }

        // public methods
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // private methods
        private void Dispose(bool disposing)
        {
            if (Password != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(Password);
                Password = IntPtr.Zero;
            }
        }
    }
}
