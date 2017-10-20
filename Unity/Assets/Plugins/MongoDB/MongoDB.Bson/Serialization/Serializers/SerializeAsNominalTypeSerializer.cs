/* Copyright 2010-2015 MongoDB Inc.
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

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for a class that will be serialized as if it were one of its base classes.
    /// </summary>
    /// <typeparam name="TActualType">The actual type.</typeparam>
    /// <typeparam name="TNominalType">The nominal type.</typeparam>
    public class SerializeAsNominalTypeSerializer<TActualType, TNominalType> : SerializerBase<TActualType> where TActualType : class, TNominalType
    {
        // private fields
        private readonly Lazy<IBsonSerializer<TNominalType>> _lazyNominalTypeSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeAsNominalTypeSerializer{TActualType, TNominalType}"/> class.
        /// </summary>
        public SerializeAsNominalTypeSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeAsNominalTypeSerializer{TActualType, TNominalType}"/> class.
        /// </summary>
        /// <param name="nominalTypeSerializer">The base class serializer.</param>
        public SerializeAsNominalTypeSerializer(IBsonSerializer<TNominalType> nominalTypeSerializer)
        {
            if (nominalTypeSerializer == null)
            {
                throw new ArgumentNullException("nominalTypeSerializer");
            }

            _lazyNominalTypeSerializer = new Lazy<IBsonSerializer<TNominalType>>(() => nominalTypeSerializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeAsNominalTypeSerializer{TActualType, TNominalType}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public SerializeAsNominalTypeSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null)
            {
                throw new ArgumentNullException("serializerRegistry");
            }

            _lazyNominalTypeSerializer = new Lazy<IBsonSerializer<TNominalType>>(() => serializerRegistry.GetSerializer<TNominalType>());
        }

        // public methods
        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TActualType value)
        {
            if (value == null)
            {
                var bsonWriter = context.Writer;
                bsonWriter.WriteNull();
            }
            else
            {
                args.NominalType = typeof(TNominalType);
                args.SerializeAsNominalType = true;
                _lazyNominalTypeSerializer.Value.Serialize(context, args, value);
            }
        }
    }
}
