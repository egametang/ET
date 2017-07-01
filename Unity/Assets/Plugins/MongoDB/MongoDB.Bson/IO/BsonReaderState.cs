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
    /// Represents the state of a reader.
    /// </summary>
    public enum BsonReaderState
    {
        /// <summary>
        /// The initial state.
        /// </summary>
        Initial,
        /// <summary>
        /// The reader is positioned at the type of an element or value.
        /// </summary>
        Type,
        /// <summary>
        /// The reader is positioned at the name of an element.
        /// </summary>
        Name,
        /// <summary>
        /// The reader is positioned at a value.
        /// </summary>
        Value,
        /// <summary>
        /// The reader is positioned at a scope document.
        /// </summary>
        ScopeDocument,
        /// <summary>
        /// The reader is positioned at the end of a document.
        /// </summary>
        EndOfDocument,
        /// <summary>
        /// The reader is positioned at the end of an array.
        /// </summary>
        EndOfArray,
        /// <summary>
        /// The reader has finished reading a document.
        /// </summary>
        Done,
        /// <summary>
        /// The reader is closed.
        /// </summary>
        Closed
    }
}
