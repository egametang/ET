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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    internal class BulkInsertOperationEmulator : BulkUnmixedWriteOperationEmulatorBase
    {
        // constructors
        public BulkInsertOperationEmulator(
            CollectionNamespace collectionNamespace,
            IEnumerable<InsertRequest> requests,
            MessageEncoderSettings messageEncoderSettings)
            : base(collectionNamespace, requests, messageEncoderSettings)
        {
        }

        //  methods
        protected override WriteConcernResult ExecuteProtocol(IChannelHandle channel, WriteRequest request, CancellationToken cancellationToken)
        {
            var insertRequest = (InsertRequest)request;
            var documentSource = new BatchableSource<BsonDocument>(new[] { insertRequest.Document });

            return channel.Insert(
                CollectionNamespace,
                WriteConcern,
                BsonDocumentSerializer.Instance,
                MessageEncoderSettings,
                documentSource,
                MaxBatchCount,
                MaxBatchLength,
                !IsOrdered, // continueOnError
                null, // shouldSendGetLastError
                cancellationToken);
        }

        protected override Task<WriteConcernResult> ExecuteProtocolAsync(IChannelHandle channel, WriteRequest request, CancellationToken cancellationToken)
        {
            var insertRequest = (InsertRequest)request;
            var documentSource = new BatchableSource<BsonDocument>(new[] { insertRequest.Document });

            return channel.InsertAsync(
                CollectionNamespace,
                WriteConcern,
                BsonDocumentSerializer.Instance,
                MessageEncoderSettings,
                documentSource,
                MaxBatchCount,
                MaxBatchLength,
                !IsOrdered, // continueOnError
                null, // shouldSendGetLastError
                cancellationToken);
        }
    }
}
