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
    /// Represents a serializer for a GeoJsonLinkedCoordinateReferenceSystem value.
    /// </summary>
    public class GeoJsonLinkedCoordinateReferenceSystemSerializer : ClassSerializerBase<GeoJsonLinkedCoordinateReferenceSystem>
    {
        // private constants
        private static class Flags
        {
            public const long Type = 1;
            public const long Properties = 2;
        }

        private static class PropertiesFlags
        {
            public const long HRef = 1;
            public const long Type = 2;
        }

        // private fields
        private readonly SerializerHelper _helper;
        private readonly SerializerHelper _propertiesHelper;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonLinkedCoordinateReferenceSystemSerializer"/> class.
        /// </summary>
        public GeoJsonLinkedCoordinateReferenceSystemSerializer()
        {
            _helper = new SerializerHelper
            (
                new SerializerHelper.Member("type", Flags.Type),
                new SerializerHelper.Member("properties", Flags.Properties)
            );

            _propertiesHelper = new SerializerHelper
            (
                new SerializerHelper.Member("href", PropertiesFlags.HRef),
                new SerializerHelper.Member("type", PropertiesFlags.Type, isOptional: true)
            );
        }

        // protected methods
        /// <summary>
        /// Deserializes a class.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>An object.</returns>
        protected override GeoJsonLinkedCoordinateReferenceSystem DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            string type = null, href = null, hrefType = null;
            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Type: type = bsonReader.ReadString(); break;
                    case Flags.Properties:
                        _propertiesHelper.DeserializeMembers(context, (propertiesElementName, propertiesFlag) =>
                        {
                            switch (propertiesFlag)
                            {
                                case PropertiesFlags.HRef: href = bsonReader.ReadString(); break;
                                case PropertiesFlags.Type: hrefType = bsonReader.ReadString(); break;
                            }
                        });
                        break;
                }
            });

            if (type != "link")
            {
                var message = string.Format("Expected type to be 'link'.");
                throw new FormatException(message);
            }

            return new GeoJsonLinkedCoordinateReferenceSystem(href, hrefType);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, GeoJsonLinkedCoordinateReferenceSystem value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();
            bsonWriter.WriteString("type", "link");
            bsonWriter.WriteStartDocument("properties");
            bsonWriter.WriteString("href", value.HRef);
            if (value.HRefType != null)
            {
                bsonWriter.WriteString("type", value.HRefType);
            }
            bsonWriter.WriteEndDocument();
            bsonWriter.WriteEndDocument();
        }
    }
}
