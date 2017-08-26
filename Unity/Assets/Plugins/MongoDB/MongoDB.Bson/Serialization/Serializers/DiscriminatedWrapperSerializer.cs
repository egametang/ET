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
    /// Represents a serializer that serializes values as a discriminator/value pair.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class DiscriminatedWrapperSerializer<TValue> : SerializerBase<TValue>
    {
        // private constants
        private static class Flags
        {
            public const long Discriminator = 1;
            public const long Value = 2;
            public const long Other = 4;
        }

        // private fields
        private readonly IDiscriminatorConvention _discriminatorConvention;
        private readonly SerializerHelper _helper;
        private readonly SerializerHelper _isPositionedHelper;
        private readonly IBsonSerializer<TValue> _wrappedSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscriminatedWrapperSerializer{TValue}" /> class.
        /// </summary>
        /// <param name="discriminatorConvention">The discriminator convention.</param>
        /// <param name="wrappedSerializer">The wrapped serializer.</param>
        public DiscriminatedWrapperSerializer(IDiscriminatorConvention discriminatorConvention, IBsonSerializer<TValue> wrappedSerializer)
        {
            _discriminatorConvention = discriminatorConvention;
            _wrappedSerializer = wrappedSerializer;

            _helper = new SerializerHelper
            (
                new SerializerHelper.Member(discriminatorConvention.ElementName, Flags.Discriminator),
                new SerializerHelper.Member("_v", Flags.Value)
            );

            _isPositionedHelper = new SerializerHelper
            (
                new SerializerHelper.Member(discriminatorConvention.ElementName, Flags.Discriminator, isOptional: true),
                new SerializerHelper.Member("_v", Flags.Value, isOptional: true),
                new SerializerHelper.Member("*", Flags.Other, isOptional: true)
            );
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            var nominalType = args.NominalType;
            var actualType = _discriminatorConvention.GetActualType(bsonReader, nominalType);
            var serializer = BsonSerializer.LookupSerializer(actualType);

            TValue value = default(TValue);
            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Discriminator:
                        bsonReader.SkipValue();
                        break;
                    case Flags.Value:
                        var valueDeserializationArgs = new BsonDeserializationArgs { NominalType = actualType };
                        value = (TValue)serializer.Deserialize(context, valueDeserializationArgs);
                        break;
                }
            });

            return value;
        }

        /// <summary>
        /// Determines whether the reader is positioned at a discriminated wrapper.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>True if the reader is positioned at a discriminated wrapper.</returns>
        public bool IsPositionedAtDiscriminatedWrapper(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            var bookmark = bsonReader.GetBookmark();

            try
            {
                if (bsonReader.GetCurrentBsonType() != BsonType.Document) { return false; }
                var foundFields = _isPositionedHelper.DeserializeMembers(context, (elementName, flag) =>
                {
                    context.Reader.SkipValue();
                });
                return foundFields == (Flags.Discriminator | Flags.Value);
            }
            finally
            {
                bsonReader.ReturnToBookmark(bookmark);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            var bsonWriter = context.Writer;
            var nominalType = args.NominalType;
            var actualType = value.GetType();
            var discriminator = _discriminatorConvention.GetDiscriminator(nominalType, actualType);

            bsonWriter.WriteStartDocument();
            bsonWriter.WriteName(_discriminatorConvention.ElementName);
            BsonValueSerializer.Instance.Serialize(context, discriminator);
            bsonWriter.WriteName("_v");
            args.NominalType = actualType;
            _wrappedSerializer.Serialize(context, args, value);
            bsonWriter.WriteEndDocument();
        }
    }
}
