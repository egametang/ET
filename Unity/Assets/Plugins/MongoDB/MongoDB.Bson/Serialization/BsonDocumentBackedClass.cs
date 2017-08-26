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

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// A class backed by a BsonDocument.
    /// </summary>
    public abstract class BsonDocumentBackedClass
    {
        // private fields
        private readonly BsonDocument _backingDocument;
        private readonly IBsonDocumentSerializer _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentBackedClass"/> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        protected BsonDocumentBackedClass(IBsonDocumentSerializer serializer)
            : this(new BsonDocument(), serializer)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentBackedClass"/> class.
        /// </summary>
        /// <param name="backingDocument">The backing document.</param>
        /// <param name="serializer">The serializer.</param>
        protected BsonDocumentBackedClass(BsonDocument backingDocument, IBsonDocumentSerializer serializer)
        {
            if (backingDocument == null)
            {
                throw new ArgumentNullException("backingDocument");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            _backingDocument = backingDocument;
            _serializer = serializer;
        }

        // protected internal properties
        /// <summary>
        /// Gets the backing document.
        /// </summary>
        protected internal BsonDocument BackingDocument
        {
            get { return _backingDocument; }
        }

        // protected methods
        /// <summary>
        /// Gets the value from the backing document.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="memberName">The member name.</param>
        /// <returns>The value.</returns>
        protected T GetValue<T>(string memberName)
        {
            BsonSerializationInfo info;
            if (!_serializer.TryGetMemberSerializationInfo(memberName, out info))
            {
                var message = string.Format("The member {0} does not exist.", memberName);
                throw new ArgumentException(message, "memberName");
            }

            BsonValue bsonValue;
            if (!_backingDocument.TryGetValue(info.ElementName, out bsonValue))
            {
                var message = string.Format("The backing document does not contain an element named '{0}'.", info.ElementName);
                throw new KeyNotFoundException(message);
            }

            return (T)info.DeserializeValue(bsonValue);
        }

        /// <summary>
        /// Gets the value from the backing document.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="memberName">The member name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        protected T GetValue<T>(string memberName, T defaultValue)
        {
            BsonSerializationInfo info;
            if (!_serializer.TryGetMemberSerializationInfo(memberName, out info))
            {
                var message = string.Format("The member {0} does not exist.", memberName);
                throw new ArgumentException(message, "memberName");
            }

            BsonValue bsonValue;
            if (!_backingDocument.TryGetValue(info.ElementName, out bsonValue))
            {
                return defaultValue;
            }

            return (T)info.DeserializeValue(bsonValue);
        }

        /// <summary>
        /// Sets the value in the backing document.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <param name="value">The value.</param>
        protected void SetValue(string memberName, object value)
        {
            BsonSerializationInfo info;
            if (!_serializer.TryGetMemberSerializationInfo(memberName, out info))
            {
                var message = string.Format("The member {0} does not exist.", memberName);
                throw new ArgumentException("memberName", message);
            }

            var bsonValue = info.SerializeValue(value);
            _backingDocument.Set(info.ElementName, bsonValue);
        }
    }
}