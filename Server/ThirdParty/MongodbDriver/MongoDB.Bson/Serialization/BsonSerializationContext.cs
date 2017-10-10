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
    /// Represents all the contextual information needed by a serializer to serialize a value.
    /// </summary>
    public class BsonSerializationContext
    {
        // private fields
        private readonly Func<Type, bool> _isDynamicType;
        private readonly IBsonWriter _writer;

        // constructors
        private BsonSerializationContext(
            IBsonWriter writer,
            Func<Type, bool> isDynamicType)
        {
            _writer = writer;
            _isDynamicType = isDynamicType;
        }

        // public properties
        /// <summary>
        /// Gets a function that, when executed, will indicate whether the type 
        /// is a dynamic type.
        /// </summary>
        public Func<Type, bool> IsDynamicType
        {
            get { return _isDynamicType; }
        }

        /// <summary>
        /// Gets the writer.
        /// </summary>
        /// <value>
        /// The writer.
        /// </value>
        public IBsonWriter Writer
        {
            get { return _writer; }
        }

        // public static methods
        /// <summary>
        /// Creates a root context.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="configurator">The serialization context configurator.</param>
        /// <returns>
        /// A root context.
        /// </returns>
        public static BsonSerializationContext CreateRoot(
            IBsonWriter writer,
            Action<Builder> configurator = null)
        {
            var builder = new Builder(null, writer);
            if (configurator != null)
            {
                configurator(builder);
            }
            return builder.Build();
        }

        /// <summary>
        /// Creates a new context with some values changed.
        /// </summary>
        /// <param name="configurator">The serialization context configurator.</param>
        /// <returns>
        /// A new context.
        /// </returns>
        public BsonSerializationContext With(
            Action<Builder> configurator = null)
        {
            var builder = new Builder(this, _writer);
            if (configurator != null)
            {
                configurator(builder);
            }
            return builder.Build();
        }

        // nested classes
        /// <summary>
        /// Represents a builder for a BsonSerializationContext.
        /// </summary>
        public class Builder
        {
            // private fields
            private Func<Type, bool> _isDynamicType;
            private IBsonWriter _writer;

            // constructors
            internal Builder(BsonSerializationContext other, IBsonWriter writer)
            {
                if (writer == null)
                {
                    throw new ArgumentNullException("writer");
                }

                _writer = writer;
                if (other != null)
                {
                    _isDynamicType = other._isDynamicType;
                }
                else
                {
                    _isDynamicType = t =>
                        (BsonDefaults.DynamicArraySerializer != null && t == BsonDefaults.DynamicArraySerializer.ValueType) ||
                        (BsonDefaults.DynamicDocumentSerializer != null && t == BsonDefaults.DynamicDocumentSerializer.ValueType);
                }
            }

            // properties
            /// <summary>
            /// Gets or sets the function used to determine if a type is a dynamic type.
            /// </summary>
            public Func<Type, bool> IsDynamicType
            {
                get { return _isDynamicType; }
                set { _isDynamicType = value; }
            }

            /// <summary>
            /// Gets the writer.
            /// </summary>
            /// <value>
            /// The writer.
            /// </value>
            public IBsonWriter Writer
            {
                get { return _writer; }
            }

            // public methods
            /// <summary>
            /// Builds the BsonSerializationContext instance.
            /// </summary>
            /// <returns>A BsonSerializationContext.</returns>
            internal BsonSerializationContext Build()
            {
                return new BsonSerializationContext(_writer, _isDynamicType);
            }
        }
    }
}