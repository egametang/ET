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

using System.Collections.Generic;
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for a BsonValue that can round trip C# null.
    /// </summary>
    /// <typeparam name="TBsonValue">The type of the BsonValue.</typeparam>
    public class BsonValueCSharpNullSerializer<TBsonValue> : SerializerBase<TBsonValue> where TBsonValue : BsonValue
    {
        // private fields
        private readonly IBsonSerializer<TBsonValue> _wrappedSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonValueCSharpNullSerializer{TBsonValue}"/> class.
        /// </summary>
        /// <param name="wrappedSerializer">The wrapped serializer.</param>
        public BsonValueCSharpNullSerializer(IBsonSerializer<TBsonValue> wrappedSerializer)
        {
            _wrappedSerializer = wrappedSerializer;
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override TBsonValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var bsonType = bsonReader.GetCurrentBsonType();
            if (bsonType == BsonType.Document && IsCSharpNullRepresentation(bsonReader))
            {
                // if IsCSharpNullRepresentation returns true it will have consumed the document representing C# null
                return null;
            }

            // handle BSON null for backward compatibility with existing data (new data would have _csharpnull)
            if (bsonType == BsonType.Null && (args.NominalType != typeof(BsonValue) && args.NominalType != typeof(BsonNull)))
            {
                bsonReader.ReadNull();
                return null;
            }

            return _wrappedSerializer.Deserialize(context);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TBsonValue value)
        {
            if (value == null)
            {
                var bsonWriter = context.Writer;
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteBoolean("_csharpnull", true);
                bsonWriter.WriteEndDocument();
            }
            else
            {
                _wrappedSerializer.Serialize(context, value);
            }
        }

        // private methods
        private bool IsCSharpNullRepresentation(IBsonReader bsonReader)
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

    /// <summary>
    /// Represents a serializer for a BsonValue that can round trip C# null and implements IBsonArraySerializer and IBsonDocumentSerializer.
    /// </summary>
    /// <typeparam name="TBsonValue">The type of the bson value.</typeparam>
    public class BsonValueCSharpNullArrayAndDocumentSerializer<TBsonValue> : BsonValueCSharpNullSerializer<TBsonValue>, IBsonArraySerializer, IBsonDocumentSerializer
        where TBsonValue : BsonValue
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonValueCSharpNullArrayAndDocumentSerializer{TBsonValue}"/> class.
        /// </summary>
        /// <param name="wrappedSerializer">The wrapped serializer.</param>
        public BsonValueCSharpNullArrayAndDocumentSerializer(IBsonSerializer<TBsonValue> wrappedSerializer)
            : base(wrappedSerializer)
        {
        }

        /// <summary>
        /// Tries to get the serialization info for the individual items of the array.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        /// The serialization info for the items.
        /// </returns>
        public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
        {
            return BsonValueSerializer.Instance.TryGetItemSerializationInfo(out serializationInfo);
        }

        /// <summary>
        /// Tries to get the serialization info for a member.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        ///   <c>true</c> if the serialization info exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            return BsonValueSerializer.Instance.TryGetMemberSerializationInfo(memberName, out serializationInfo);
        }
    }

    /// <summary>
    /// Represents a serializer for a BsonValue that can round trip C# null and implements IBsonArraySerializer.
    /// </summary>
    /// <typeparam name="TBsonValue">The type of the bson value.</typeparam>
    public class BsonValueCSharpNullArraySerializer<TBsonValue> : BsonValueCSharpNullSerializer<TBsonValue>, IBsonArraySerializer
         where TBsonValue : BsonValue
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonValueCSharpNullArraySerializer{TBsonValue}"/> class.
        /// </summary>
        /// <param name="wrappedSerializer">The wrapped serializer.</param>
        public BsonValueCSharpNullArraySerializer(IBsonSerializer<TBsonValue> wrappedSerializer)
            : base(wrappedSerializer)
        {
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
            return BsonValueSerializer.Instance.TryGetItemSerializationInfo(out serializationInfo);
        }
    }

    /// <summary>
    /// Represents a serializer for a BsonValue that can round trip C# null and implements IBsonDocumentSerializer.
    /// </summary>
    /// <typeparam name="TBsonValue">The type of the bson value.</typeparam>
    public class BsonValueCSharpNullDocumentSerializer<TBsonValue> : BsonValueCSharpNullSerializer<TBsonValue>, IBsonDocumentSerializer
          where TBsonValue : BsonValue
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonValueCSharpNullDocumentSerializer{TBsonValue}"/> class.
        /// </summary>
        /// <param name="wrappedSerializer">The wrapped serializer.</param>
        public BsonValueCSharpNullDocumentSerializer(IBsonSerializer<TBsonValue> wrappedSerializer)
            : base(wrappedSerializer)
        {
        }

        /// <summary>
        /// Tries to get the serialization info for a member.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        ///   <c>true</c> if the serialization info exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            return BsonValueSerializer.Instance.TryGetMemberSerializationInfo(memberName, out serializationInfo);
        }
    }
}
