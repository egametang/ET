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

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a base implementation for the many implementations of IBsonSerializer.
    /// </summary>
    public abstract class BsonBaseSerializer : IBsonSerializer
    {
        // private fields
        private IBsonSerializationOptions _defaultSerializationOptions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBaseSerializer class.
        /// </summary>
        protected BsonBaseSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonBaseSerializer class.
        /// </summary>
        /// <param name="defaultSerializationOptions">The default serialization options for this serializer.</param>
        protected BsonBaseSerializer(IBsonSerializationOptions defaultSerializationOptions)
        {
            if (defaultSerializationOptions != null)
            {
                _defaultSerializationOptions = defaultSerializationOptions.Clone().Freeze();
            }
        }

        // public properties
        /// <summary>
        /// Gets the default serialization options.
        /// </summary>
        public IBsonSerializationOptions DefaultSerializationOptions
        {
            get { return _defaultSerializationOptions; }
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public virtual object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            // override this method to determine actualType if your serializer handles polymorphic data types
            return Deserialize(bsonReader, nominalType, nominalType, options);
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public virtual object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            throw new NotSupportedException("Subclass must implement Deserialize.");
        }

        /// <summary>
        /// Gets the default serialization options for this serializer.
        /// </summary>
        /// <returns>The default serialization options for this serializer.</returns>
        public virtual IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            return _defaultSerializationOptions;
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public virtual void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            throw new NotSupportedException("Subclass must implement Serialize.");
        }

        // protected methods
        /// <summary>
        /// Ensures that the serializer has serialization options of the right type (replacing null with the default serialization options if necessary).
        /// </summary>
        /// <typeparam name="TSerializationOptions">The required serialization options type.</typeparam>
        /// <param name="options">The serialization options.</param>
        /// <returns>The serialization options (or the defaults if null) cast to the required type.</returns>
        protected TSerializationOptions EnsureSerializationOptions<TSerializationOptions>(IBsonSerializationOptions options) where TSerializationOptions : class, IBsonSerializationOptions
        {
            if (options == null)
            {
                options = _defaultSerializationOptions;
            }
            if (options == null)
            {
                var message = string.Format(
                    "Serializer {0} expected serialization options of type {1}, but none were provided.",
                    BsonUtils.GetFriendlyTypeName(this.GetType()),
                    BsonUtils.GetFriendlyTypeName(typeof(TSerializationOptions)));
                throw new BsonSerializationException(message);
            }

            var typedOptions = options as TSerializationOptions;
            if (typedOptions == null)
            {
                var message = string.Format(
                    "Serializer {0} expected serialization options of type {1}, not {2}.",
                    BsonUtils.GetFriendlyTypeName(this.GetType()),
                    BsonUtils.GetFriendlyTypeName(typeof(TSerializationOptions)),
                    BsonUtils.GetFriendlyTypeName(options.GetType()));
                throw new BsonSerializationException(message);
            }

            return typedOptions;
        }

        /// <summary>
        /// Verifies the nominal and actual types against the expected type.
        /// </summary>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="actualType">The actual type.</param>
        /// <param name="expectedType">The expected type.</param>
        protected void VerifyTypes(Type nominalType, Type actualType, Type expectedType)
        {
            if (actualType != expectedType)
            {
                var message = string.Format(
                    "{0} can only be used with type {1}, not with type {2}.",
                    this.GetType().FullName, expectedType.FullName, actualType.FullName);
                throw new BsonSerializationException(message);
            }

            // Type.IsAssignableFrom is extremely expensive, always perform a direct type check before calling Type.IsAssignableFrom
            if (actualType != nominalType && !nominalType.IsAssignableFrom(actualType))
            {
                var message = string.Format(
                    "{0} can only be used with a nominal type that is assignable from the actual type {1}, but nominal type {2} is not.",
                    this.GetType().FullName, actualType.FullName, nominalType.FullName);
                throw new BsonSerializationException(message);
            }
        }
    }
}
