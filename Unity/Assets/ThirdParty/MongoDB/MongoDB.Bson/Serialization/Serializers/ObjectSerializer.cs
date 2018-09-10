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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for objects.
    /// </summary>
    public class ObjectSerializer : ClassSerializerBase<object>
    {
        // private static fields
        private static readonly ObjectSerializer __instance = new ObjectSerializer();

        // private fields
        private readonly IDiscriminatorConvention _discriminatorConvention;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
        /// </summary>
        public ObjectSerializer()
            : this(BsonSerializer.LookupDiscriminatorConvention(typeof(object)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
        /// </summary>
        /// <param name="discriminatorConvention">The discriminator convention.</param>
        /// <exception cref="System.ArgumentNullException">discriminatorConvention</exception>
        public ObjectSerializer(IDiscriminatorConvention discriminatorConvention)
        {
            if (discriminatorConvention == null)
            {
                throw new ArgumentNullException("discriminatorConvention");
            }

            _discriminatorConvention = discriminatorConvention;
        }

        // public static properties
        /// <summary>
        /// Gets the standard instance.
        /// </summary>
        /// <value>
        /// The standard instance.
        /// </value>
        public static ObjectSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Array:
                    if (context.DynamicArraySerializer != null)
                    {
                        return context.DynamicArraySerializer.Deserialize(context);
                    }
                    goto default;

                case BsonType.Binary:
                    var binaryData = bsonReader.ReadBinaryData();
                    var subType = binaryData.SubType;
                    if (subType == BsonBinarySubType.UuidStandard || subType == BsonBinarySubType.UuidLegacy)
                    {
                        return binaryData.ToGuid();
                    }
                    goto default;

                case BsonType.Boolean:
                    return bsonReader.ReadBoolean();

                case BsonType.DateTime:
                    var millisecondsSinceEpoch = bsonReader.ReadDateTime();
                    var bsonDateTime = new BsonDateTime(millisecondsSinceEpoch);
                    return bsonDateTime.ToUniversalTime();

                case BsonType.Decimal128:
                    return bsonReader.ReadDecimal128();

                case BsonType.Document:
                    return DeserializeDiscriminatedValue(context, args);

                case BsonType.Double:
                    return bsonReader.ReadDouble();

                case BsonType.Int32:
                    return bsonReader.ReadInt32();

                case BsonType.Int64:
                    return bsonReader.ReadInt64();

                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;

                case BsonType.ObjectId:
                    return bsonReader.ReadObjectId();

                case BsonType.String:
                    return bsonReader.ReadString();

                default:
                    var message = string.Format("ObjectSerializer does not support BSON type '{0}'.", bsonType);
                    throw new FormatException(message);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                if (actualType == typeof(object))
                {
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteEndDocument();
                }
                else
                {
                    // certain types can be written directly as BSON value
                    // if we're not at the top level document, or if we're using the JsonWriter
                    if (bsonWriter.State == BsonWriterState.Value || bsonWriter is JsonWriter)
                    {
                        switch (Type.GetTypeCode(actualType))
                        {
                            case TypeCode.Boolean:
                                bsonWriter.WriteBoolean((bool)value);
                                return;

                            case TypeCode.DateTime:
                                // TODO: is this right? will lose precision after round trip
                                var bsonDateTime = new BsonDateTime(BsonUtils.ToUniversalTime((DateTime)value));
                                bsonWriter.WriteDateTime(bsonDateTime.MillisecondsSinceEpoch);
                                return;

                            case TypeCode.Double:
                                bsonWriter.WriteDouble((double)value);
                                return;

                            case TypeCode.Int16:
                                // TODO: is this right? will change type to Int32 after round trip
                                bsonWriter.WriteInt32((short)value);
                                return;

                            case TypeCode.Int32:
                                bsonWriter.WriteInt32((int)value);
                                return;

                            case TypeCode.Int64:
                                bsonWriter.WriteInt64((long)value);
                                return;

                            case TypeCode.Object:
                                if (actualType == typeof(Decimal128))
                                {
                                    var decimal128 = (Decimal128)value;
                                    bsonWriter.WriteDecimal128(decimal128);
                                    return;
                                }
                                if (actualType == typeof(Guid))
                                {
                                    var guid = (Guid)value;
                                    var guidRepresentation = bsonWriter.Settings.GuidRepresentation;
                                    var binaryData = new BsonBinaryData(guid, guidRepresentation);
                                    bsonWriter.WriteBinaryData(binaryData);
                                    return;
                                }
                                if (actualType == typeof(ObjectId))
                                {
                                    bsonWriter.WriteObjectId((ObjectId)value);
                                    return;
                                }
                                break;

                            case TypeCode.String:
                                bsonWriter.WriteString((string)value);
                                return;
                        }
                    }

                    SerializeDiscriminatedValue(context, args, value, actualType);
                }
            }
        }

        // private methods
        private object DeserializeDiscriminatedValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var actualType = _discriminatorConvention.GetActualType(bsonReader, typeof(object));
            if (actualType == typeof(object))
            {
                var type = bsonReader.GetCurrentBsonType();
                switch(type)
                {
                    case BsonType.Document:
                        if (context.DynamicDocumentSerializer != null)
                        {
                            return context.DynamicDocumentSerializer.Deserialize(context, args);
                        }
                        break;
                }

                bsonReader.ReadStartDocument();
                bsonReader.ReadEndDocument();
                return new object();
            }
            else
            {
                var serializer = BsonSerializer.LookupSerializer(actualType);
                var polymorphicSerializer = serializer as IBsonPolymorphicSerializer;
                if (polymorphicSerializer != null && polymorphicSerializer.IsDiscriminatorCompatibleWithObjectSerializer)
                {
                    return serializer.Deserialize(context, args);
                }
                else
                {
                    object value = null;
                    var wasValuePresent = false;

                    bsonReader.ReadStartDocument();
                    while (bsonReader.ReadBsonType() != 0)
                    {
                        var name = bsonReader.ReadName();
                        if (name == _discriminatorConvention.ElementName)
                        {
                            bsonReader.SkipValue();
                        }
                        else if (name == "_v")
                        {
                            value = serializer.Deserialize(context);
                            wasValuePresent = true;
                        }
                        else
                        {
                            var message = string.Format("Unexpected element name: '{0}'.", name);
                            throw new FormatException(message);
                        }
                    }
                    bsonReader.ReadEndDocument();

                    if (!wasValuePresent)
                    {
                        throw new FormatException("_v element missing.");
                    }

                    return value;
                }
            }
        }

        private void SerializeDiscriminatedValue(BsonSerializationContext context, BsonSerializationArgs args, object value, Type actualType)
        {
            var serializer = BsonSerializer.LookupSerializer(actualType);

            var polymorphicSerializer = serializer as IBsonPolymorphicSerializer;
            if (polymorphicSerializer != null && polymorphicSerializer.IsDiscriminatorCompatibleWithObjectSerializer)
            {
                serializer.Serialize(context, args, value);
            }
            else
            {
                if (context.IsDynamicType != null && context.IsDynamicType(value.GetType()))
                {
                    args.NominalType = actualType;
                    serializer.Serialize(context, args, value);
                }
                else
                {
                    var bsonWriter = context.Writer;
                    var discriminator = _discriminatorConvention.GetDiscriminator(typeof(object), actualType);

                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteName(_discriminatorConvention.ElementName);
                    BsonValueSerializer.Instance.Serialize(context, discriminator);
                    bsonWriter.WriteName("_v");
                    serializer.Serialize(context, value);
                    bsonWriter.WriteEndDocument();
                }
            }
        }
    }
}
