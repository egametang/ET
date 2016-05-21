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
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for CultureInfos.
    /// </summary>
    public class CultureInfoSerializer : BsonBaseSerializer
    {
        // private static fields
        private static CultureInfoSerializer __instance = new CultureInfoSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the CultureInfoSerializer class.
        /// </summary>
        public CultureInfoSerializer()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the CultureInfoSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static CultureInfoSerializer Instance
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
            VerifyTypes(nominalType, actualType, typeof(CultureInfo));

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;
                case BsonType.Document:
                    bsonReader.ReadStartDocument();
                    var name = bsonReader.ReadString("Name");
                    var useUserOverride = bsonReader.ReadBoolean("UseUserOverride");
                    bsonReader.ReadEndDocument();
                    return new CultureInfo(name, useUserOverride);
                case BsonType.String:
                    return new CultureInfo(bsonReader.ReadString());
                default:
                    var message = string.Format("Cannot deserialize CultureInfo from BsonType {0}.", bsonType);
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
            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var cultureInfo = (CultureInfo)value;
                if (cultureInfo.UseUserOverride)
                {
                    // the default for UseUserOverride is true so we don't need to serialize it
                    bsonWriter.WriteString(cultureInfo.Name);
                }
                else
                {
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteString("Name", cultureInfo.Name);
                    bsonWriter.WriteBoolean("UseUserOverride", cultureInfo.UseUserOverride);
                    bsonWriter.WriteEndDocument();
                }
            }
        }
    }
}
