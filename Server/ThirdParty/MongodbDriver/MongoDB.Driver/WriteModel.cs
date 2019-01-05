/* Copyright 2010-present MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Driver.Core.Operations;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Driver
{
    /// <summary>
    /// Base class for a write model.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
#if NET452
    [Serializable]
#endif
    public abstract class WriteModel<TDocument>
    {
        // static methods
        // These static methods are only called from the Legacy
        // API, so there is type safety in how they got allowed
        // into the system, meaning that even though
        // some things below seem unsafe, they are in a roundabout
        // way. In addition, we know that there will always 
        // be one level of BsonDocumentWrapper for everything, even
        // when the type is already a BsonDocument :(.
        internal static WriteModel<TDocument> FromCore(WriteRequest request)
        {
            switch (request.RequestType)
            {
                case WriteRequestType.Delete:
                    return ConvertDeleteRequest((DeleteRequest)request);
                case WriteRequestType.Insert:
                    return ConvertInsertRequest((InsertRequest)request);
                case WriteRequestType.Update:
                    return ConvertUpdateRequest((UpdateRequest)request);
                default:
                    throw new NotSupportedException();
            }
        }

        private static WriteModel<TDocument> ConvertDeleteRequest(DeleteRequest request)
        {
            if (request.Limit == 1)
            {
                return new DeleteOneModel<TDocument>(UnwrapFilter(request.Filter))
                {
                    Collation = request.Collation
                };
            }

            return new DeleteManyModel<TDocument>(UnwrapFilter(request.Filter))
            {
                Collation = request.Collation
            };
        }

        private static WriteModel<TDocument> ConvertInsertRequest(InsertRequest request)
        {
            var document = (TDocument)Unwrap(request.Document);
            return new InsertOneModel<TDocument>(document);
        }

        private static WriteModel<TDocument> ConvertUpdateRequest(UpdateRequest request)
        {
            if (request.IsMulti)
            {
                return new UpdateManyModel<TDocument>(UnwrapFilter(request.Filter), UnwrapUpdate(request.Update))
                {
                    ArrayFilters = request.ArrayFilters == null ? null : new List<ArrayFilterDefinition>(request.ArrayFilters.Select(f => new BsonDocumentArrayFilterDefinition<BsonValue>(f))),
                    Collation = request.Collation,
                    IsUpsert = request.IsUpsert
                };
            }

            var firstElement = request.Update.GetElement(0).Name;
            if (firstElement.StartsWith("$"))
            {
                return new UpdateOneModel<TDocument>(UnwrapFilter(request.Filter), UnwrapUpdate(request.Update))
                {
                    ArrayFilters = request.ArrayFilters == null ? null : new List<ArrayFilterDefinition>(request.ArrayFilters.Select(f => new BsonDocumentArrayFilterDefinition<BsonValue>(f))),
                    Collation = request.Collation,
                    IsUpsert = request.IsUpsert
                };
            }

            return ConvertToReplaceOne(request);
        }

        private static WriteModel<TDocument> ConvertToReplaceOne(UpdateRequest request)
        {
            var document = (TDocument)Unwrap(request.Update);
            if (request.ArrayFilters != null)
            {
                throw new ArgumentException("ReplaceOne does not support arrayFilters.", nameof(request));
            }

            return new ReplaceOneModel<TDocument>(UnwrapFilter(request.Filter), document)
            {
                Collation = request.Collation,
                IsUpsert = request.IsUpsert
            };
        }

        private static FilterDefinition<TDocument> UnwrapFilter(BsonDocument filter)
        {
            var wrapper = filter as BsonDocumentWrapper;
            if (wrapper != null)
            {
                if (wrapper.Wrapped is BsonDocument)
                {
                    return new BsonDocumentFilterDefinition<TDocument>((BsonDocument)wrapper.Wrapped);
                }
                return new ObjectFilterDefinition<TDocument>(wrapper.Wrapped);
            }

            return new BsonDocumentFilterDefinition<TDocument>(filter);
        }

        private static UpdateDefinition<TDocument> UnwrapUpdate(BsonDocument update)
        {
            var wrapper = update as BsonDocumentWrapper;
            if (wrapper != null)
            {
                if (wrapper.Wrapped is BsonDocument)
                {
                    return new BsonDocumentUpdateDefinition<TDocument>((BsonDocument)wrapper.Wrapped);
                }
                return new ObjectUpdateDefinition<TDocument>(wrapper.Wrapped);
            }

            return new BsonDocumentUpdateDefinition<TDocument>(update);
        }

        private static object Unwrap(BsonDocument wrapper)
        {
            return ((BsonDocumentWrapper)wrapper).Wrapped;
        }

        // constructors
        // only MongoDB can define new write models.
        internal WriteModel()
        {
        }

        // properties
        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        public abstract WriteModelType ModelType { get; }
    }
}
