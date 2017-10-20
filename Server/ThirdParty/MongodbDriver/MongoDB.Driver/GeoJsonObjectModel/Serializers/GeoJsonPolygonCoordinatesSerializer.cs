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

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJsonPolygonCoordinates value.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonPolygonCoordinatesSerializer<TCoordinates> : ClassSerializerBase<GeoJsonPolygonCoordinates<TCoordinates>> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private readonly IBsonSerializer<GeoJsonLinearRingCoordinates<TCoordinates>> _linearRingCoordinatesSerializer = BsonSerializer.LookupSerializer<GeoJsonLinearRingCoordinates<TCoordinates>>();

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>The value.</returns>
        protected override GeoJsonPolygonCoordinates<TCoordinates> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            var holes = new List<GeoJsonLinearRingCoordinates<TCoordinates>>();

            bsonReader.ReadStartArray();
            var exterior = _linearRingCoordinatesSerializer.Deserialize(context);
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var hole = _linearRingCoordinatesSerializer.Deserialize(context);
                holes.Add(hole);
            }
            bsonReader.ReadEndArray();

            return new GeoJsonPolygonCoordinates<TCoordinates>(exterior, holes);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, GeoJsonPolygonCoordinates<TCoordinates> value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartArray();
            _linearRingCoordinatesSerializer.Serialize(context, value.Exterior);
            foreach (var hole in value.Holes)
            {
                _linearRingCoordinatesSerializer.Serialize(context, hole);
            }
            bsonWriter.WriteEndArray();
        }
    }
}
