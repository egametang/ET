/* Copyright 2010-present MongoDB Inc.
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
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents a serializer for TClass (a subclass of BsonDocumentBackedClass).
    /// </summary>
    /// <typeparam name="TClass">The subclass of BsonDocumentBackedClass.</typeparam>
    public abstract class BsonDocumentBackedClassSerializer<TClass> : ClassSerializerBase<TClass>, IBsonDocumentSerializer
        where TClass : BsonDocumentBackedClass
    {
        // private fields
        private readonly Dictionary<string, BsonSerializationInfo> _memberSerializationInfo;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentBackedClassSerializer{TClass}"/> class.
        /// </summary>
        protected BsonDocumentBackedClassSerializer()
        {
            _memberSerializationInfo = new Dictionary<string, BsonSerializationInfo>();
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
        public virtual bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            return _memberSerializationInfo.TryGetValue(memberName, out serializationInfo);
        }

        // protected methods
        /// <inheritdoc />
        protected override TClass DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var backingDocument = BsonDocumentSerializer.Instance.Deserialize(context);
            return CreateInstance(backingDocument);
        }

        /// <summary>
        /// Registers a member.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <param name="elementName">The element name.</param>
        /// <param name="serializer">The serializer.</param>
        protected void RegisterMember(string memberName, string elementName, IBsonSerializer serializer)
        {
            if (memberName == null)
            {
                throw new ArgumentNullException("memberName");
            }
            if (elementName == null)
            {
                throw new ArgumentNullException("elementName");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            var info = new BsonSerializationInfo(elementName, serializer, serializer.ValueType);
            _memberSerializationInfo.Add(memberName, info);
        }

        /// <inheritdoc />
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, TClass value)
        {
            var backingDocument = ((BsonDocumentBackedClass)value).BackingDocument;
            BsonDocumentSerializer.Instance.Serialize(context, backingDocument);
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="backingDocument">The backing document.</param>
        /// <returns>An instance of TClass.</returns>
        protected abstract TClass CreateInstance(BsonDocument backingDocument);
    }
}
