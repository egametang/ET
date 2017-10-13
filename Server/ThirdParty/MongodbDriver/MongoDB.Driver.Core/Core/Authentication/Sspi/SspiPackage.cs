﻿/* Copyright 2010-2016 MongoDB Inc.
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
    /// This is represented as a string in AcquireCredentialsHandle. This value will have .ToString() called on it.
    /// </summary>
    internal enum SspiPackage
    {
        /// <summary>
        /// Kerberos
        /// </summary>
        Kerberos
    }
}
