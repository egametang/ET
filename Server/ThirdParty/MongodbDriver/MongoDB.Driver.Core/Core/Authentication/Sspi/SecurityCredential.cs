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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace MongoDB.Driver.Core.Authentication.Sspi
{
    /// <summary>
    /// A wrapper around the SspiHandle structure specifically used as a credential handle.
    /// </summary>
    internal class SecurityCredential : SafeHandle
    {
        // fields
        internal SspiHandle _sspiHandle;

        // constructors
        public SecurityCredential()
            : base(IntPtr.Zero, true)
        {
            _sspiHandle = new SspiHandle();
        }

        // properties
        public override bool IsInvalid
        {
            get { return base.IsClosed || _sspiHandle.IsZero; }
        }

        // public methods
        public static SecurityCredential Acquire(SspiPackage package, string username, SecureString password)
        {
            long timestamp;

            var credential = new SecurityCredential();
#if NET45
            RuntimeHelpers.PrepareConstrainedRegions();
#endif
            try { }
            finally
            {
                uint result;
                if (password == null)
                {
                    result = NativeMethods.AcquireCredentialsHandle(
                        null,
                        package.ToString(),
                        SecurityCredentialUse.Outbound,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        0,
                        IntPtr.Zero,
                        ref credential._sspiHandle,
                        out timestamp);
                }
                else
                {
                    using(var authIdentity = new AuthIdentity(username, password))
                    {
                        // TODO: make this secure by using SecurePassword
                        result = NativeMethods.AcquireCredentialsHandle(
                            null,
                            package.ToString(),
                            SecurityCredentialUse.Outbound,
                            IntPtr.Zero,
                            authIdentity,
                            0,
                            IntPtr.Zero,
                            ref credential._sspiHandle,
                            out timestamp);
                    }
                }
                if (result != NativeMethods.SEC_E_OK)
                {
                    credential.SetHandleAsInvalid();
                    throw NativeMethods.CreateException(result, "Unable to acquire credential.");
                }
            }
            return credential;
        }

        // protected methods
        /// <summary>
        /// When overridden in a derived class, executes the code required to free the handle.
        /// </summary>
        /// <returns>
        /// true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false. In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.
        /// </returns>
        protected override bool ReleaseHandle()
        {
            return NativeMethods.FreeCredentialsHandle(ref _sspiHandle) == 0;
        }
    }
}
