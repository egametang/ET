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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a base class for map-reduce operations.
    /// </summary>
    public abstract class MapReduceOperationBase
    {
        // fields
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private BsonDocument _filter;
        private BsonJavaScript _finalizeFunction;
        private bool? _javaScriptMode;
        private long? _limit;
        private readonly BsonJavaScript _mapFunction;
        private TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly BsonJavaScript _reduceFunction;
        private BsonDocument _scope;
        private BsonDocument _sort;
        private bool? _verbose;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MapReduceOperationBase"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="mapFunction">The map function.</param>
        /// <param name="reduceFunction">The reduce function.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        protected MapReduceOperationBase(CollectionNamespace collectionNamespace, BsonJavaScript mapFunction, BsonJavaScript reduceFunction, MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _mapFunction = Ensure.IsNotNull(mapFunction, nameof(mapFunction));
            _reduceFunction = Ensure.IsNotNull(reduceFunction, nameof(reduceFunction));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
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
        /// Gets or sets a value indicating whether objects emitted by the map function remain as JavaScript objects.
        /// </summary>
        /// <value>
        /// <remarks>
        /// Setting this value to true can result in faster execution, but requires more memory on the server, and if
        /// there are too many emitted objects the map-reduce operation may fail.
        /// </remarks>
        ///   <c>true</c> if objects emitted by the map function remain as JavaScript objects; otherwise, <c>false</c>.
        /// </value>
        public bool? JavaScriptMode
        {
            get { return _javaScriptMode; }
            set { _javaScriptMode = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of documents to pass to the map function.
        /// </summary>
        /// <value>
        /// The maximum number of documents to pass to the map function.
        /// </value>
        public long? Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        /// <summary>
        /// Gets the map function.
        /// </summary>
        /// <value>
        /// The map function.
        /// </value>
        public BsonJavaScript MapFunction
        {
            get { return _mapFunction; }
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
            set { _maxTime = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
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
        /// Gets or sets the scope document.
        /// </summary>
        /// <remarks>
        /// The scode document defines global variables that are accessible from the map, reduce and finalize functions.
        /// </remarks>
        /// <value>
        /// The scope document.
        /// </value>
        public BsonDocument Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        /// <summary>
        /// Gets or sets the sort specification.
        /// </summary>
        /// <value>
        /// The sort specification.
        /// </value>
        public BsonDocument Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include extra information, such as timing, in the result.
        /// </summary>
        /// <value>
        ///   <c>true</c> if extra information, such as timing, should be included in the result; otherwise, <c>false</c>.
        /// </value>
        public bool? Verbose
        {
            get { return _verbose; }
            set { _verbose = value; }
        }

        // methods
        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <param name="serverVersion">The server version.</param>
        /// <returns>The command.</returns>
        protected internal virtual BsonDocument CreateCommand(SemanticVersion serverVersion)
        {
            Feature.Collation.ThrowIfNotSupported(serverVersion, _collation);

            return new BsonDocument
            {
                { "mapreduce", _collectionNamespace.CollectionName }, // all lowercase command name for backwards compatibility
                { "map", _mapFunction },
                { "reduce", _reduceFunction },
                { "out" , CreateOutputOptions() },
                { "query", _filter, _filter != null },
                { "sort", _sort, _sort != null },
                { "limit", () => _limit.Value, _limit.HasValue },
                { "finalize", _finalizeFunction, _finalizeFunction != null },
                { "scope", _scope, _scope != null },
                { "jsMode", () => _javaScriptMode.Value, _javaScriptMode.HasValue },
                { "verbose", () => _verbose.Value, _verbose.HasValue },
                { "maxTimeMS", () => _maxTime.Value.TotalMilliseconds, _maxTime.HasValue },
                { "collation", () => _collation.ToBsonDocument(), _collation != null }
            };
        }

        /// <summary>
        /// Creates the output options.
        /// </summary>
        /// <returns>The output options.</returns>
        protected abstract BsonDocument CreateOutputOptions();
    }
}
