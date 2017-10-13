/* Copyright 2015-2016 MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents the geoNear command.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class GeoNearOperation<TResult> : IReadOperation<TResult>
    {
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private double? _distanceMultiplier;
        private BsonDocument _filter;
        private bool? _includeLocs;
        private int? _limit;
        private double? _maxDistance;
        private TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly BsonValue _near;
        private ReadConcern _readConcern = ReadConcern.Default;
        private readonly IBsonSerializer<TResult> _resultSerializer;
        private bool? _spherical;
        private bool? _uniqueDocs;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoNearOperation{TResult}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="near">The point for which to find the closest documents.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public GeoNearOperation(CollectionNamespace collectionNamespace, BsonValue near, IBsonSerializer<TResult> resultSerializer, MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _near = Ensure.IsNotNull(near, nameof(near));
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets or sets the distance multiplier.
        /// </summary>
        public double? DistanceMultiplier
        {
            get { return _distanceMultiplier; }
            set { _distanceMultiplier = value; }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        public BsonDocument Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Gets or sets whether to include the locations of the matching documents.
        /// </summary>
        public bool? IncludeLocs
        {
            get { return _includeLocs; }
            set { _includeLocs = value; }
        }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        public int? Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        /// <summary>
        /// Gets or sets the maximum distance.
        /// </summary>
        public double? MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = value; }
        }

        /// <summary>
        /// Gets or sets the maximum time.
        /// </summary>
        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = value; }
        }

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        /// <summary>
        /// Gets the point for which to find the closest documents.
        /// </summary>
        public BsonValue Near
        {
            get { return _near; }
        }

        /// <summary>
        /// Gets or sets the read concern.
        /// </summary>
        public ReadConcern ReadConcern
        {
            get { return _readConcern; }
            set { _readConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        /// <summary>
        /// Gets or sets whether to use spherical geometry.
        /// </summary>
        public bool? Spherical
        {
            get { return _spherical; }
            set { _spherical = value; }
        }

        /// <summary>
        /// Gets or sets whether to return a document only once.
        /// </summary>
        public bool? UniqueDocs
        {
            get { return _uniqueDocs; }
            set { _uniqueDocs = value; }
        }

        /// <inheritdoc/>
        public TResult Execute(IReadBinding binding, CancellationToken cancellationToken)
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
        public async Task<TResult> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
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

        internal BsonDocument CreateCommand(SemanticVersion serverVersion)
        {
            Feature.ReadConcern.ThrowIfNotSupported(serverVersion, _readConcern);
            Feature.Collation.ThrowIfNotSupported(serverVersion, _collation);

            return new BsonDocument
            {
                { "geoNear", _collectionNamespace.CollectionName },
                { "near", _near },
                { "limit", () => _limit.Value, _limit.HasValue },
                { "maxDistance", () => _maxDistance.Value, _maxDistance.HasValue },
                { "query", _filter, _filter != null },
                { "spherical", () => _spherical.Value, _spherical.HasValue },
                { "distanceMultiplier", () => _distanceMultiplier.Value, _distanceMultiplier.HasValue },
                { "includeLocs", () => _includeLocs.Value, _includeLocs.HasValue },
                { "uniqueDocs", () => _uniqueDocs.Value, _uniqueDocs.HasValue },
                { "maxTimeMS", () => _maxTime.Value.TotalMilliseconds, _maxTime.HasValue },
                { "readConcern", _readConcern.ToBsonDocument(), !_readConcern.IsServerDefault },
                { "collation", () => _collation.ToBsonDocument(), _collation != null }
            };
        }

        private ReadCommandOperation<TResult> CreateOperation(SemanticVersion serverVersion)
        {
            var command = CreateCommand(serverVersion);
            return new ReadCommandOperation<TResult>(
                _collectionNamespace.DatabaseNamespace,
                command,
                _resultSerializer,
                _messageEncoderSettings);
        }
    }
}
