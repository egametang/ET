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

using System;

namespace MongoDB.Driver
{
    internal class OfTypeMongoCollection<TRootDocument, TDerivedDocument> : FilteredMongoCollectionBase<TDerivedDocument>
        where TDerivedDocument : TRootDocument
    {
        // private fields
        private readonly IMongoCollection<TRootDocument> _rootDocumentCollection;

        // constructors
        public OfTypeMongoCollection(
            IMongoCollection<TRootDocument> rootDocumentCollection,
            IMongoCollection<TDerivedDocument> derivedDocumentCollection,
            FilterDefinition<TDerivedDocument> ofTypeFilter)
            : base(derivedDocumentCollection, ofTypeFilter)
        {
            _rootDocumentCollection = rootDocumentCollection;
        }

        // public methods
        public override IFilteredMongoCollection<TMoreDerivedDocument> OfType<TMoreDerivedDocument>()
        {
            return _rootDocumentCollection.OfType<TMoreDerivedDocument>();
        }

        public override IMongoCollection<TDerivedDocument> WithReadConcern(ReadConcern readConcern)
        {
            return new OfTypeMongoCollection<TRootDocument, TDerivedDocument>(_rootDocumentCollection, WrappedCollection.WithReadConcern(readConcern), Filter);
        }

        public override IMongoCollection<TDerivedDocument> WithReadPreference(ReadPreference readPreference)
        {
            return new OfTypeMongoCollection<TRootDocument, TDerivedDocument>(_rootDocumentCollection, WrappedCollection.WithReadPreference(readPreference), Filter);
        }

        public override IMongoCollection<TDerivedDocument> WithWriteConcern(WriteConcern writeConcern)
        {
            return new OfTypeMongoCollection<TRootDocument, TDerivedDocument>(_rootDocumentCollection, WrappedCollection.WithWriteConcern(writeConcern), Filter);
        }
    }
}
