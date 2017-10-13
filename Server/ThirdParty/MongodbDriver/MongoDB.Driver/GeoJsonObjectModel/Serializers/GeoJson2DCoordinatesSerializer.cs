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

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJson2DCoordinates value.
    /// </summary>
    public class GeoJson2DCoordinatesSerializer : ClassSerializerBase<GeoJson2DCoordinates>
    {
        // private static fields
        private static readonly IBsonSerializer<double> __doubleSerializer = new DoubleSerializer();

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>The value.</returns>
        protected override GeoJson2DCoordinates DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            bsonReader.ReadStartArray();
            var x = __doubleSerializer.Deserialize(context);
            var y = __doubleSerializer.Deserialize(context);
            bsonReader.ReadEndArray();

            return new GeoJson2DCoordinates(x, y);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, GeoJson2DCoordinates value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartArray();
            bsonWriter.WriteDouble(value.X);
            bsonWriter.WriteDouble(value.Y);
            bsonWriter.WriteEndArray();
        }
    }
}
