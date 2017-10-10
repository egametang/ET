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
    /// Represents a serializer for a GeoJsonFeatureCollection value.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonFeatureCollectionSerializer<TCoordinates> : ClassSerializerBase<GeoJsonFeatureCollection<TCoordinates>> where TCoordinates : GeoJsonCoordinates
    {
        // private constants
        private static class Flags
        {
            public const long Features = 16;
        }

        // private fields
        private readonly IBsonSerializer<GeoJsonFeature<TCoordinates>> _featureSerializer = BsonSerializer.LookupSerializer<GeoJsonFeature<TCoordinates>>();
        private readonly GeoJsonObjectSerializerHelper<TCoordinates> _helper;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonFeatureCollectionSerializer{TCoordinates}"/> class.
        /// </summary>
        public GeoJsonFeatureCollectionSerializer()
        {
            _helper = new GeoJsonObjectSerializerHelper<TCoordinates>
            (
                "FeatureCollection",
                new SerializerHelper.Member("features", Flags.Features)
            );
        }

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>The value.</returns>
        protected override GeoJsonFeatureCollection<TCoordinates> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var geoJsonObjectArgs = new GeoJsonObjectArgs<TCoordinates>();
            List<GeoJsonFeature<TCoordinates>> features = null;

            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Features: features = DeserializeFeatures(context); break;
                    default: _helper.DeserializeBaseMember(context, elementName, flag, geoJsonObjectArgs); break;
                }
            });

            return new GeoJsonFeatureCollection<TCoordinates>(geoJsonObjectArgs, features);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, GeoJsonFeatureCollection<TCoordinates> value)
        {
            _helper.SerializeMembers(context, value, SerializeDerivedMembers);
        }

        // private methods
        private List<GeoJsonFeature<TCoordinates>> DeserializeFeatures(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;

            bsonReader.ReadStartArray();
            var features = new List<GeoJsonFeature<TCoordinates>>();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var feature = _featureSerializer.Deserialize(context);
                features.Add(feature);
            }
            bsonReader.ReadEndArray();

            return features;
        }

        private void SerializeDerivedMembers(BsonSerializationContext context, GeoJsonFeatureCollection<TCoordinates> value)
        {
            SerializeFeatures(context, value.Features);
        }

        private void SerializeFeatures(BsonSerializationContext context, IEnumerable<GeoJsonFeature<TCoordinates>> features)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteName("features");
            bsonWriter.WriteStartArray();
            foreach (var feature in features)
            {
                _featureSerializer.Serialize(context, feature);
            }
            bsonWriter.WriteEndArray();
        }
    }
}
