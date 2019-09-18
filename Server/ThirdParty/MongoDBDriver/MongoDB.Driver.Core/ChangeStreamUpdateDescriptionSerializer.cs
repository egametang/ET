/* Copyright 2017-present MongoDB Inc.
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

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;
using System;

namespace MongoDB.Driver
{
    /// <summary>
    /// A serialize for ChangeStreamUpdateDescription values.
    /// </summary>
    public class ChangeStreamUpdateDescriptionSerializer : SealedClassSerializerBase<ChangeStreamUpdateDescription>
    {
        #region static
        // private static fields
        private static readonly ChangeStreamUpdateDescriptionSerializer __instance = new ChangeStreamUpdateDescriptionSerializer();
        private static readonly IBsonSerializer<string[]> __stringArraySerializer = new ArraySerializer<string>();

        // public static properties
        /// <summary>
        /// Gets a ChangeStreamUpdateDescriptionSerializer.
        /// </summary>
        /// <value>
        /// A ChangeStreamUpdateDescriptionSerializer.
        /// </value>
        public static ChangeStreamUpdateDescriptionSerializer Instance => __instance;
        #endregion

        /// <inheritdoc />
        protected override ChangeStreamUpdateDescription DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            BsonDocument updatedFields = null;
            string[] removedFields = null;

            reader.ReadStartDocument();
            while (reader.ReadBsonType() != 0)
            {
                var fieldName = reader.ReadName();
                switch (fieldName)
                {
                    case "updatedFields":
                        updatedFields = BsonDocumentSerializer.Instance.Deserialize(context);
                        break;

                    case "removedFields":
                        removedFields = __stringArraySerializer.Deserialize(context);
                        break;

                    default:
                        throw new FormatException($"Invalid field name: \"{fieldName}\".");
                }
            }
            reader.ReadEndDocument();

            return new ChangeStreamUpdateDescription(updatedFields, removedFields);
        }

        /// <inheritdoc />
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, ChangeStreamUpdateDescription value)
        {
            var writer = context.Writer;

            writer.WriteStartDocument();
            writer.WriteName("updatedFields");
            BsonDocumentSerializer.Instance.Serialize(context, value.UpdatedFields);
            writer.WriteName("removedFields");
            __stringArraySerializer.Serialize(context, value.RemovedFields);
            writer.WriteEndDocument();
        }
    }
}
