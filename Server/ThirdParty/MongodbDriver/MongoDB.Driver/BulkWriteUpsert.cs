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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Support;
using MongoDB.Shared;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the information about one Upsert.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class BulkWriteUpsert
    {
        // private fields
        private readonly BsonValue _id;
        private readonly int _index;

        // constructors
        internal BulkWriteUpsert(
            int index,
            BsonValue id)
        {
            _index = index;
            _id = id;
        }

        // public properties
        /// <summary>
        /// Gets the id.
        /// </summary>
        public BsonValue Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        // internal static methods
        internal static BulkWriteUpsert FromCore(Core.Operations.BulkWriteOperationUpsert upsert)
        {
            return new BulkWriteUpsert(upsert.Index, upsert.Id);
        }

        // internal methods
        internal BulkWriteUpsert WithMappedIndex(IndexMap indexMap)
        {
            var mappedIndex = indexMap.Map(_index);
            return (_index == mappedIndex) ? this : new BulkWriteUpsert(mappedIndex, _id);
        }
    }
}

