/* Copyright 2010-2015 MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a chunk of bytes.
    /// </summary>
    public interface IBsonChunk : IDisposable
    {
        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <value>
        /// The bytes.
        /// </value>
        ArraySegment<byte> Bytes { get; }

        /// <summary>
        /// Returns a new reference to the same chunk that can be independently disposed.
        /// </summary>
        /// <returns>A new reference to the same chunk.</returns>
        IBsonChunk Fork();
    }
}
