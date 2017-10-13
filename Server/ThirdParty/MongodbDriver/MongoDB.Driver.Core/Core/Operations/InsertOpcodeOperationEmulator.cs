/* Copyright 2013-2015 MongoDB Inc.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    internal class InsertOpcodeOperationEmulator<TDocument>
    {
        // fields
        private bool? _bypassDocumentValidation;
        private readonly CollectionNamespace _collectionNamespace;
        private bool _continueOnError;
        private readonly BatchableSource<TDocument> _documentSource;
        private int? _maxBatchCount;
        private int? _maxDocumentSize;
        private int? _maxMessageSize;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly IBsonSerializer<TDocument> _serializer;
        private WriteConcern _writeConcern = WriteConcern.Acknowledged;

        // constructors
        public InsertOpcodeOperationEmulator(
            CollectionNamespace collectionNamespace,
            IBsonSerializer<TDocument> serializer,
            BatchableSource<TDocument> documentSource,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _documentSource = Ensure.IsNotNull(documentSource, nameof(documentSource));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
        }

        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        public bool ContinueOnError
        {
            get { return _continueOnError; }
            set { _continueOnError = value; }
        }

        public BatchableSource<TDocument> DocumentSource
        {
            get { return _documentSource; }
        }

        public int? MaxBatchCount
        {
            get { return _maxBatchCount; }
            set { _maxBatchCount = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        public int? MaxDocumentSize
        {
            get { return _maxDocumentSize; }
            set { _maxDocumentSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        public int? MaxMessageSize
        {
            get { return _maxMessageSize; }
            set { _maxMessageSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        public WriteConcernResult Execute(IChannelHandle channel, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(channel, nameof(channel));

            var operation = CreateOperation();
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                result = operation.Execute(channel, cancellationToken);
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return CreateResultOrThrow(channel, result, exception);
        }

        public async Task<WriteConcernResult> ExecuteAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(channel, nameof(channel));

            var operation = CreateOperation();
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                result = await operation.ExecuteAsync(channel, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return CreateResultOrThrow(channel, result, exception);
        }

        // private methods
        private BulkInsertOperation CreateOperation()
        {
            var requests = _documentSource.GetRemainingItems().Select(d =>
            {
                if (d == null)
                {
                    throw new ArgumentException("Batch contains one or more null documents.");
                }

                return new InsertRequest(new BsonDocumentWrapper(d, _serializer));
            });
            return new BulkInsertOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                BypassDocumentValidation = _bypassDocumentValidation,
                IsOrdered = !_continueOnError,
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxMessageSize,
                // ReaderSettings = ?
                WriteConcern = _writeConcern,
                // WriteSettings = ?
            };
        }

        private WriteConcernResult CreateResultOrThrow(IChannel channel, BulkWriteOperationResult result, MongoBulkWriteOperationException exception)
        {
            var converter = new BulkWriteOperationResultConverter();
            if (exception != null)
            {
                throw converter.ToWriteConcernException(channel.ConnectionDescription.ConnectionId, exception);
            }
            else
            {
                if (_writeConcern.IsAcknowledged)
                {
                    return converter.ToWriteConcernResult(result);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
