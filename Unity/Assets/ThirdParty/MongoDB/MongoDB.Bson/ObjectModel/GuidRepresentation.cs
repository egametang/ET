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
namespace MongoDB.Bson
{
    /// <summary>
    /// Represents the representation to use when converting a Guid to a BSON binary value.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public enum GuidRepresentation
    {
        /// <summary>
        /// The representation for Guids is unspecified, so conversion between Guids and Bson binary data is not possible.
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// Use the new standard representation for Guids (binary subtype 4 with bytes in network byte order).
        /// </summary>
        Standard,
        /// <summary>
        /// Use the representation used by older versions of the C# driver (including most community provided C# drivers).
        /// </summary>
        CSharpLegacy,
        /// <summary>
        /// Use the representation used by older versions of the Java driver.
        /// </summary>
        JavaLegacy,
        /// <summary>
        /// Use the representation used by older versions of the Python driver.
        /// </summary>
        PythonLegacy
    }
}
