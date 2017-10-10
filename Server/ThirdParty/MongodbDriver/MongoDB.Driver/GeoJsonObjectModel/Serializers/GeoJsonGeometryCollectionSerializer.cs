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

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJsonGeometryCollection value.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonGeometryCollectionSerializer<TCoordinates> : ClassSerializerBase<GeoJsonGeometryCollection<TCoordinates>> where TCoordinates : GeoJsonCoordinates
    {
        // private constants
        private static class Flags
        {
            public const long Geometries = 16;
        }

        // private fields
        private readonly IBsonSerializer<GeoJsonGeometry<TCoordinates>> _geometrySerializer = BsonSerializer.LookupSerializer<GeoJsonGeometry<TCoordinates>>();
        private readonly GeoJsonObjectSerializerHelper<TCoordinates> _helper;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonGeometryCollectionSerializer{TCoordinates}"/> class.
        /// </summary>
        public GeoJsonGeometryCollectionSerializer()
        {
            _helper = new GeoJsonObjectSerializerHelper<TCoordinates>
            (
                "GeometryCollection",
                new SerializerHelper.Member("geometries", Flags.Geometries)
            );
        }

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>The value.</returns>
        protected override GeoJsonGeometryCollection<TCoordinates> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var geoJsonObjectArgs = new GeoJsonObjectArgs<TCoordinates>();
            List<GeoJsonGeometry<TCoordinates>> geometries = null;

            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Geometries: geometries = DeserializeGeometries(context); break;
                    default: _helper.DeserializeBaseMember(context, elementName, flag, geoJsonObjectArgs); break;
                }
            });

            return new GeoJsonGeometryCollection<TCoordinates>(geoJsonObjectArgs, geometries);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, GeoJsonGeometryCollection<TCoordinates> value)
        {
            _helper.SerializeMembers(context, value, SerializeDerivedMembers);
        }

        // private methods
        private List<GeoJsonGeometry<TCoordinates>> DeserializeGeometries(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;

            bsonReader.ReadStartArray();
            var geometries = new List<GeoJsonGeometry<TCoordinates>>();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var geometry = _geometrySerializer.Deserialize(context);
                geometries.Add(geometry);
            }
            bsonReader.ReadEndArray();

            return geometries;
        }

        private void SerializeDerivedMembers(BsonSerializationContext context, GeoJsonGeometryCollection<TCoordinates> value)
        {
            SerializeGeometries(context, value.Geometries);
        }

        private void SerializeGeometries(BsonSerializationContext context, IEnumerable<GeoJsonGeometry<TCoordinates>> geometries)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteName("geometries");
            bsonWriter.WriteStartArray();
            foreach (var geometry in geometries)
            {
                _geometrySerializer.Serialize(context, geometry);
            }
            bsonWriter.WriteEndArray();
        }
    }
}
