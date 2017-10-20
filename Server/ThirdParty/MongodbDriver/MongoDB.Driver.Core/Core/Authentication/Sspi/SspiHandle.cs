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
#if NET45
using System.Runtime.ConstrainedExecution;
#endif
using System.Runtime.InteropServices;

namespace MongoDB.Driver.Core.Authentication.Sspi
{
    /// <summary>
    /// A SecHandle structure.
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa380495(v=vs.85).aspx
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SspiHandle
    {
        // private fields
        private IntPtr _hi;
        private IntPtr _low;

        // public properties
        /// <summary>
        /// Gets a value indicating whether this instance is zero.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is zero; otherwise, <c>false</c>.
        /// </value>
        public bool IsZero
        {
            get
            {
                if (_hi != IntPtr.Zero)
                {
                    return false;
                }
                else
                {
                    return _low == IntPtr.Zero;
                }
            }
        }

        // public methods
        /// <summary>
        /// Sets to invalid.
        /// </summary>
#if NET45
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
        public void SetToInvalid()
        {
            _hi = IntPtr.Zero;
            _low = IntPtr.Zero;
        }
    }
}
