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
using System.Collections;
using System.IO;
using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BitArrays.
    /// </summary>
    public class BitArraySerializer : BsonBaseSerializer
    {
        // private static fields
        private static BitArraySerializer __instance = new BitArraySerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BitArraySerializer class.
        /// </summary>
        public BitArraySerializer()
            : base(new RepresentationSerializationOptions(BsonType.Binary))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BitArraySerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static BitArraySerializer Instance
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
            VerifyTypes(nominalType, actualType, typeof(BitArray));

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            BitArray bitArray;
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;
                case BsonType.Binary:
                    return new BitArray(bsonReader.ReadBytes());
                case BsonType.Document:
                    bsonReader.ReadStartDocument();
                    var length = bsonReader.ReadInt32("Length");
                    var bytes = bsonReader.ReadBytes("Bytes");
                    bsonReader.ReadEndDocument();
                    bitArray = new BitArray(bytes);
                    bitArray.Length = length;
                    return bitArray;
                case BsonType.String:
                    var s = bsonReader.ReadString();
                    bitArray = new BitArray(s.Length);
                    for (int i = 0; i < s.Length; i++)
                    {
                        var c = s[i];
                        switch (c)
                        {
                            case '0':
                                break;
                            case '1':
                                bitArray[i] = true;
                                break;
                            default:
                                throw new Exception("String value is not a valid BitArray.");
                        }
                    }
                    return bitArray;
                default:
                    var message = string.Format("Cannot deserialize Byte[] from BsonType {0}.", bsonType);
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
                var bitArray = (BitArray)value;
                var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

                switch (representationSerializationOptions.Representation)
                {
                    case BsonType.Binary:
                        if ((bitArray.Length % 8) == 0)
                        {
                            bsonWriter.WriteBytes(GetBytes(bitArray));
                        }
                        else
                        {
                            bsonWriter.WriteStartDocument();
                            bsonWriter.WriteInt32("Length", bitArray.Length);
                            bsonWriter.WriteBytes("Bytes", GetBytes(bitArray));
                            bsonWriter.WriteEndDocument();
                        }
                        break;
                    case BsonType.String:
                        var sb = new StringBuilder(bitArray.Length);
                        for (int i = 0; i < bitArray.Length; i++)
                        {
                            sb.Append(bitArray[i] ? '1' : '0');
                        }
                        bsonWriter.WriteString(sb.ToString());
                        break;
                    default:
                        var message = string.Format("'{0}' is not a valid BitArray representation.", representationSerializationOptions.Representation);
                        throw new BsonSerializationException(message);
                }
            }
        }

        // private methods
        private byte[] GetBytes(BitArray bitArray)
        {
            // TODO: is there a more efficient way to do this?
            var bytes = new byte[(bitArray.Length + 7) / 8];
            var i = 0;
            foreach (bool value in bitArray)
            {
                if (value)
                {
                    var index = i / 8;
                    var bit = i % 8;
                    bytes[index] |= (byte)(1 << bit);
                }
                i++;
            }
            return bytes;
        }
    }
}
