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
    /// Represents a source of chunks.
    /// </summary>
    public interface IBsonChunkSource : IDisposable
    {
        /// <summary>
        /// Gets the chunk.
        /// </summary>
        /// <remarks>The chunk source is free to return a larger or smaller chunk than requested.</remarks>
        /// <param name="requestedSize">Size of the requested.</param>
        /// <returns>A chunk.</returns>
        IBsonChunk GetChunk(int requestedSize);
    }
}
