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
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJsonCoordinateReferenceSystem value.
    /// </summary>
    public class GeoJsonCoordinateReferenceSystemSerializer : ClassSerializerBase<GeoJsonCoordinateReferenceSystem>
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
                var type = bsonReader.ReadString();
                bsonReader.ReturnToBookmark(bookmark);

                switch (type)
                {
                    case "link": return typeof(GeoJsonLinkedCoordinateReferenceSystem);
                    case "name": return typeof(GeoJsonNamedCoordinateReferenceSystem);
                    default:
                        var message = string.Format("The type field of the GeoJsonCoordinateReferenceSystem is not valid: '{0}'.", type);
                        throw new FormatException(message);
                }
            }
            else
            {
                throw new FormatException("GeoJsonCoordinateReferenceSystem object is missing the type field.");
            }
        }
    }
}
