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
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a create index request.
    /// </summary>
    public class CreateIndexRequest
    {
        // fields
        private BsonDocument _additionalOptions;
        private bool? _background;
        private int? _bits;
        private double? _bucketSize;
        private Collation _collation;
        private string _defaultLanguage;
        private TimeSpan? _expireAfter;
        private string _languageOverride;
        private readonly BsonDocument _keys;
        private double? _max;
        private double? _min;
        private string _name;
        private BsonDocument _partialFilterExpression;
        private bool? _sparse;
        private int? _sphereIndexVersion;
        private BsonDocument _storageEngine;
        private int? _textIndexVersion;
        private bool? _unique;
        private int? _version;
        private BsonDocument _weights;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIndexRequest"/> class.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public CreateIndexRequest(BsonDocument keys)
        {
            _keys = Ensure.IsNotNull(keys, nameof(keys));
        }

        // properties
        /// <summary>
        /// Gets or sets the additional options.
        /// </summary>
        /// <value>
        /// The additional options.
        /// </value>
        public BsonDocument AdditionalOptions
        {
            get { return _additionalOptions; }
            set { _additionalOptions = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the index should be created in the background.
        /// </summary>
        /// <value>
        /// A value indicating whether the index should be created in the background.
        /// </value>
        public bool? Background
        {
            get { return _background; }
            set { _background = value; }
        }

        /// <summary>
        /// Gets or sets the bits of precision of the geohash values for 2d geo indexes.
        /// </summary>
        /// <value>
        /// The bits of precision of the geohash values for 2d geo indexes.
        /// </value>
        public int? Bits
        {
            get { return _bits; }
            set { _bits = value; }
        }

        /// <summary>
        /// Gets or sets the size of the bucket for geo haystack indexes.
        /// </summary>
        /// <value>
        /// The size of the bucket for geo haystack indexes.
        /// </value>
        public double? BucketSize
        {
            get { return _bucketSize; }
            set { _bucketSize = value; }
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
        /// Gets or sets the default language for text indexes.
        /// </summary>
        /// <value>
        /// The default language for text indexes.
        /// </value>
        public string DefaultLanguage
        {
            get { return _defaultLanguage; }
            set { _defaultLanguage = value; }
        }

        /// <summary>
        /// Gets or sets when documents in a TTL collection expire.
        /// </summary>
        /// <value>
        /// When documents in a TTL collection expire.
        /// </value>
        public TimeSpan? ExpireAfter
        {
            get { return _expireAfter; }
            set { _expireAfter = value; }
        }

        /// <summary>
        /// Gets or sets the language override for text indexes.
        /// </summary>
        /// <value>
        /// The language override for text indexes.
        /// </value>
        public string LanguageOverride
        {
            get { return _languageOverride; }
            set { _languageOverride = value; }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public BsonDocument Keys
        {
            get { return _keys; }
        }

        /// <summary>
        /// Gets or sets the maximum coordinate value for 2d indexes.
        /// </summary>
        /// <value>
        /// The maximum coordinate value for 2d indexesThe maximum.
        /// </value>
        public double? Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets or sets the minimum coordinate value for 2d indexes.
        /// </summary>
        /// <value>
        /// The minimum coordinate value for 2d indexes.
        /// </value>
        public double? Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Gets or sets the index name.
        /// </summary>
        /// <value>
        /// The index name.
        /// </value>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the partial filter expression.
        /// </summary>
        /// <value>
        /// The partial filter expression.
        /// </value>
        public BsonDocument PartialFilterExpression
        {
            get { return _partialFilterExpression; }
            set { _partialFilterExpression = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the index is a sparse index.
        /// </summary>
        /// <value>
        /// A value indicating whether the index is a sparse index.
        /// </value>
        public bool? Sparse
        {
            get { return _sparse; }
            set { _sparse = value; }
        }

        /// <summary>
        /// Gets or sets the 2dsphere index version.
        /// </summary>
        /// <value>
        /// The 2dsphere index version.
        /// </value>
        public int? SphereIndexVersion
        {
            get { return _sphereIndexVersion; }
            set { _sphereIndexVersion = value; }
        }

        /// <summary>
        /// Gets or sets the storage engine options.
        /// </summary>
        /// <value>
        /// The storage engine options.
        /// </value>
        public BsonDocument StorageEngine
        {
            get { return _storageEngine; }
            set { _storageEngine = value; }
        }

        /// <summary>
        /// Gets or sets the text index version.
        /// </summary>
        /// <value>
        /// The text index version.
        /// </value>
        public int? TextIndexVersion
        {
            get { return _textIndexVersion; }
            set { _textIndexVersion = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the index enforces the uniqueness of the key values.
        /// </summary>
        /// <value>
        /// A value indicating whether the index enforces the uniqueness of the key values.
        /// </value>
        public bool? Unique
        {
            get { return _unique; }
            set { _unique = value; }
        }

        /// <summary>
        /// Gets or sets the index version.
        /// </summary>
        /// <value>
        /// The index version.
        /// </value>
        public int? Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// Gets or sets the weights for text indexes.
        /// </summary>
        /// <value>
        /// The weights for text indexes.
        /// </value>
        public BsonDocument Weights
        {
            get { return _weights; }
            set { _weights = value; }
        }

        // publuc methods
        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        /// <returns>The name of the index.</returns>
        public string GetIndexName()
        {
            if (_name != null)
            {
                return _name;
            }
            
            if (_additionalOptions != null)
            {
                BsonValue name;
                if (_additionalOptions.TryGetValue("name", out name))
                {
                    return name.AsString;
                }
            }

            return IndexNameHelper.GetIndexName(_keys);
        }

        // methods
        internal BsonDocument CreateIndexDocument(SemanticVersion serverVersion)
        {
            Feature.Collation.ThrowIfNotSupported(serverVersion, _collation);

            var document = new BsonDocument
            {
                { "key", _keys },
                { "name", GetIndexName() },
                { "background", () => _background.Value, _background.HasValue },
                { "bits", () => _bits.Value, _bits.HasValue },
                { "bucketSize", () => _bucketSize.Value, _bucketSize.HasValue },
                { "collation", () => _collation.ToBsonDocument(), _collation != null },
                { "default_language", () => _defaultLanguage, _defaultLanguage != null },
                { "expireAfterSeconds", () => _expireAfter.Value.TotalSeconds, _expireAfter.HasValue },
                { "language_override", () => _languageOverride, _languageOverride != null },
                { "max", () => _max.Value, _max.HasValue },
                { "min", () => _min.Value, _min.HasValue },
                { "partialFilterExpression", _partialFilterExpression, _partialFilterExpression != null },
                { "sparse", () => _sparse.Value, _sparse.HasValue },
                { "2dsphereIndexVersion", () => _sphereIndexVersion.Value, _sphereIndexVersion.HasValue },
                { "storageEngine", () => _storageEngine, _storageEngine != null },
                { "textIndexVersion", () => _textIndexVersion.Value, _textIndexVersion.HasValue },
                { "unique", () => _unique.Value, _unique.HasValue },
                { "v", () => _version.Value, _version.HasValue },
                { "weights", () => _weights, _weights != null }
            };

            if (_additionalOptions != null)
            {
                document.Merge(_additionalOptions, overwriteExistingElements: false);
            }
            return document;
        }
    }
}
