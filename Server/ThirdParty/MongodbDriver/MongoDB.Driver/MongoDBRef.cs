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

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a DBRef (a convenient way to refer to a document).
    /// </summary>
    [BsonSerializer(typeof(MongoDBRefSerializer))]
    public class MongoDBRef : IEquatable<MongoDBRef>
    {
        // private fields
        private string _databaseName;
        private string _collectionName;
        private BsonValue _id;

        // constructors
        // default constructor is private and only used for deserialization
        private MongoDBRef()
        {
        }

        /// <summary>
        /// Creates a MongoDBRef.
        /// </summary>
        /// <param name="collectionName">The name of the collection that contains the document.</param>
        /// <param name="id">The Id of the document.</param>
        public MongoDBRef(string collectionName, BsonValue id)
            : this(null, collectionName, id)
        {
        }

        /// <summary>
        /// Creates a MongoDBRef.
        /// </summary>
        /// <param name="databaseName">The name of the database that contains the document.</param>
        /// <param name="collectionName">The name of the collection that contains the document.</param>
        /// <param name="id">The Id of the document.</param>
        public MongoDBRef(string databaseName, string collectionName, BsonValue id)
        {
            if (collectionName == null)
            {
                throw new ArgumentNullException("collectionName");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            _databaseName = databaseName;
            _collectionName = collectionName;
            _id = id;
        }

        // public properties
        /// <summary>
        /// Gets the name of the database that contains the document.
        /// </summary>
        public string DatabaseName
        {
            get { return _databaseName; }
        }

        /// <summary>
        /// Gets the name of the collection that contains the document.
        /// </summary>
        public string CollectionName
        {
            get { return _collectionName; }
        }

        /// <summary>
        /// Gets the Id of the document.
        /// </summary>
        public BsonValue Id
        {
            get { return _id; }
        }

        // public operators
        /// <summary>
        /// Determines whether two specified MongoDBRef objects have different values.
        /// </summary>
        /// <param name="lhs">The first value to compare, or null.</param>
        /// <param name="rhs">The second value to compare, or null.</param>
        /// <returns>True if the value of lhs is different from the value of rhs; otherwise, false.</returns>
        public static bool operator !=(MongoDBRef lhs, MongoDBRef rhs)
        {
            return !MongoDBRef.Equals(lhs, rhs);
        }

        /// <summary>
        /// Determines whether two specified MongoDBRef objects have the same value.
        /// </summary>
        /// <param name="lhs">The first value to compare, or null.</param>
        /// <param name="rhs">The second value to compare, or null.</param>
        /// <returns>True if the value of lhs is the same as the value of rhs; otherwise, false.</returns>
        public static bool operator ==(MongoDBRef lhs, MongoDBRef rhs)
        {
            return MongoDBRef.Equals(lhs, rhs);
        }

        // public static methods
        /// <summary>
        /// Determines whether two specified MongoDBRef objects have the same value.
        /// </summary>
        /// <param name="lhs">The first value to compare, or null.</param>
        /// <param name="rhs">The second value to compare, or null.</param>
        /// <returns>True if the value of lhs is the same as the value of rhs; otherwise, false.</returns>
        public static bool Equals(MongoDBRef lhs, MongoDBRef rhs)
        {
            if ((object)lhs == null) { return (object)rhs == null; }
            return lhs.Equals(rhs);
        }

        // public methods
        /// <summary>
        /// Determines whether this instance and another specified MongoDBRef object have the same value.
        /// </summary>
        /// <param name="rhs">The MongoDBRef object to compare to this instance.</param>
        /// <returns>True if the value of the rhs parameter is the same as this instance; otherwise, false.</returns>
        public bool Equals(MongoDBRef rhs)
        {
            if ((object)rhs == null || GetType() != rhs.GetType()) { return false; }
            if ((object)this == (object)rhs) { return true; }
            // note: _databaseName can be null
            return string.Equals(_databaseName, rhs._databaseName) && _collectionName.Equals(rhs._collectionName) && _id.Equals(rhs._id);
        }

        /// <summary>
        /// Determines whether this instance and a specified object, which must also be a MongoDBRef object, have the same value.
        /// </summary>
        /// <param name="obj">The MongoDBRef object to compare to this instance.</param>
        /// <returns>True if obj is a MongoDBRef object and its value is the same as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MongoDBRef); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Returns the hash code for this MongoDBRef object.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + ((_databaseName == null) ? 0 : _databaseName.GetHashCode());
            hash = 37 * hash + _collectionName.GetHashCode();
            hash = 37 * hash + _id.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            if (_databaseName == null)
            {
                return string.Format("new MongoDBRef(\"{0}\", {1})", _collectionName, _id);
            }
            else
            {
                return string.Format("new MongoDBRef(\"{0}\", \"{1}\", {2})", _databaseName, _collectionName, _id);
            }
        }
    }

    /// <summary>
    /// Represents a serializer for MongoDBRefs.
    /// </summary>
    public class MongoDBRefSerializer : ClassSerializerBase<MongoDBRef>, IBsonDocumentSerializer
    {
        // private constants
        private static class Flags
        {
            public const long CollectionName = 1;
            public const long Id = 2;
            public const long DatabaseName = 4;
        }

        // private fields
        private readonly SerializerHelper _helper;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDBRefSerializer"/> class.
        /// </summary>
        public MongoDBRefSerializer()
        {
            _helper = new SerializerHelper
            (
                new SerializerHelper.Member("$ref", Flags.CollectionName),
                new SerializerHelper.Member("$id", Flags.Id),
                new SerializerHelper.Member("$db", Flags.DatabaseName, isOptional: true)
            );
        }

        // public methods
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
            string elementName;
            IBsonSerializer serializer;

            switch (memberName)
            {
                case "DatabaseName":
                    elementName = "$db";
                    serializer = new StringSerializer();
                    break;
                case "CollectionName":
                    elementName = "$ref";
                    serializer = new StringSerializer();
                    break;
                case "Id":
                    elementName = "$id";
                    serializer = BsonValueSerializer.Instance;
                    break;
                default:
                    serializationInfo = null;
                    return false;
            }

            serializationInfo = new BsonSerializationInfo(elementName, serializer, serializer.ValueType);
            return true;
        }

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>The value.</returns>
        protected override MongoDBRef DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            string databaseName = null;
            string collectionName = null;
            BsonValue id = null;

            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.CollectionName: collectionName = bsonReader.ReadString(); break;
                    case Flags.Id: id = BsonValueSerializer.Instance.Deserialize(context); break;
                    case Flags.DatabaseName: databaseName = bsonReader.ReadString(); break;
                }
            });

            return new MongoDBRef(databaseName, collectionName, id);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, MongoDBRef value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();
            bsonWriter.WriteString("$ref", value.CollectionName);
            bsonWriter.WriteName("$id");
            BsonValueSerializer.Instance.Serialize(context, value.Id);
            if (value.DatabaseName != null)
            {
                bsonWriter.WriteString("$db", value.DatabaseName);
            }
            bsonWriter.WriteEndDocument();
        }
    }
}
