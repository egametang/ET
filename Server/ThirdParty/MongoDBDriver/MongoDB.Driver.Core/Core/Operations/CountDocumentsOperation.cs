/* Copyright 2018-present MongoDB Inc.
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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a count documents operation.
    /// </summary>
    public class CountDocumentsOperation : IReadOperation<long>
    {
        // private fields
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private BsonDocument _filter;
        private BsonValue _hint;
        private long? _limit;
        private TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private ReadConcern _readConcern = ReadConcern.Default;
        private long? _skip;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CountOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public CountDocumentsOperation(CollectionNamespace collectionNamespace, MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        // public properties
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
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public BsonDocument Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Gets or sets the index hint.
        /// </summary>
        /// <value>
        /// The index hint.
        /// </value>
        public BsonValue Hint
        {
            get { return _hint; }
            set { _hint = value; }
        }

        /// <summary>
        /// Gets or sets a limit on the number of matching documents to count.
        /// </summary>
        /// <value>
        /// A limit on the number of matching documents to count.
        /// </value>
        public long? Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        /// <summary>
        /// Gets or sets the maximum time the server should spend on this operation.
        /// </summary>
        /// <value>
        /// The maximum time the server should spend on this operation.
        /// </value>
        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = Ensure.IsNullOrInfiniteOrGreaterThanOrEqualToZero(value, nameof(value)); }
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
        /// Gets or sets the number of documents to skip before counting the remaining matching documents.
        /// </summary>
        /// <value>
        /// The number of documents to skip before counting the remaining matching documents.
        /// </value>
        public long? Skip
        {
            get { return _skip; }
            set { _skip = value; }
        }

        // public methods
        /// <inheritdoc/>
        public long Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference, binding.Session.Fork()))
            {
                var operation = CreateOperation();
                var cursor = operation.Execute(channelBinding, cancellationToken);
                var result = cursor.ToList(cancellationToken);
                return ExtractCountFromResult(result);
            }
        }

        /// <inheritdoc/>
        public async Task<long> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference, binding.Session.Fork()))
            {
                var operation = CreateOperation();
                var cursor = await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
                var result = await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
                return ExtractCountFromResult(result);
            }
        }

        // private methods
        private AggregateOperation<BsonDocument> CreateOperation()
        {
            var pipeline = CreatePipeline();
            var operation = new AggregateOperation<BsonDocument>(_collectionNamespace, pipeline, BsonDocumentSerializer.Instance, _messageEncoderSettings)
            {
                Collation = _collation,
                Hint = _hint,
                MaxTime = _maxTime,
                ReadConcern = _readConcern
            };
            return operation;
        }

        private List<BsonDocument> CreatePipeline()
        {
            var pipeline = new List<BsonDocument>();
            pipeline.Add(new BsonDocument("$match", _filter ?? new BsonDocument()));
            if (_skip.HasValue)
            {
                pipeline.Add(new BsonDocument("$skip", _skip.Value));
            }
            if (_limit.HasValue)
            {
                pipeline.Add(new BsonDocument("$limit", _limit.Value));
            }
            pipeline.Add(new BsonDocument("$group", new BsonDocument { { "_id", BsonNull.Value }, { "n", new BsonDocument("$sum", 1) } }));
            return pipeline;
        }

        private long ExtractCountFromResult(List<BsonDocument> result)
        {
            switch (result.Count)
            {
                case 0:
                    return 0;

                case 1:
                    return result[0]["n"].ToInt64();

                default:
                    throw new MongoClientException($"Expected aggregate command for CountDocuments to return 1 document, but got {result.Count}.");
            }
        }
    }
}
