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
using System.IO;
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonValue members of a BsonClassMap.
    /// </summary>
    public class BsonValueCSharpNullSerializer : BsonBaseSerializer, IBsonArraySerializer, IBsonDocumentSerializer
    {
        // private fields
        private static BsonValueCSharpNullSerializer __instance = new BsonValueCSharpNullSerializer();

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonBooleanSerializer class.
        /// </summary>
        public static BsonValueCSharpNullSerializer Instance
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
            var bsonType = bsonReader.GetCurrentBsonType();
            if (bsonType == BsonType.Document && IsCSharpNullRepresentation(bsonReader))
            {
                // if IsCSharpNullRepresentation returns true it will have consumed the document representing C# null
                return null;
            }

            // handle BSON null for backward compatibility with existing data (new data would have _csharpnull)
            if (bsonType == BsonType.Null && (nominalType != typeof(BsonValue) && nominalType != typeof(BsonNull)))
            {
                bsonReader.ReadNull();
                return null;
            }

            var serializer = BsonSerializer.LookupSerializer(actualType);
            return serializer.Deserialize(bsonReader, nominalType, actualType, options);
        }

        /// <summary>
        /// Gets the serialization info for a member.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <returns>
        /// The serialization info for the member.
        /// </returns>
        public BsonSerializationInfo GetMemberSerializationInfo(string memberName)
        {
            return BsonValueSerializer.Instance.GetMemberSerializationInfo(memberName);
        }

        /// <summary>
        /// Gets the serialization info for individual items of the array.
        /// </summary>
        /// <returns>
        /// The serialization info for the items.
        /// </returns>
        public BsonSerializationInfo GetItemSerializationInfo()
        {
            return BsonValueSerializer.Instance.GetItemSerializationInfo();
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
            if (value == null)
            {
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteBoolean("_csharpnull", true);
                bsonWriter.WriteEndDocument();
            }
            else
            {
                BsonValueSerializer.Instance.Serialize(bsonWriter, nominalType, value, options);
            }
        }

        // private methods
        private bool IsCSharpNullRepresentation(BsonReader bsonReader)
        {
            var bookmark = bsonReader.GetBookmark();
            bsonReader.ReadStartDocument();
            var bsonType = bsonReader.ReadBsonType();
            if (bsonType == BsonType.Boolean)
            {
                var name = bsonReader.ReadName();
                if (name == "_csharpnull" || name == "$csharpnull")
                {
                    var value = bsonReader.ReadBoolean();
                    if (value)
                    {
                        bsonType = bsonReader.ReadBsonType();
                        if (bsonType == BsonType.EndOfDocument)
                        {
                            bsonReader.ReadEndDocument();
                            return true;
                        }
                    }
                }
            }

            bsonReader.ReturnToBookmark(bookmark);
            return false;
        }
    }
}
