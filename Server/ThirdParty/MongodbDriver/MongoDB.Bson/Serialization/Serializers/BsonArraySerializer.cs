/* Copyright 2010-2015 MongoDB Inc.
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


namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonArrays.
    /// </summary>
    public class BsonArraySerializer : BsonValueSerializerBase<BsonArray>, IBsonArraySerializer
    {
        // private static fields
        private static BsonArraySerializer __instance = new BsonArraySerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonArraySerializer class.
        /// </summary>
        public BsonArraySerializer()
            : base(BsonType.Array)
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonArraySerializer class.
        /// </summary>
        public static BsonArraySerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override BsonArray DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            bsonReader.ReadStartArray();
            var array = new BsonArray();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var item = BsonValueSerializer.Instance.Deserialize(context);
                array.Add(item);
            }
            bsonReader.ReadEndArray();

            return array;
        }

        /// <summary>
        /// Tries to get the serialization info for the individual items of the array.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        ///   <c>true</c> if the serialization info exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
        {
            serializationInfo = new BsonSerializationInfo(
                null,
                BsonValueSerializer.Instance,
                typeof(BsonValue));
            return true;
        }

        // protected methods
        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, BsonArray value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartArray();
            for (int i = 0; i < value.Count; i++)
            {
                BsonValueSerializer.Instance.Serialize(context, value[i]);
            }
            bsonWriter.WriteEndArray();
        }
    }
}
