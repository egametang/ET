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

namespace MongoDB.Driver.Core.Authentication.Sspi
{
    /// <summary>
    /// Flags for InitiateSecurityContext.
    /// </summary>
    /// <remarks>
    /// See the fContextReq parameter at 
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa375507(v=vs.85).aspx
    /// </remarks>
    [Flags]
    internal enum SspiContextFlags
    {
        None = 0,
        /// <summary>
        /// ISC_REQ_MUTUAL_AUTH
        /// </summary>
        MutualAuth = 0x2,
        /// <summary>
        /// ISC_REQ_CONFIDENTIALITY
        /// </summary>
        Confidentiality = 0x10,
        /// <summary>
        /// ISC_REQ_INTEGRITY
        /// </summary>
        InitIntegrity = 0x10000
    }
}
