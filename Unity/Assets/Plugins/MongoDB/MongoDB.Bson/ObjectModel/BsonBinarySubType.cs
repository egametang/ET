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
    /// Represents the binary data subtype of a BsonBinaryData.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public enum BsonBinarySubType
    {
        /// <summary>
        /// Binary data.
        /// </summary>
        Binary = 0x00,
        /// <summary>
        /// A function.
        /// </summary>
        Function = 0x01,
        /// <summary>
        /// Obsolete binary data subtype (use Binary instead).
        /// </summary>
        [Obsolete("Use Binary instead")]
        OldBinary = 0x02,
        /// <summary>
        /// A UUID in a driver dependent legacy byte order.
        /// </summary>
        UuidLegacy = 0x03,
        /// <summary>
        /// A UUID in standard network byte order.
        /// </summary>
        UuidStandard = 0x04,
        /// <summary>
        /// An MD5 hash.
        /// </summary>
        MD5 = 0x05,
        /// <summary>
        /// User defined binary data.
        /// </summary>
        UserDefined = 0x80
    }
}
