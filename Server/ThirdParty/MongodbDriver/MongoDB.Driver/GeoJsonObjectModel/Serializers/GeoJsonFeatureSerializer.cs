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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJsonFeature value.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonFeatureSerializer<TCoordinates> : ClassSerializerBase<GeoJsonFeature<TCoordinates>> where TCoordinates : GeoJsonCoordinates
    {
        // private constants
        private static class Flags
        {
            public const long Geometry = 16;
            public const long Id = 32;
            public const long Properties = 64;
        }

        // private fields
        private readonly IBsonSerializer<GeoJsonGeometry<TCoordinates>> _geometrySerializer = BsonSerializer.LookupSerializer<GeoJsonGeometry<TCoordinates>>();
        private readonly GeoJsonObjectSerializerHelper<TCoordinates> _helper;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonFeatureSerializer{TCoordinates}"/> class.
        /// </summary>
        public GeoJsonFeatureSerializer()
        {
            _helper = new GeoJsonObjectSerializerHelper<TCoordinates>
            (
                "Feature",
                new SerializerHelper.Member("geometry", Flags.Geometry),
                new SerializerHelper.Member("id", Flags.Id, isOptional: true),
                new SerializerHelper.Member("properties", Flags.Properties, isOptional: true)
            );
        }

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>The value.</returns>
        protected override GeoJsonFeature<TCoordinates> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var geoJsonFeatureArgs = new GeoJsonFeatureArgs<TCoordinates>();
            GeoJsonGeometry<TCoordinates> geometry = null;

            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Geometry: geometry = DeserializeGeometry(context); break;
                    case Flags.Id: geoJsonFeatureArgs.Id = DeserializeId(context); break;
                    case Flags.Properties: geoJsonFeatureArgs.Properties = DeserializeProperties(context); break;
                    default: _helper.DeserializeBaseMember(context, elementName, flag, geoJsonFeatureArgs); break;
                }
            });

            return new GeoJsonFeature<TCoordinates>(geoJsonFeatureArgs, geometry);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, GeoJsonFeature<TCoordinates> value)
        {
            _helper.SerializeMembers(context, value, SerializeDerivedMembers);
        }

        // private methods
        private GeoJsonGeometry<TCoordinates> DeserializeGeometry(BsonDeserializationContext context)
        {
            return _geometrySerializer.Deserialize(context);
        }

        private BsonValue DeserializeId(BsonDeserializationContext context)
        {
            return BsonValueSerializer.Instance.Deserialize(context);
        }

        private BsonDocument DeserializeProperties(BsonDeserializationContext context)
        {
            return BsonDocumentSerializer.Instance.Deserialize(context);
        }

        private void SerializeDerivedMembers(BsonSerializationContext context, GeoJsonFeature<TCoordinates> value)
        {
            SerializeGeometry(context, value.Geometry);
            SerializeId(context, value.Id);
            SerializeProperties(context, value.Properties);
        }

        private void SerializeGeometry(BsonSerializationContext context, GeoJsonGeometry<TCoordinates> geometry)
        {
            context.Writer.WriteName("geometry");
            _geometrySerializer.Serialize(context, geometry);
        }

        private void SerializeId(BsonSerializationContext context, BsonValue id)
        {
            if (id != null)
            {
                context.Writer.WriteName("id");
                BsonValueSerializer.Instance.Serialize(context, id);
            }
        }

        private void SerializeProperties(BsonSerializationContext context, BsonDocument properties)
        {
            if (properties != null)
            {
                context.Writer.WriteName("properties");
                BsonDocumentSerializer.Instance.Serialize(context, properties);
            }
        }
    }
}
