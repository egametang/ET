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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for creating a collection.
    /// </summary>
    public class CreateCollectionOptions
    {
        // fields
        private bool? _autoIndexId;
        private bool? _capped;
        private Collation _collation;
        private IndexOptionDefaults _indexOptionDefaults;
        private long? _maxDocuments;
        private long? _maxSize;
        private bool? _noPadding;
        private BsonDocument _storageEngine;
        private bool? _usePowerOf2Sizes;
        private IBsonSerializerRegistry _serializerRegistry;
        private DocumentValidationAction? _validationAction;
        private DocumentValidationLevel? _validationLevel;

        // properties
        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically create an index on the _id.
        /// </summary>
        public bool? AutoIndexId
        {
            get { return _autoIndexId; }
            set { _autoIndexId = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the collection is capped.
        /// </summary>
        public bool? Capped
        {
            get { return _capped; }
            set { _capped = value; }
        }

        /// <summary>
        /// Gets or sets the index option defaults.
        /// </summary>
        /// <value>
        /// The index option defaults.
        /// </value>
        public IndexOptionDefaults IndexOptionDefaults
        {
            get { return _indexOptionDefaults; }
            set { _indexOptionDefaults = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of documents (used with capped collections).
        /// </summary>
        public long? MaxDocuments
        {
            get { return _maxDocuments; }
            set { _maxDocuments = value; }
        }

        /// <summary>
        /// Gets or sets the maximum size of the collection (used with capped collections).
        /// </summary>
        public long? MaxSize
        {
            get { return _maxSize; }
            set { _maxSize = value; }
        }

        /// <summary>
        /// Gets or sets whether padding should not be used.
        /// </summary>
        public bool? NoPadding
        {
            get { return _noPadding; }
            set { _noPadding = value; }
        }

        /// <summary>
        /// Gets or sets the serializer registry.
        /// </summary>
        public IBsonSerializerRegistry SerializerRegistry
        {
            get { return _serializerRegistry; }
            set { _serializerRegistry = value; }
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
        /// Gets or sets a value indicating whether to use power of 2 sizes.
        /// </summary>
        public bool? UsePowerOf2Sizes
        {
            get { return _usePowerOf2Sizes; }
            set { _usePowerOf2Sizes = value; }
        }

        /// <summary>
        /// Gets or sets the validation action.
        /// </summary>
        /// <value>
        /// The validation action.
        /// </value>
        public DocumentValidationAction? ValidationAction
        {
            get { return _validationAction; }
            set { _validationAction = value; }
        }

        /// <summary>
        /// Gets or sets the validation level.
        /// </summary>
        /// <value>
        /// The validation level.
        /// </value>
        public DocumentValidationLevel? ValidationLevel
        {
            get { return _validationLevel; }
            set { _validationLevel = value; }
        }
    }

    /// <summary>
    /// Options for creating a collection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class CreateCollectionOptions<TDocument> : CreateCollectionOptions
    {
        #region static
        // internal static methods
        /// <summary>
        /// Coerces a generic CreateCollectionOptions{TDocument} from a non-generic CreateCollectionOptions.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The generic options.</returns>
        internal static CreateCollectionOptions<TDocument> CoercedFrom(CreateCollectionOptions options)
        {
            if (options == null)
            {
                return null;
            }

            if (options.GetType() == typeof(CreateCollectionOptions))
            {
                return new CreateCollectionOptions<TDocument>
                {
                    AutoIndexId = options.AutoIndexId,
                    Capped = options.Capped,
                    Collation = options.Collation,
                    IndexOptionDefaults = options.IndexOptionDefaults,
                    MaxDocuments = options.MaxDocuments,
                    MaxSize = options.MaxSize,
                    NoPadding = options.NoPadding,
                    SerializerRegistry = options.SerializerRegistry,
                    StorageEngine = options.StorageEngine,
                    UsePowerOf2Sizes = options.UsePowerOf2Sizes,
                    ValidationAction = options.ValidationAction,
                    ValidationLevel = options.ValidationLevel
                };
            }

            return (CreateCollectionOptions<TDocument>)options;
        }
        #endregion

        // private fields
        private IBsonSerializer<TDocument> _documentSerializer;
        private FilterDefinition<TDocument> _validator;

        // public properties
        /// <summary>
        /// Gets or sets the document serializer.
        /// </summary>
        public IBsonSerializer<TDocument> DocumentSerializer
        {
            get { return _documentSerializer; }
            set { _documentSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the validator.
        /// </summary>
        /// <value>
        /// The validator.
        /// </value>
        public FilterDefinition<TDocument> Validator
        {
            get { return _validator; }
            set { _validator = value; }
        }
    }
}
