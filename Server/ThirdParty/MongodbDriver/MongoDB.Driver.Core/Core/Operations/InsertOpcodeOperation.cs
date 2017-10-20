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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents an insert operation using the insert opcode.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class InsertOpcodeOperation<TDocument> : IWriteOperation<IEnumerable<WriteConcernResult>>
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
        private WriteConcern _writeConcern;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertOpcodeOperation{TDocument}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="documentSource">The document source.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public InsertOpcodeOperation(CollectionNamespace collectionNamespace, BatchableSource<TDocument> documentSource, IBsonSerializer<TDocument> serializer, MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _documentSource = Ensure.IsNotNull(documentSource, nameof(documentSource));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
            _writeConcern = WriteConcern.Acknowledged;
        }

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        /// <value>
        /// A value indicating whether to bypass document validation.
        /// </value>
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
        }

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
        /// Gets a value indicating whether the server should continue on error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server should continue on error; otherwise, <c>false</c>.
        /// </value>
        public bool ContinueOnError
        {
            get { return _continueOnError; }
            set { _continueOnError = value; }
        }

        /// <summary>
        /// Gets the document source.
        /// </summary>
        /// <value>
        /// The document source.
        /// </value>
        public BatchableSource<TDocument> DocumentSource
        {
            get { return _documentSource; }
        }

        /// <summary>
        /// Gets or sets the maximum number of documents in a batch.
        /// </summary>
        /// <value>
        /// The maximum number of documents in a batch.
        /// </value>
        public int? MaxBatchCount
        {
            get { return _maxBatchCount; }
            set { _maxBatchCount = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum size of a document.
        /// </summary>
        /// <value>
        /// The maximum size of a document.
        /// </value>
        public int? MaxDocumentSize
        {
            get { return _maxDocumentSize; }
            set { _maxDocumentSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum size of a message.
        /// </summary>
        /// <value>
        /// The maximum size of a message.
        /// </value>
        public int? MaxMessageSize
        {
            get { return _maxMessageSize; }
            set { _maxMessageSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <value>
        /// The serializer.
        /// </value>
        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }

        /// <summary>
        /// Gets or sets the write concern.
        /// </summary>
        /// <value>
        /// The write concern.
        /// </value>
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        /// <inheritdoc/>
        public IEnumerable<WriteConcernResult> Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = binding.GetWriteChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            {
                if (Feature.WriteCommands.IsSupported(channel.ConnectionDescription.ServerVersion) && _writeConcern.IsAcknowledged)
                {
                    var emulator = CreateEmulator();
                    var result = emulator.Execute(channel, cancellationToken);
                    return new[] { result };
                }
                else
                {
                    if (_documentSource.Batch == null)
                    {
                        return InsertMultipleBatches(channel, cancellationToken);
                    }
                    else
                    {
                        var result = InsertSingleBatch(channel, cancellationToken);
                        return new[] { result };
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WriteConcernResult>> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = await binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            {
                if (Feature.WriteCommands.IsSupported(channel.ConnectionDescription.ServerVersion) && _writeConcern.IsAcknowledged)
                {
                    var emulator = CreateEmulator();
                    var result = await emulator.ExecuteAsync(channel, cancellationToken).ConfigureAwait(false);
                    return new[] { result };
                }
                else
                {
                    if (_documentSource.Batch == null)
                    {
                        return await InsertMultipleBatchesAsync(channel, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var result = await InsertSingleBatchAsync(channel, cancellationToken).ConfigureAwait(false);
                        return new[] { result };
                    }
                }
            }
        }

        // private methods
        private InsertOpcodeOperationEmulator<TDocument> CreateEmulator()
        {
            return new InsertOpcodeOperationEmulator<TDocument>(_collectionNamespace, _serializer, _documentSource, _messageEncoderSettings)
            {
                BypassDocumentValidation = _bypassDocumentValidation,
                ContinueOnError = _continueOnError,
                MaxBatchCount = _maxBatchCount,
                MaxDocumentSize = _maxDocumentSize,
                MaxMessageSize = _maxMessageSize,
                WriteConcern = _writeConcern
            };
        }

        private WriteConcernResult ExecuteProtocol(IChannelHandle channel, WriteConcern batchWriteConcern, Func<bool> shouldSendGetLastError, CancellationToken cancellationToken)
        {
            return channel.Insert<TDocument>(
                _collectionNamespace,
                batchWriteConcern,
                _serializer,
                _messageEncoderSettings,
                _documentSource,
                _maxBatchCount,
                _maxMessageSize,
                _continueOnError,
                shouldSendGetLastError,
                cancellationToken);
        }

        private Task<WriteConcernResult> ExecuteProtocolAsync(IChannelHandle channel, WriteConcern batchWriteConcern, Func<bool> shouldSendGetLastError, CancellationToken cancellationToken)
        {
            return channel.InsertAsync<TDocument>(
                _collectionNamespace,
                batchWriteConcern,
                _serializer,
                _messageEncoderSettings,
                _documentSource,
                _maxBatchCount,
                _maxMessageSize,
                _continueOnError,
                shouldSendGetLastError,
                cancellationToken);
        }

        private IEnumerable<WriteConcernResult> InsertMultipleBatches(IChannelHandle channel, CancellationToken cancellationToken)
        {
            var helper = new BatchHelper(_documentSource, _writeConcern, _continueOnError);

            while (_documentSource.HasMore)
            {
                WriteConcernResult result;
                try
                {
                    result = ExecuteProtocol(channel, helper.BatchWriteConcern, helper.ShouldSendGetLastError, cancellationToken);
                }
                catch (MongoWriteConcernException ex)
                {
                    result = helper.HandleException(ex);
                    if (!_continueOnError)
                    {
                        return null;
                    }
                }
                helper.AddResult(result);

                _documentSource.ClearBatch();
            }

            return helper.CreateFinalResultOrThrow();
        }

        private async Task<IEnumerable<WriteConcernResult>> InsertMultipleBatchesAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            var helper = new BatchHelper(_documentSource, _writeConcern, _continueOnError);

            while (_documentSource.HasMore)
            {
                WriteConcernResult result;
                try
                {
                    result = await ExecuteProtocolAsync(channel, helper.BatchWriteConcern, helper.ShouldSendGetLastError, cancellationToken).ConfigureAwait(false);
                }
                catch (MongoWriteConcernException ex)
                {
                    result = helper.HandleException(ex);
                    if (!_continueOnError)
                    {
                        return null;
                    }
                }
                helper.AddResult(result);

                _documentSource.ClearBatch();
            }

            return helper.CreateFinalResultOrThrow();
        }

        private WriteConcernResult InsertSingleBatch(IChannelHandle channel, CancellationToken cancellationToken)
        {
            return ExecuteProtocol(channel, _writeConcern, null, cancellationToken);
        }

        private Task<WriteConcernResult> InsertSingleBatchAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            return ExecuteProtocolAsync(channel, _writeConcern, null, cancellationToken);
        }

        // nested types
        private class BatchHelper
        {
            private readonly WriteConcern _batchWriteConcern;
            private readonly bool _continueOnError;
            private Exception _finalException;
            private readonly List<WriteConcernResult> _results;
            private Func<bool> _shouldSendGetLastError;
            private readonly WriteConcern _writeConcern;

            public BatchHelper(BatchableSource<TDocument> documentSource, WriteConcern writeConcern, bool continueOnError)
            {
                _writeConcern = writeConcern;
                _continueOnError = continueOnError;
                _results = writeConcern.IsAcknowledged ? new List<WriteConcernResult>() : null;
                _batchWriteConcern = writeConcern;
                _shouldSendGetLastError = null;
                if (!writeConcern.IsAcknowledged && !continueOnError)
                {
                    _batchWriteConcern = WriteConcern.Acknowledged;
                    _shouldSendGetLastError = () => documentSource.HasMore;
                }
            }

            public WriteConcern BatchWriteConcern { get { return _batchWriteConcern; } }
            public Func<bool> ShouldSendGetLastError { get { return _shouldSendGetLastError; } }

            public void AddResult(WriteConcernResult result)
            {
                if (_results != null)
                {
                    _results.Add(result);
                }
            }

            public WriteConcernResult HandleException(MongoWriteConcernException exception)
            {
                var result = exception.WriteConcernResult;

                if (_continueOnError)
                {
                    _finalException = exception;
                }
                else if (_writeConcern.IsAcknowledged)
                {
                    _results.Add(result);
                    exception.Data["results"] = _results;
                    throw exception;
                }

                return result;
            }

            public IEnumerable<WriteConcernResult> CreateFinalResultOrThrow()
            {
                if (_writeConcern.IsAcknowledged && _finalException != null)
                {
                    _finalException.Data["results"] = _results;
                    throw _finalException;
                }

                return _results;
            }
        }
    }
}
