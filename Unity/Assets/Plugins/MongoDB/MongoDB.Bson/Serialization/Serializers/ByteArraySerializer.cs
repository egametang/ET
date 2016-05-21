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
using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for ByteArrays.
    /// </summary>
    public class ByteArraySerializer : BsonBaseSerializer
    {
        // private static fields
        private static ByteArraySerializer __instance = new ByteArraySerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the ByteArraySerializer class.
        /// </summary>
        public ByteArraySerializer()
            : base(new RepresentationSerializationOptions(BsonType.Binary))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the ByteArraySerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static ByteArraySerializer Instance
        {
            get { return __instance; }
        }

        // public methods
#pragma warning disable 618 // about obsolete BsonBinarySubType.OldBinary
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
            VerifyTypes(nominalType, actualType, typeof(byte[]));

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            byte[] bytes;
            string message;
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;
                case BsonType.Binary:
                    bytes = bsonReader.ReadBytes();
                    return bytes;
                case BsonType.String:
                    var s = bsonReader.ReadString();
                    if ((s.Length % 2) != 0)
                    {
                        s = "0" + s; // prepend a zero to make length even
                    }
                    bytes = new byte[s.Length / 2];
                    for (int i = 0; i < s.Length; i += 2)
                    {
                        var hex = s.Substring(i, 2);
                        var b = byte.Parse(hex, NumberStyles.HexNumber);
                        bytes[i / 2] = b;
                    }
                    return bytes;
                default:
                    message = string.Format("Cannot deserialize Byte[] from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }
        }
#pragma warning restore 618

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
            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var bytes = (byte[])value;
                var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

                switch (representationSerializationOptions.Representation)
                {
                    case BsonType.Binary:
                        bsonWriter.WriteBytes(bytes);
                        break;
                    case BsonType.String:
                        var sb = new StringBuilder(bytes.Length * 2);
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            sb.Append(string.Format("{0:x2}", bytes[i]));
                        }
                        bsonWriter.WriteString(sb.ToString());
                        break;
                    default:
                        var message = string.Format("'{0}' is not a valid Byte[] representation.", representationSerializationOptions.Representation);
                        throw new BsonSerializationException(message);
                }
            }
        }
    }
}
