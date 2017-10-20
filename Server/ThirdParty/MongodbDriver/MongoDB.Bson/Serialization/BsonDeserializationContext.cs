/* Copyright 2010-2014 MongoDB Inc.
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
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents all the contextual information needed by a serializer to deserialize a value.
    /// </summary>
    public class BsonDeserializationContext
    {
        // private fields
        private readonly bool _allowDuplicateElementNames;
        private readonly IBsonSerializer _dynamicArraySerializer;
        private readonly IBsonSerializer _dynamicDocumentSerializer;
        private readonly IBsonReader _reader;

        // constructors
        private BsonDeserializationContext(
            IBsonReader reader,
            bool allowDuplicateElementNames,
            IBsonSerializer dynamicArraySerializer,
            IBsonSerializer dynamicDocumentSerializer)
        {
            _reader = reader;
            _allowDuplicateElementNames = allowDuplicateElementNames;
            _dynamicArraySerializer = dynamicArraySerializer;
            _dynamicDocumentSerializer = dynamicDocumentSerializer;
        }

        // public properties
        /// <summary>
        /// Gets a value indicating whether to allow duplicate element names.
        /// </summary>
        /// <value>
        /// <c>true</c> if duplicate element names shoud be allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowDuplicateElementNames
        {
            get { return _allowDuplicateElementNames; }
        }

        /// <summary>
        /// Gets the dynamic array serializer.
        /// </summary>
        /// <value>
        /// The dynamic array serializer.
        /// </value>
        public IBsonSerializer DynamicArraySerializer
        {
            get { return _dynamicArraySerializer; }
        }

        /// <summary>
        /// Gets the dynamic document serializer.
        /// </summary>
        /// <value>
        /// The dynamic document serializer.
        /// </value>
        public IBsonSerializer DynamicDocumentSerializer
        {
            get { return _dynamicDocumentSerializer; }
        }

        /// <summary>
        /// Gets the reader.
        /// </summary>
        /// <value>
        /// The reader.
        /// </value>
        public IBsonReader Reader
        {
            get { return _reader; }
        }

        // public static methods
        /// <summary>
        /// Creates a root context.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>
        /// A root context.
        /// </returns>
        public static BsonDeserializationContext CreateRoot(
            IBsonReader reader, 
            Action<Builder> configurator = null)
        {
            var builder = new Builder(null, reader);
            if (configurator != null)
            {
                configurator(builder);
            }
            return builder.Build();
        }

        // public methods
        /// <summary>
        /// Creates a new context with some values changed.
        /// </summary>
        /// <param name="configurator">The configurator.</param>
        /// <returns>
        /// A new context.
        /// </returns>
        public BsonDeserializationContext With(
            Action<Builder> configurator = null)
        {
            var builder = new Builder(this, _reader);
            if (configurator != null)
            {
                configurator(builder);
            }
            return builder.Build();
        }

        // nested classes
        /// <summary>
        /// Represents a builder for a BsonDeserializationContext.
        /// </summary>
        public class Builder
        {
            // private fields
            private bool _allowDuplicateElementNames;
            private IBsonSerializer _dynamicArraySerializer;
            private IBsonSerializer _dynamicDocumentSerializer;
            private IBsonReader _reader;

            // constructors
            internal Builder(BsonDeserializationContext other, IBsonReader reader)
            {
                if (reader == null)
                {
                    throw new ArgumentNullException("reader");
                }

                _reader = reader;
                if (other != null)
                {
                    _allowDuplicateElementNames = other.AllowDuplicateElementNames;
                    _dynamicArraySerializer = other.DynamicArraySerializer;
                    _dynamicDocumentSerializer = other.DynamicDocumentSerializer;
                }
                else
                {
                    _dynamicArraySerializer = BsonDefaults.DynamicArraySerializer;
                    _dynamicDocumentSerializer = BsonDefaults.DynamicDocumentSerializer;
                }
            }

            // properties
            /// <summary>
            /// Gets or sets a value indicating whether to allow duplicate element names.
            /// </summary>
            /// <value>
            /// <c>true</c> if duplicate element names should be allowed; otherwise, <c>false</c>.
            /// </value>
            public bool AllowDuplicateElementNames
            {
                get { return _allowDuplicateElementNames; }
                set { _allowDuplicateElementNames = value; }
            }

            /// <summary>
            /// Gets or sets the dynamic array serializer.
            /// </summary>
            /// <value>
            /// The dynamic array serializer.
            /// </value>
            public IBsonSerializer DynamicArraySerializer
            {
                get { return _dynamicArraySerializer; }
                set { _dynamicArraySerializer = value; }
            }

            /// <summary>
            /// Gets or sets the dynamic document serializer.
            /// </summary>
            /// <value>
            /// The dynamic document serializer.
            /// </value>
            public IBsonSerializer DynamicDocumentSerializer
            {
                get { return _dynamicDocumentSerializer; }
                set { _dynamicDocumentSerializer = value; }
            }

            /// <summary>
            /// Gets the reader.
            /// </summary>
            /// <value>
            /// The reader.
            /// </value>
            public IBsonReader Reader
            {
                get { return _reader; }
            }

            // public methods
            /// <summary>
            /// Builds the BsonDeserializationContext instance.
            /// </summary>
            /// <returns>A BsonDeserializationContext.</returns>
            internal BsonDeserializationContext Build()
            {
                return new BsonDeserializationContext(_reader, _allowDuplicateElementNames, _dynamicArraySerializer, _dynamicDocumentSerializer);
            }
        }
    }
}