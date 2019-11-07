﻿/* Copyright 2010-present MongoDB Inc.
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
    /// Flags for EncryptMessage.
    /// </summary>
    /// <remarks>
    /// See the fQOP parameter at 
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa375378(v=vs.85).aspx.
    /// </remarks>
    internal enum EncryptQualityOfProtection : uint
    {
        /// <summary>
        /// SECQOP_WRAP_NO_ENCRYPT
        /// </summary>
        WrapNoEncrypt = 0x80000001
    }
}
