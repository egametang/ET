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
    /// Represents the type of a BSON element.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public enum BsonType
    {
        /// <summary>
        /// Not a real BSON type. Used to signal the end of a document.
        /// </summary>
        EndOfDocument = 0x00, // no values of this type exist, it marks the end of a document
        /// <summary>
        /// A BSON double.
        /// </summary>
        Double = 0x01,
        /// <summary>
        /// A BSON string.
        /// </summary>
        String = 0x02,
        /// <summary>
        /// A BSON document.
        /// </summary>
        Document = 0x03,
        /// <summary>
        /// A BSON array.
        /// </summary>
        Array = 0x04,
        /// <summary>
        /// BSON binary data.
        /// </summary>
        Binary = 0x05,
        /// <summary>
        /// A BSON undefined value.
        /// </summary>
        Undefined = 0x06,
        /// <summary>
        /// A BSON ObjectId.
        /// </summary>
        ObjectId = 0x07,
        /// <summary>
        /// A BSON bool.
        /// </summary>
        Boolean = 0x08,
        /// <summary>
        /// A BSON DateTime.
        /// </summary>
        DateTime = 0x09,
        /// <summary>
        /// A BSON null value.
        /// </summary>
        Null = 0x0a,
        /// <summary>
        /// A BSON regular expression.
        /// </summary>
        RegularExpression = 0x0b,
        /// <summary>
        /// BSON JavaScript code.
        /// </summary>
        JavaScript = 0x0d,
        /// <summary>
        /// A BSON symbol.
        /// </summary>
        Symbol = 0x0e,
        /// <summary>
        /// BSON JavaScript code with a scope (a set of variables with values).
        /// </summary>
        JavaScriptWithScope = 0x0f,
        /// <summary>
        /// A BSON 32-bit integer.
        /// </summary>
        Int32 = 0x10,
        /// <summary>
        /// A BSON timestamp.
        /// </summary>
        Timestamp = 0x11,
        /// <summary>
        /// A BSON 64-bit integer.
        /// </summary>
        Int64 = 0x12,
        /// <summary>
        /// A BSON 128-bit decimal.
        /// </summary>
        Decimal128 = 0x13,
        /// <summary>
        /// A BSON MinKey value.
        /// </summary>
        MinKey = 0xff,
        /// <summary>
        /// A BSON MaxKey value.
        /// </summary>
        MaxKey = 0x7f
    }
}
