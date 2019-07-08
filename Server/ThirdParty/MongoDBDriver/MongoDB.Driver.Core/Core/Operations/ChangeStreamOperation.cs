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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// A change stream operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IChangeStreamOperation<TResult> : IReadOperation<IAsyncCursor<TResult>>
    {
        // properties
        /// <summary>
        /// Gets or sets the resume after value.
        /// </summary>
        /// <value>
        /// The resume after value.
        /// </value>
        BsonDocument ResumeAfter { get; set; }

        /// <summary>
        /// Gets or sets the start at operation time.
        /// </summary>
        /// <value>
        /// The start at operation time.
        /// </value>
        BsonTimestamp StartAtOperationTime { get; set; }

        // methods
        /// <summary>
        /// Resumes the operation after a resume token.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor.</returns>
        IAsyncCursor<RawBsonDocument> Resume(IReadBinding binding, CancellationToken cancellationToken);

        /// <summary>
        /// Resumes the operation after a resume token.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        Task<IAsyncCursor<RawBsonDocument>> ResumeAsync(IReadBinding binding, CancellationToken cancellationToken);
    }

    /// <summary>
    /// A change stream operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result values.</typeparam>
    public class ChangeStreamOperation<TResult> : IChangeStreamOperation<TResult>
    {
        // private fields
        private int? _batchSize;
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly DatabaseNamespace _databaseNamespace;
        private ChangeStreamFullDocumentOption _fullDocument = ChangeStreamFullDocumentOption.Default;
        private TimeSpan? _maxAwaitTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly IReadOnlyList<BsonDocument> _pipeline;
        private ReadConcern _readConcern = ReadConcern.Default;
        private readonly IBsonSerializer<TResult> _resultSerializer;
        private BsonDocument _resumeAfter;
        private BsonTimestamp _startAtOperationTime;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStreamOperation{TResult}"/> class.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="resultSerializer">The result value serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ChangeStreamOperation(
            IEnumerable<BsonDocument> pipeline, 
            IBsonSerializer<TResult> resultSerializer, 
            MessageEncoderSettings messageEncoderSettings)
        {
            _pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline)).ToList();
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStreamOperation{TResult}"/> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="resultSerializer">The result value serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ChangeStreamOperation(
            DatabaseNamespace databaseNamespace,
            IEnumerable<BsonDocument> pipeline,
            IBsonSerializer<TResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings)
            : this(pipeline, resultSerializer, messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStreamOperation{TResult}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="resultSerializer">The result value serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ChangeStreamOperation(
            CollectionNamespace collectionNamespace,
            IEnumerable<BsonDocument> pipeline,
            IBsonSerializer<TResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings)
            : this(pipeline, resultSerializer, messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
        }

        // public properties
        /// <summary>
        /// Gets or sets the size of the batch.
        /// </summary>
        /// <value>
        /// The size of the batch.
        /// </value>
        public int? BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = value; }
        }

        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        /// <value>
        /// The collation.
        /// </value>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public CollectionNamespace CollectionNamespace => _collectionNamespace;

        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        public DatabaseNamespace DatabaseNamespace => _databaseNamespace;

        /// <summary>
        /// Gets or sets the full document option.
        /// </summary>
        /// <value>
        /// The full document option.
        /// </value>
        public ChangeStreamFullDocumentOption FullDocument
        {
            get { return _fullDocument; }
            set { _fullDocument = value; }
        }

        /// <summary>
        /// Gets or sets the maximum await time.
        /// </summary>
        /// <value>
        /// The maximum await time.
        /// </value>
        public TimeSpan? MaxAwaitTime
        {
            get { return _maxAwaitTime; }
            set { _maxAwaitTime = value; }
        }

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings => _messageEncoderSettings;

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        /// <value>
        /// The pipeline.
        /// </value>
        public IReadOnlyList<BsonDocument> Pipeline => _pipeline;

        /// <summary>
        /// Gets or sets the read concern.
        /// </summary>
        /// <value>
        /// The read concern.
        /// </value>
        public ReadConcern ReadConcern
        {
            get { return _readConcern; }
            set { _readConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        /// <value>
        /// The result serializer.
        /// </value>
        public IBsonSerializer<TResult> ResultSerializer => _resultSerializer;

        /// <inheritdoc />
        public BsonDocument ResumeAfter
        {
            get { return _resumeAfter; }
            set { _resumeAfter = value; }
        }

        /// <inheritdoc />
        public BsonTimestamp StartAtOperationTime
        {
            get { return _startAtOperationTime; }
            set { _startAtOperationTime = value; }
        }

        // public methods        
        /// <inheritdoc />
        public IAsyncCursor<TResult> Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            var bindingHandle = binding as IReadBindingHandle;
            if (bindingHandle == null)
            {
                throw new ArgumentException("The binding value passed to ChangeStreamOperation.Execute must implement IReadBindingHandle.", nameof(binding));
            }

            IAsyncCursor<RawBsonDocument> cursor;
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference, binding.Session.Fork()))
            {
                cursor = Resume(channelBinding, cancellationToken);
                if (_startAtOperationTime == null && _resumeAfter == null)
                {
                    var maxWireVersion = channel.ConnectionDescription.IsMasterResult.MaxWireVersion;
                    if (maxWireVersion >= 7)
                    {
                        _startAtOperationTime = binding.Session.OperationTime;
                    }
                }
            }

            return new ChangeStreamCursor<TResult>(cursor, _resultSerializer, bindingHandle.Fork(), this);
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TResult>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            var bindingHandle = binding as IReadBindingHandle;
            if (bindingHandle == null)
            {
                throw new ArgumentException("The binding value passed to ChangeStreamOperation.ExecuteAsync must implement IReadBindingHandle.", nameof(binding));
            }

            IAsyncCursor<RawBsonDocument> cursor;
            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference, binding.Session.Fork()))
            {
                cursor = await ResumeAsync(channelBinding, cancellationToken).ConfigureAwait(false);
                if (_startAtOperationTime == null && _resumeAfter == null)
                {
                    var maxWireVersion = channel.ConnectionDescription.IsMasterResult.MaxWireVersion;
                    if (maxWireVersion >= 7)
                    {
                        _startAtOperationTime = binding.Session.OperationTime;
                    }
                }
            }

            return new ChangeStreamCursor<TResult>(cursor, _resultSerializer, bindingHandle.Fork(), this);
        }

        /// <inheritdoc />
        public IAsyncCursor<RawBsonDocument> Resume(IReadBinding binding, CancellationToken cancellationToken)
        {
            var aggregateOperation = CreateAggregateOperation();
            return aggregateOperation.Execute(binding, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IAsyncCursor<RawBsonDocument>> ResumeAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            var aggregateOperation = CreateAggregateOperation();
            return aggregateOperation.ExecuteAsync(binding, cancellationToken);
        }

        // private methods
        private AggregateOperation<RawBsonDocument> CreateAggregateOperation()
        {
            var changeStreamStage = CreateChangeStreamStage();
            var combinedPipeline = CreateCombinedPipeline(changeStreamStage);

            AggregateOperation<RawBsonDocument> operation;
            if (_collectionNamespace != null)
            {
                operation = new AggregateOperation<RawBsonDocument>(_collectionNamespace, combinedPipeline, RawBsonDocumentSerializer.Instance, _messageEncoderSettings);
            }
            else
            {
                var databaseNamespace = _databaseNamespace ?? DatabaseNamespace.Admin;
                operation = new AggregateOperation<RawBsonDocument>(databaseNamespace, combinedPipeline, RawBsonDocumentSerializer.Instance, _messageEncoderSettings);
            }

            operation.BatchSize = _batchSize;
            operation.Collation = _collation;
            operation.MaxAwaitTime = _maxAwaitTime;
            operation.ReadConcern = _readConcern;

            return operation;
        }

        private BsonDocument CreateChangeStreamStage()
        {
            var changeStreamOptions = new BsonDocument
            {
                { "fullDocument", ToString(_fullDocument) },
                { "allChangesForCluster", true, _collectionNamespace == null && _databaseNamespace == null },
                { "startAtOperationTime", _startAtOperationTime, _startAtOperationTime != null },
                { "resumeAfter", _resumeAfter, _resumeAfter != null }
            };
            return new BsonDocument("$changeStream", changeStreamOptions);
        }

        private List<BsonDocument> CreateCombinedPipeline(BsonDocument changeStreamStage)
        {
            var combinedPipeline = new List<BsonDocument>();
            combinedPipeline.Add(changeStreamStage);
            combinedPipeline.AddRange(_pipeline);
            return combinedPipeline;
        }

        private string ToString(ChangeStreamFullDocumentOption fullDocument)
        {
            switch (fullDocument)
            {
                case ChangeStreamFullDocumentOption.Default: return "default";
                case ChangeStreamFullDocumentOption.UpdateLookup: return "updateLookup";
                default: throw new ArgumentException($"Invalid FullDocument option: {fullDocument}.", nameof(fullDocument));
            }
        }
    }
}
