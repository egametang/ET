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
    /// Represents a serializer for a GeoJsonNamedCoordinateReferenceSystem value.
    /// </summary>
    public class GeoJsonNamedCoordinateReferenceSystemSerializer : ClassSerializerBase<GeoJsonNamedCoordinateReferenceSystem>
    {
        // private constants
        private static class Flags
        {
            public const long Type = 1;
            public const long Properties = 2;
        }

        private static class PropertiesFlags
        {
            public const long Name = 1;
        }

        // private fields
        private readonly SerializerHelper _helper;
        private readonly SerializerHelper _propertiesHelper;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonNamedCoordinateReferenceSystemSerializer"/> class.
        /// </summary>
        public GeoJsonNamedCoordinateReferenceSystemSerializer()
        {
            _helper = new SerializerHelper
            (
                new SerializerHelper.Member("type", Flags.Type),
                new SerializerHelper.Member("properties", Flags.Properties)
            );

            _propertiesHelper = new SerializerHelper
            (
                new SerializerHelper.Member("name", PropertiesFlags.Name)
            );
        }

        // protected methods
        /// <summary>
        /// Deserializes a class.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>An object.</returns>
        protected override GeoJsonNamedCoordinateReferenceSystem DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            string type = null, name = null;
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
                                case PropertiesFlags.Name: name = bsonReader.ReadString(); break;
                            }
                        });
                        break;
                }
            });

            if (type != "name")
            {
                var message = string.Format("Expected type to be 'name'.");
                throw new FormatException(message);
            }

            return new GeoJsonNamedCoordinateReferenceSystem(name);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, GeoJsonNamedCoordinateReferenceSystem value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();
            bsonWriter.WriteString("type", "name");
            bsonWriter.WriteStartDocument("properties");
            bsonWriter.WriteString("name", value.Name);
            bsonWriter.WriteEndDocument();
            bsonWriter.WriteEndDocument();
        }
    }
}
