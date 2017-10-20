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
    /// Types for the SecurityBuffer structure.
    /// </summary>
    internal enum SecurityBufferType
    {
        /// <summary>
        /// SECBUFFER_VERSION
        /// </summary>
        Version = 0,
        /// <summary>
        /// SECBUFFER_EMPTY
        /// </summary>
        Empty = 0,
        /// <summary>
        /// SECBUFFER_DATA
        /// </summary>
        Data = 1,
        /// <summary>
        /// SECBUFFER_TOKEN
        /// </summary>
        Token = 2,
        /// <summary>
        /// SECBUFFER_PADDING
        /// </summary>
        Padding = 9,
        /// <summary>
        /// SECBUFFER_STREAM
        /// </summary>
        Stream = 10
    }
}
