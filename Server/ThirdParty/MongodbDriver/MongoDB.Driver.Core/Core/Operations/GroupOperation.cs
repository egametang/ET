/* Copyright 2013-2016 MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a group operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class GroupOperation<TResult> : IReadOperation<IEnumerable<TResult>>
    {
        // fields
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly BsonDocument _filter;
        private BsonJavaScript _finalizeFunction;
        private readonly BsonDocument _initial;
        private readonly BsonDocument _key;
        private readonly BsonJavaScript _keyFunction;
        private TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly BsonJavaScript _reduceFunction;
        private IBsonSerializer<TResult> _resultSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupOperation{TResult}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="key">The key.</param>
        /// <param name="initial">The initial aggregation result for each group.</param>
        /// <param name="reduceFunction">The reduce function.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public GroupOperation(CollectionNamespace collectionNamespace, BsonDocument key, BsonDocument initial, BsonJavaScript reduceFunction, BsonDocument filter, MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _key = Ensure.IsNotNull(key, nameof(key));
            _initial = Ensure.IsNotNull(initial, nameof(initial));
            _reduceFunction = Ensure.IsNotNull(reduceFunction, nameof(reduceFunction));
            _filter = filter; // can be null
            _messageEncoderSettings = messageEncoderSettings;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupOperation{TResult}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="keyFunction">The key function.</param>
        /// <param name="initial">The initial aggregation result for each group.</param>
        /// <param name="reduceFunction">The reduce function.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public GroupOperation(CollectionNamespace collectionNamespace, BsonJavaScript keyFunction, BsonDocument initial, BsonJavaScript reduceFunction, BsonDocument filter, MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _keyFunction = Ensure.IsNotNull(keyFunction, nameof(keyFunction));
            _initial = Ensure.IsNotNull(initial, nameof(initial));
            _reduceFunction = Ensure.IsNotNull(reduceFunction, nameof(reduceFunction));
            _filter = filter;
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
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
        /// Gets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public BsonDocument Filter
        {
            get { return _filter; }
        }

        /// <summary>
        /// Gets or sets the finalize function.
        /// </summary>
        /// <value>
        /// The finalize function.
        /// </value>
        public BsonJavaScript FinalizeFunction
        {
            get { return _finalizeFunction; }
            set { _finalizeFunction = value; }
        }

        /// <summary>
        /// Gets the initial aggregation result for each group.
        /// </summary>
        /// <value>
        /// The initial aggregation result for each group.
        /// </value>
        public BsonDocument Initial
        {
            get { return _initial; }
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public BsonDocument Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets the key function.
        /// </summary>
        /// <value>
        /// The key function.
        /// </value>
        public BsonJavaScript KeyFunction
        {
            get { return _keyFunction; }
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
            set { _maxTime = value; }
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
        /// Gets the reduce function.
        /// </summary>
        /// <value>
        /// The reduce function.
        /// </value>
        public BsonJavaScript ReduceFunction
        {
            get { return _reduceFunction; }
        }

        /// <summary>
        /// Gets or sets the result serializer.
        /// </summary>
        /// <value>
        /// The result serializer.
        /// </value>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
            set { _resultSerializer = value; }
        }

        // public methods
        /// <inheritdoc/>
        public IEnumerable<TResult> Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                return operation.Execute(channelBinding, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TResult>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                return await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
            }
        }

        // private methods
        internal BsonDocument CreateCommand(SemanticVersion serverVersion)
        {
            Feature.Collation.ThrowIfNotSupported(serverVersion, _collation);

            return new BsonDocument
            {
                { "group", new BsonDocument
                    {
                        { "ns", _collectionNamespace.CollectionName },
                        { "key", _key, _key != null },
                        { "$keyf", _keyFunction, _keyFunction != null },
                        { "$reduce", _reduceFunction },
                        { "initial", _initial },
                        { "cond", _filter, _filter != null },
                        { "finalize", _finalizeFunction, _finalizeFunction != null },
                        { "collation", () => _collation.ToBsonDocument(), _collation != null }
                    }
                },
                { "maxTimeMS", () => _maxTime.Value.TotalMilliseconds, _maxTime.HasValue }
           };
        }

        private ReadCommandOperation<TResult[]> CreateOperation(SemanticVersion serverVersion)
        {
            var command = CreateCommand(serverVersion);
            var resultSerializer = _resultSerializer ?? BsonSerializer.LookupSerializer<TResult>();
            var resultArraySerializer = new ArraySerializer<TResult>(resultSerializer);
            var commandResultSerializer = new ElementDeserializer<TResult[]>("retval", resultArraySerializer);
            return new ReadCommandOperation<TResult[]>(_collectionNamespace.DatabaseNamespace, command, commandResultSerializer, _messageEncoderSettings);
        }
    }
}
