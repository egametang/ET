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

namespace MongoDB.Driver.Core.Authentication.Sspi
{
    /// <summary>
    /// Flag for the AuthIdentity structure.
    /// </summary>
    internal enum AuthIdentityFlag
    {
        /// <summary>
        /// SEC_WINNT_AUTH_IDENTITY_ANSI
        /// </summary>
        Ansi = 0x1,
        /// <summary>
        /// SEC_WINNT_AUTH_IDENTITY_UNICODE
        /// </summary>
        Unicode = 0x2
    }
}
