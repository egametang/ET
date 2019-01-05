/* Copyright 2017-present MongoDB Inc.
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
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a delete command operation.
    /// </summary>
    public class RetryableDeleteCommandOperation : RetryableWriteCommandOperationBase
    {
        // private fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly BatchableSource<DeleteRequest> _deletes;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryableDeleteCommandOperation" /> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="deletes">The deletes.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public RetryableDeleteCommandOperation(
            CollectionNamespace collectionNamespace,
            BatchableSource<DeleteRequest> deletes,
            MessageEncoderSettings messageEncoderSettings)
            : base(Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace)).DatabaseNamespace, messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _deletes = Ensure.IsNotNull(deletes, nameof(deletes));
        }

        // public properties
        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets the deletes.
        /// </summary>
        /// <value>
        /// The deletes.
        /// </value>
        public BatchableSource<DeleteRequest> Deletes
        {
            get { return _deletes; }
        }

        // protected methods
        /// <inheritdoc />
        protected override BsonDocument CreateCommand(ICoreSessionHandle session, ConnectionDescription connectionDescription, int attempt, long? transactionNumber)
        {
            if (!Feature.Collation.IsSupported(connectionDescription.ServerVersion))
            {
                if (_deletes.Items.Skip(_deletes.Offset).Take(_deletes.Count).Any(d => d.Collation != null))
                {
                    throw new NotSupportedException($"Server version {connectionDescription.ServerVersion} does not support collations.");
                }
            }

            var writeConcern = WriteConcernHelper.GetWriteConcernForWriteCommand(session, WriteConcern);
            return new BsonDocument
            {
                { "delete", _collectionNamespace.CollectionName },
                { "ordered", IsOrdered },
                { "writeConcern", writeConcern, writeConcern != null },
                { "txnNumber", () => transactionNumber.Value, transactionNumber.HasValue }
            };
        }

        /// <inheritdoc />
        protected override IEnumerable<Type1CommandMessageSection> CreateCommandPayloads(IChannelHandle channel, int attempt)
        {
            BatchableSource<DeleteRequest> deletes;
            if (attempt == 1)
            {
                deletes = _deletes;
            }
            else
            {
                deletes = new BatchableSource<DeleteRequest>(_deletes.Items, _deletes.Offset, _deletes.ProcessedCount, canBeSplit: false);
            }
            var maxBatchCount = Math.Min(MaxBatchCount ?? int.MaxValue, channel.ConnectionDescription.MaxBatchCount);
            var maxDocumentSize = channel.ConnectionDescription.MaxWireDocumentSize;
            var payload = new Type1CommandMessageSection<DeleteRequest>("deletes", deletes, DeleteRequestSerializer.Instance, NoOpElementNameValidator.Instance, maxBatchCount, maxDocumentSize);
            return new Type1CommandMessageSection[] { payload };
        }

        // nested types
        private class DeleteRequestSerializer : SealedClassSerializerBase<DeleteRequest>
        {
            public static readonly IBsonSerializer<DeleteRequest> Instance = new DeleteRequestSerializer();

            protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, DeleteRequest value)
            {
                var writer = context.Writer;
                writer.WriteStartDocument();
                writer.WriteName("q");
                BsonDocumentSerializer.Instance.Serialize(context, value.Filter);
                writer.WriteName("limit");
                writer.WriteInt32(value.Limit);
                if (value.Collation != null)
                {
                    writer.WriteName("collation");
                    BsonDocumentSerializer.Instance.Serialize(context, value.Collation.ToBsonDocument());
                }
                writer.WriteEndDocument();
            }
        }
    }
}
