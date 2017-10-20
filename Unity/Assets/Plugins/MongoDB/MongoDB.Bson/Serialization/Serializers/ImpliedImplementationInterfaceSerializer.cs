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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Interfaces.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    public class ImpliedImplementationInterfaceSerializer<TInterface, TImplementation> :
        SerializerBase<TInterface>,
        IBsonArraySerializer,
        IBsonDictionarySerializer,
        IBsonDocumentSerializer,
        IChildSerializerConfigurable
            where TImplementation : class, TInterface
    {
        // private fields
        private readonly Lazy<IBsonSerializer<TImplementation>> _lazyImplementationSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ImpliedImplementationInterfaceSerializer{TInterface, TImplementation}"/> class.
        /// </summary>
        public ImpliedImplementationInterfaceSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImpliedImplementationInterfaceSerializer{TInterface, TImplementation}"/> class.
        /// </summary>
        /// <param name="implementationSerializer">The implementation serializer.</param>
        public ImpliedImplementationInterfaceSerializer(IBsonSerializer<TImplementation> implementationSerializer)
            : this(new Lazy<IBsonSerializer<TImplementation>>(() => implementationSerializer))
        {
            if (implementationSerializer == null)
            {
                throw new ArgumentNullException("implementationSerializer");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImpliedImplementationInterfaceSerializer{TInterface, TImplementation}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public ImpliedImplementationInterfaceSerializer(IBsonSerializerRegistry serializerRegistry)
            : this(new Lazy<IBsonSerializer<TImplementation>>(() => serializerRegistry.GetSerializer<TImplementation>()))
        {
            if (serializerRegistry == null)
            {
                throw new ArgumentNullException("serializerRegistry");
            }
        }

        private ImpliedImplementationInterfaceSerializer(Lazy<IBsonSerializer<TImplementation>> lazyImplementationSerializer)
        {
            var interfaceTypeInfo = typeof(TInterface).GetTypeInfo();
            if (!interfaceTypeInfo.IsInterface)
            {
                var message = string.Format("{0} is not an interface.", typeof(TInterface).FullName);
                throw new ArgumentException(message, "<TInterface>");
            }

            _lazyImplementationSerializer = lazyImplementationSerializer;
        }

        // public properties
        /// <summary>
        /// Gets the dictionary representation.
        /// </summary>
        /// <value>
        /// The dictionary representation.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public DictionaryRepresentation DictionaryRepresentation
        {
            get
            {
                var dictionarySerializer = _lazyImplementationSerializer.Value as IBsonDictionarySerializer;
                if (dictionarySerializer != null)
                {
                    return dictionarySerializer.DictionaryRepresentation;
                }

                var message = string.Format(
                    "{0} does not have a DictionaryRepresentation.",
                    BsonUtils.GetFriendlyTypeName(_lazyImplementationSerializer.Value.GetType()));
                throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Gets the key serializer.
        /// </summary>
        /// <value>
        /// The key serializer.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public IBsonSerializer KeySerializer
        {
            get
            {
                var dictionarySerializer = _lazyImplementationSerializer.Value as IBsonDictionarySerializer;
                if (dictionarySerializer != null)
                {
                    return dictionarySerializer.KeySerializer;
                }

                var message = string.Format(
                    "{0} does not have a KeySerializer.",
                    BsonUtils.GetFriendlyTypeName(_lazyImplementationSerializer.Value.GetType()));
                throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Gets the implementation serializer.
        /// </summary>
        /// <value>
        /// The implementation serializer.
        /// </value>
        public IBsonSerializer<TImplementation> ImplementationSerializer
        {
            get { return _lazyImplementationSerializer.Value; }
        }

        /// <summary>
        /// Gets the value serializer.
        /// </summary>
        /// <value>
        /// The value serializer.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        public IBsonSerializer ValueSerializer
        {
            get
            {
                var dictionarySerializer = _lazyImplementationSerializer.Value as IBsonDictionarySerializer;
                if (dictionarySerializer != null)
                {
                    return dictionarySerializer.ValueSerializer;
                }

                var message = string.Format(
                    "{0} does not have a ValueSerializer.",
                    BsonUtils.GetFriendlyTypeName(_lazyImplementationSerializer.Value.GetType()));
                throw new NotSupportedException(message);
            }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        /// <exception cref="System.FormatException"></exception>
        public override TInterface Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            if (bsonReader.GetCurrentBsonType() == BsonType.Null)
            {
                bsonReader.ReadNull();
                return default(TInterface);
            }
            else
            {
                return _lazyImplementationSerializer.Value.Deserialize(context);
            }
        }

        /// <summary>
        /// Tries to get the serialization info for the individual items of the array.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        ///   <c>true</c> if the serialization info exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
        {
            var arraySerializer = _lazyImplementationSerializer.Value as IBsonArraySerializer;
            if (arraySerializer != null)
            {
                return arraySerializer.TryGetItemSerializationInfo(out serializationInfo);
            }

            serializationInfo = null;
            return false;
        }

        /// <summary>
        /// Tries to get the serialization info for a member.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        ///   <c>true</c> if the serialization info exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            var documentSerializer = _lazyImplementationSerializer.Value as IBsonDocumentSerializer;
            if (documentSerializer != null)
            {
                return documentSerializer.TryGetMemberSerializationInfo(memberName, out serializationInfo);
            }

            serializationInfo = null;
            return false;
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The document.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TInterface value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                if (actualType == typeof(TImplementation))
                {
                    _lazyImplementationSerializer.Value.Serialize(context, (TImplementation)value);
                }
                else
                {
                    var serializer = BsonSerializer.LookupSerializer(actualType);
                    serializer.Serialize(context, value);
                }
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified implementation serializer.
        /// </summary>
        /// <param name="implementationSerializer">The implementation serializer.</param>
        /// <returns>
        /// The reconfigured serializer.
        /// </returns>
        public ImpliedImplementationInterfaceSerializer<TInterface, TImplementation> WithImplementationSerializer(IBsonSerializer<TImplementation> implementationSerializer)
        {
            if (implementationSerializer == ImplementationSerializer)
            {
                return this;
            }
            else
            {
                return new ImpliedImplementationInterfaceSerializer<TInterface, TImplementation>(implementationSerializer);
            }
        }

        // explicit interface implementations
        IBsonSerializer IChildSerializerConfigurable.ChildSerializer
        {
            get { return ImplementationSerializer; }
        }

        IBsonSerializer IChildSerializerConfigurable.WithChildSerializer(IBsonSerializer childSerializer)
        {
            return WithImplementationSerializer((IBsonSerializer<TImplementation>)childSerializer);
        }
    }
}