/* Copyright 2010-2016 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the options for a map-reduce operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class MapReduceOptions<TDocument, TResult>
    {
        // fields
        private bool? _bypassDocumentValidation;
        private Collation _collation;
        private FilterDefinition<TDocument> _filter;
        private BsonJavaScript _finalize;
        private bool? _javaScriptMode;
        private long? _limit;
        private TimeSpan? _maxTime;
        private MapReduceOutputOptions _outputOptions;
        private IBsonSerializer<TResult> _resultSerializer;
        private BsonDocument _scope;
        private SortDefinition<TDocument> _sort;
        private bool? _verbose;

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
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
        /// Gets or sets the filter.
        /// </summary>
        public FilterDefinition<TDocument> Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Gets or sets the finalize function.
        /// </summary>
        public BsonJavaScript Finalize
        {
            get { return _finalize; }
            set { _finalize = value; }
        }

        /// <summary>
        /// Gets or sets the java script mode.
        /// </summary>
        public bool? JavaScriptMode
        {
            get { return _javaScriptMode; }
            set { _javaScriptMode = value; }
        }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        public long? Limit
        {
            get { return _limit; }
            set { _limit = value; }
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
        /// Gets or sets the output options.
        /// </summary>
        public MapReduceOutputOptions OutputOptions
        {
            get { return _outputOptions; }
            set { _outputOptions = value; }
        }

        /// <summary>
        /// Gets or sets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
            set { _resultSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        public BsonDocument Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        /// <summary>
        /// Gets or sets the sort.
        /// </summary>
        public SortDefinition<TDocument> Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }

        /// <summary>
        /// Gets or sets whether to include timing information.
        /// </summary>
        public bool? Verbose
        {
            get { return _verbose; }
            set { _verbose = value; }
        }
    }

    /// <summary>
    /// Represents the output options for a map-reduce operation.
    /// </summary>
    public abstract class MapReduceOutputOptions
    {
        private static MapReduceOutputOptions __inline = new InlineOutput();

        private MapReduceOutputOptions()
        { }

        /// <summary>
        /// An inline map-reduce output options.
        /// </summary>
        public static MapReduceOutputOptions Inline
        {
            get { return __inline; }
        }

        /// <summary>
        /// A merge map-reduce output options.
        /// </summary>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="sharded">Whether the output collection should be sharded.</param>
        /// <param name="nonAtomic">Whether the server should not lock the database for the duration of the merge.</param>
        /// <returns>A merge map-reduce output options.</returns>
        public static MapReduceOutputOptions Merge(string collectionName, string databaseName = null, bool? sharded = null, bool? nonAtomic = null)
        {
            Ensure.IsNotNull(collectionName, nameof(collectionName));
            return new CollectionOutput(collectionName, Core.Operations.MapReduceOutputMode.Merge, databaseName, sharded, nonAtomic);
        }

        /// <summary>
        /// A reduce map-reduce output options.
        /// </summary>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="sharded">Whether the output collection should be sharded.</param>
        /// <param name="nonAtomic">Whether the server should not lock the database for the duration of the reduce.</param>
        /// <returns>A reduce map-reduce output options.</returns>
        public static MapReduceOutputOptions Reduce(string collectionName, string databaseName = null, bool? sharded = null, bool? nonAtomic = null)
        {
            Ensure.IsNotNull(collectionName, nameof(collectionName));
            return new CollectionOutput(collectionName, Core.Operations.MapReduceOutputMode.Reduce, databaseName, sharded, nonAtomic);
        }

        /// <summary>
        /// A replace map-reduce output options.
        /// </summary>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="sharded">Whether the output collection should be sharded.</param>
        /// <returns>A replace map-reduce output options.</returns>
        public static MapReduceOutputOptions Replace(string collectionName, string databaseName = null, bool? sharded = null)
        {
            Ensure.IsNotNull(collectionName, nameof(collectionName));
            return new CollectionOutput(collectionName, Core.Operations.MapReduceOutputMode.Replace, databaseName, sharded, null);
        }

        internal sealed class InlineOutput : MapReduceOutputOptions
        {
            internal InlineOutput()
            { }
        }

        internal sealed class CollectionOutput : MapReduceOutputOptions
        {
            private readonly string _collectionName;
            private readonly string _databaseName;
            private readonly bool? _nonAtomic;
            private readonly Core.Operations.MapReduceOutputMode _outputMode;
            private readonly bool? _sharded;

            internal CollectionOutput(string collectionName, Core.Operations.MapReduceOutputMode outputMode, string databaseName = null, bool? sharded = null, bool? nonAtomic = null)
            {
                _collectionName = collectionName;
                _outputMode = outputMode;
                _databaseName = databaseName;
                _sharded = sharded;
                _nonAtomic = nonAtomic;
            }

            public string CollectionName
            {
                get { return _collectionName; }
            }

            public string DatabaseName
            {
                get { return _databaseName; }
            }

            public bool? NonAtomic
            {
                get { return _nonAtomic; }
            }

            public Core.Operations.MapReduceOutputMode OutputMode
            {
                get { return _outputMode; }
            }

            public bool? Sharded
            {
                get { return _sharded; }
            }
        }
    }
}
