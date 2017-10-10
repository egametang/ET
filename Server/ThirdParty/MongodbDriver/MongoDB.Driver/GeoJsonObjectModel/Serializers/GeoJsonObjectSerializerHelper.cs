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
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer helper for GeoJsonObjects.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonObjectSerializerHelper<TCoordinates> : SerializerHelper where TCoordinates : GeoJsonCoordinates
    {
        // private constants
        private static class Flags
        {
            public const long Type = 1;
            public const long CoordinateReferenceSystem = 2;
            public const long BoundingBox = 4;
            public const long ExtraMember = 8;
        }

        // private fields
        private readonly IBsonSerializer<GeoJsonBoundingBox<TCoordinates>> _boundingBoxSerializer = BsonSerializer.LookupSerializer<GeoJsonBoundingBox<TCoordinates>>();
        private readonly IBsonSerializer<GeoJsonCoordinateReferenceSystem> _coordinateReferenceSystemSerializer = BsonSerializer.LookupSerializer<GeoJsonCoordinateReferenceSystem>();
        private readonly string _type;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonObjectSerializerHelper{TCoordinates}"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="derivedMembers">The derived members.</param>
        public GeoJsonObjectSerializerHelper(string type, params SerializerHelper.Member[] derivedMembers)
            : base(CreateCombinedMembers(derivedMembers))
        {
            _type = type;
        }

        // private static methods
        private static IEnumerable<SerializerHelper.Member> CreateBaseMembers()
        {
            return new[]
            {
                new SerializerHelper.Member("type", Flags.Type),
                new SerializerHelper.Member("bbox", Flags.BoundingBox, isOptional: true),
                new SerializerHelper.Member("crs", Flags.CoordinateReferenceSystem, isOptional: true),
                new SerializerHelper.Member("*", Flags.ExtraMember, isOptional: true)
            };
        }

        private static SerializerHelper.Member[] CreateCombinedMembers(IEnumerable<SerializerHelper.Member> derivedMembers)
        {
            var combinedMembers = CreateBaseMembers().Concat(derivedMembers).ToArray();
            ThrowIfDuplicateMemberFlags(combinedMembers);
            return combinedMembers;
        }

        private static void ThrowIfDuplicateMemberFlags(SerializerHelper.Member[] members)
        {
            var distinctFlags = new HashSet<long>(members.Select(m => m.Flag));
            if (distinctFlags.Count < members.Length)
            {
                throw new BsonInternalException("Duplicate GeoJsonObject member flags.");
            }
        }

        // public methods
        /// <summary>
        /// Deserializes a base member.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="elementName">The element name.</param>
        /// <param name="flag">The flag.</param>
        /// <param name="args">The arguments.</param>
        public void DeserializeBaseMember(BsonDeserializationContext context, string elementName, long flag, GeoJsonObjectArgs<TCoordinates> args)
        {
            switch (flag)
            {
                case Flags.Type: EnsureTypeIsValid(context); break;
                case Flags.CoordinateReferenceSystem: args.CoordinateReferenceSystem = DeserializeCoordinateReferenceSystem(context); break;
                case Flags.BoundingBox: args.BoundingBox = DeserializeBoundingBox(context); break;
                case Flags.ExtraMember: DeserializeExtraMember(context, elementName, args); break;
                default: throw new BsonInternalException();
            }
        }

        /// <summary>
        /// Serializes the members.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializeDerivedMembers">The delegate to serialize the derived members.</param>
        public void SerializeMembers<TValue>(BsonSerializationContext context, TValue value, Action<BsonSerializationContext, TValue> serializeDerivedMembers)
            where TValue : GeoJsonObject<TCoordinates>
        {
            context.Writer.WriteStartDocument();
            SerializeType(context, value.Type);
            SerializeCoordinateReferenceSystem(context, value.CoordinateReferenceSystem);
            SerializeBoundingBox(context, value.BoundingBox);
            serializeDerivedMembers(context, value);
            SerializeExtraMembers(context, value.ExtraMembers);
            context.Writer.WriteEndDocument();
        }

        // private methods
        private GeoJsonBoundingBox<TCoordinates> DeserializeBoundingBox(BsonDeserializationContext context)
        {
            return _boundingBoxSerializer.Deserialize(context);
        }

        private GeoJsonCoordinateReferenceSystem DeserializeCoordinateReferenceSystem(BsonDeserializationContext context)
        {
            return _coordinateReferenceSystemSerializer.Deserialize(context);
        }

        private void DeserializeExtraMember(BsonDeserializationContext context, string elementName, GeoJsonObjectArgs<TCoordinates> args)
        {
            var value = BsonValueSerializer.Instance.Deserialize(context);
            if (args.ExtraMembers == null)
            {
                args.ExtraMembers = new BsonDocument();
            }
            args.ExtraMembers[elementName] = value;
        }

        private void EnsureTypeIsValid(BsonDeserializationContext context)
        {
            var type = context.Reader.ReadString();
            if (type != _type)
            {
                throw new FormatException(string.Format(
                    "Invalid GeoJson type: '{0}'. Expected: '{1}'.", type, _type));
            }
        }

        private void SerializeBoundingBox(BsonSerializationContext context, GeoJsonBoundingBox<TCoordinates> boundingBox)
        {
            if (boundingBox != null)
            {
                context.Writer.WriteName("bbox");
                _boundingBoxSerializer.Serialize(context, boundingBox);
            }
        }

        private void SerializeCoordinateReferenceSystem(BsonSerializationContext context, GeoJsonCoordinateReferenceSystem coordinateReferenceSystem)
        {
            if (coordinateReferenceSystem != null)
            {
                context.Writer.WriteName("crs");
                _coordinateReferenceSystemSerializer.Serialize(context, coordinateReferenceSystem);
            }
        }

        private void SerializeExtraMembers(BsonSerializationContext context, BsonDocument value)
        {
            if (value != null)
            {
                foreach (var element in value)
                {
                    context.Writer.WriteName(element.Name);
                    BsonValueSerializer.Instance.Serialize(context, element.Value);
                }
            }
        }

        private void SerializeType(BsonSerializationContext context, GeoJsonObjectType type)
        {
            context.Writer.WriteString("type", type.ToString());
        }
    }
}
