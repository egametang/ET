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

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for creating an index.
    /// </summary>
    public class CreateIndexOptions
    {
        // fields
        private bool? _background;
        private int? _bits;
        private double? _bucketSize;
        private Collation _collation;
        private string _defaultLanguage;
        private TimeSpan? _expireAfter;
        private string _languageOverride;
        private double? _max;
        private double? _min;
        private string _name;
        private bool? _sparse;
        private int? _sphereIndexVersion;
        private BsonDocument _storageEngine;
        private int? _textIndexVersion;
        private bool? _unique;
        private int? _version;
        private BsonDocument _weights;

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether to create the index in the background.
        /// </summary>
        public bool? Background
        {
            get { return _background; }
            set { _background = value; }
        }

        /// <summary>
        /// Gets or sets the precision, in bits, used with geohash indexes.
        /// </summary>
        public int? Bits
        {
            get { return _bits; }
            set { _bits = value; }
        }

        /// <summary>
        /// Gets or sets the size of a geohash bucket.
        /// </summary>
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
        /// Gets or sets the default language.
        /// </summary>
        public string DefaultLanguage
        {
            get { return _defaultLanguage; }
            set { _defaultLanguage = value; }
        }

        /// <summary>
        /// Gets or sets when documents expire (used with TTL indexes).
        /// </summary>
        public TimeSpan? ExpireAfter
        {
            get { return _expireAfter; }
            set { _expireAfter = value; }
        }

        /// <summary>
        /// Gets or sets the language override.
        /// </summary>
        public string LanguageOverride
        {
            get { return _languageOverride; }
            set { _languageOverride = value; }
        }

        /// <summary>
        /// Gets or sets the max value for 2d indexes.
        /// </summary>
        public double? Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets or sets the min value for 2d indexes.
        /// </summary>
        public double? Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Gets or sets the index name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the index is a sparse index.
        /// </summary>
        public bool? Sparse
        {
            get { return _sparse; }
            set { _sparse = value; }
        }

        /// <summary>
        /// Gets or sets the index version for 2dsphere indexes.
        /// </summary>
        public int? SphereIndexVersion
        {
            get { return _sphereIndexVersion; }
            set { _sphereIndexVersion = value; }
        }

        /// <summary>
        /// Gets or sets the storage engine options.
        /// </summary>
        public BsonDocument StorageEngine
        {
            get { return _storageEngine; }
            set { _storageEngine = value; }
        }

        /// <summary>
        /// Gets or sets the index version for text indexes.
        /// </summary>
        public int? TextIndexVersion
        {
            get { return _textIndexVersion; }
            set { _textIndexVersion = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the index is a unique index.
        /// </summary>
        public bool? Unique
        {
            get { return _unique; }
            set { _unique = value; }
        }

        /// <summary>
        /// Gets or sets the version of the index.
        /// </summary>
        public int? Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// Gets or sets the weights for text indexes.
        /// </summary>
        public BsonDocument Weights
        {
            get { return _weights; }
            set { _weights = value; }
        }
    }

    /// <summary>
    /// Options for creating an index.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class CreateIndexOptions<TDocument> : CreateIndexOptions
    {
        #region static
        // internal static methods
        internal static CreateIndexOptions<TDocument> CoercedFrom(CreateIndexOptions options)
        {
            if (options == null)
            {
                return null;
            }

            if (options.GetType() == typeof(CreateIndexOptions))
            {
                return new CreateIndexOptions<TDocument>
                {
                    Background = options.Background,
                    Bits = options.Bits,
                    BucketSize = options.BucketSize,
                    Collation = options.Collation,
                    DefaultLanguage = options.DefaultLanguage,
                    ExpireAfter = options.ExpireAfter,
                    LanguageOverride = options.LanguageOverride,
                    Max = options.Max,
                    Min = options.Min,
                    Name = options.Name,
                    Sparse = options.Sparse,
                    SphereIndexVersion = options.SphereIndexVersion,
                    StorageEngine = options.StorageEngine,
                    TextIndexVersion = options.TextIndexVersion,
                    Unique = options.Unique,
                    Version = options.Version,
                    Weights = options.Weights
                };
            }

            return (CreateIndexOptions<TDocument>)options;
        }
        #endregion

        // private fields
        private FilterDefinition<TDocument> _partialFilterExpression;

        // public properties
        /// <summary>
        /// Gets or sets the partial filter expression.
        /// </summary>
        public FilterDefinition<TDocument> PartialFilterExpression
        {
            get { return _partialFilterExpression; }
            set { _partialFilterExpression = value; }
        }
    }
}
