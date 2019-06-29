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
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    internal class BulkInsertOperation : BulkUnmixedWriteOperationBase<InsertRequest>
    {
        // constructors
        public BulkInsertOperation(
            CollectionNamespace collectionNamespace,
            IEnumerable<InsertRequest> requests,
            MessageEncoderSettings messageEncoderSettings)
            : base(collectionNamespace, requests, messageEncoderSettings)
        {
        }

        // methods
        protected override IRetryableWriteOperation<BsonDocument> CreateBatchOperation(Batch batch)
        {
            return new RetryableInsertCommandOperation<InsertRequest>(CollectionNamespace, batch.Requests, InsertRequestSerializer.Instance, MessageEncoderSettings)
            {
                BypassDocumentValidation = BypassDocumentValidation,
                IsOrdered = IsOrdered,
                MaxBatchCount = MaxBatchCount,
                RetryRequested = RetryRequested,
                WriteConcern = WriteConcern
            };
        }

        protected override IExecutableInRetryableWriteContext<BulkWriteOperationResult> CreateEmulator()
        {
            return new BulkInsertOperationEmulator(CollectionNamespace, Requests, MessageEncoderSettings)
            {
                IsOrdered = IsOrdered,
                MaxBatchCount = MaxBatchCount,
                MaxBatchLength = MaxBatchLength,
                WriteConcern = WriteConcern
            };
        }

        protected override bool RequestHasCollation(InsertRequest request)
        {
            return false;
        }

        // nested types
        private class InsertRequestSerializer : SealedClassSerializerBase<InsertRequest>
        {
            public static InsertRequestSerializer Instance = new InsertRequestSerializer();

            protected override InsertRequest DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                var document = BsonDocumentSerializer.Instance.Deserialize(context, args);
                return new InsertRequest(document);
            }

            protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, InsertRequest value)
            {
                BsonDocumentSerializer.Instance.Serialize(context, args, value.Document);
            }
        }
    }
}
