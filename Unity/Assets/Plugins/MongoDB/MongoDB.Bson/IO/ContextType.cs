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
    /// Used by BsonReaders and BsonWriters to represent the current context.
    /// </summary>
    public enum ContextType
    {
        /// <summary>
        /// The top level of a BSON document.
        /// </summary>
        TopLevel,
        /// <summary>
        /// A (possibly embedded) BSON document.
        /// </summary>
        Document,
        /// <summary>
        /// A BSON array.
        /// </summary>
        Array,
        /// <summary>
        /// A JavaScriptWithScope BSON value.
        /// </summary>
        JavaScriptWithScope,
        /// <summary>
        /// The scope document of a JavaScriptWithScope BSON value.
        /// </summary>
        ScopeDocument
    }
}
