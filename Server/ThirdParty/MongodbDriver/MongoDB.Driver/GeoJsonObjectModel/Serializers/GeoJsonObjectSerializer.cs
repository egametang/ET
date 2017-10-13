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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJson object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonObjectSerializer<TCoordinates> : ClassSerializerBase<GeoJsonObject<TCoordinates>> where TCoordinates : GeoJsonCoordinates
    {
        // protected methods
        /// <summary>
        /// Gets the actual type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The actual type.</returns>
        protected override Type GetActualType(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            var bookmark = bsonReader.GetBookmark();
            bsonReader.ReadStartDocument();
            if (bsonReader.FindElement("type"))
            {
                var discriminator = bsonReader.ReadString();
                bsonReader.ReturnToBookmark(bookmark);

                switch (discriminator)
                {
                    case "Feature": return typeof(GeoJsonFeature<TCoordinates>);
                    case "FeatureCollection": return typeof(GeoJsonFeatureCollection<TCoordinates>);
                    case "GeometryCollection": return typeof(GeoJsonGeometryCollection<TCoordinates>);
                    case "LineString": return typeof(GeoJsonLineString<TCoordinates>);
                    case "MultiLineString": return typeof(GeoJsonMultiLineString<TCoordinates>);
                    case "MultiPoint": return typeof(GeoJsonMultiPoint<TCoordinates>);
                    case "MultiPolygon": return typeof(GeoJsonMultiPolygon<TCoordinates>);
                    case "Point": return typeof(GeoJsonPoint<TCoordinates>);
                    case "Polygon": return typeof(GeoJsonPolygon<TCoordinates>);
                    default:
                        var message = string.Format("The type field of the GeoJsonObject is not valid: '{0}'.", discriminator);
                        throw new FormatException(message);
                }
            }
            else
            {
                throw new FormatException("GeoJsonObject is missing the type field.");
            }
        }
    }
}
