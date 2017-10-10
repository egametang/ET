/* Copyright 2015 MongoDB Inc.
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

using MongoDB.Bson;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents index option defaults.
    /// </summary>
    public class IndexOptionDefaults
    {
        // private fields
        private BsonDocument _storageEngine;

        // public properties
        /// <summary>
        /// Gets or sets the storage engine options.
        /// </summary>
        public BsonDocument StorageEngine
        {
            get { return _storageEngine; }
            set { _storageEngine = value; }
        }

        // internal methods
        /// <summary>
        /// Returns this instance represented as a BsonDocument.
        /// </summary>
        /// <returns>A BsonDocument.</returns>
        internal BsonDocument ToBsonDocument()
        {
            return new BsonDocument
            {
                { "storageEngine", _storageEngine, _storageEngine != null }
            };
        }
    }
}
