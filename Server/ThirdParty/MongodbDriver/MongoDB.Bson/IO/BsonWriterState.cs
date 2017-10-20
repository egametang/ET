/* Copyright 2010-2014 MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents the state of a BsonWriter.
    /// </summary>
    public enum BsonWriterState
    {
        /// <summary>
        /// The initial state.
        /// </summary>
        Initial,
        /// <summary>
        /// The writer is positioned to write a name.
        /// </summary>
        Name,
        /// <summary>
        /// The writer is positioned to write a value.
        /// </summary>
        Value,
        /// <summary>
        /// The writer is positioned to write a scope document (call WriteStartDocument to start writing the scope document).
        /// </summary>
        ScopeDocument,
        /// <summary>
        /// The writer is done.
        /// </summary>
        Done,
        /// <summary>
        /// The writer is closed.
        /// </summary>
        Closed
    }
}
