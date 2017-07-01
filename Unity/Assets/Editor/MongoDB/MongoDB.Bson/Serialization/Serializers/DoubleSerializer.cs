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
using System.Globalization;
using System.IO;
using System.Xml;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Doubles.
    /// </summary>
    public class DoubleSerializer : BsonBaseSerializer
    {
        // private static fields
        private static DoubleSerializer __instance = new DoubleSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the DoubleSerializer class.
        /// </summary>
        public DoubleSerializer()
            : base(new RepresentationSerializationOptions(BsonType.Double))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the DoubleSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static DoubleSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof(double));
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Double:
                    return bsonReader.ReadDouble();
                case BsonType.Int32:
                    return representationSerializationOptions.ToDouble(bsonReader.ReadInt32());
                case BsonType.Int64:
                    return representationSerializationOptions.ToDouble(bsonReader.ReadInt64());
                case BsonType.String:
                    return XmlConvert.ToDouble(bsonReader.ReadString());
                default:
                    var message = string.Format("Cannot deserialize Double from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            var doubleValue = (double)value;
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            switch (representationSerializationOptions.Representation)
            {
                case BsonType.Double:
                    bsonWriter.WriteDouble(doubleValue);
                    break;
                case BsonType.Int32:
                    bsonWriter.WriteInt32(representationSerializationOptions.ToInt32(doubleValue));
                    break;
                case BsonType.Int64:
                    bsonWriter.WriteInt64(representationSerializationOptions.ToInt64(doubleValue));
                    break;
                case BsonType.String:
                    bsonWriter.WriteString(doubleValue.ToString("R", NumberFormatInfo.InvariantInfo));
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid Double representation.", representationSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
            }
        }
    }
}
