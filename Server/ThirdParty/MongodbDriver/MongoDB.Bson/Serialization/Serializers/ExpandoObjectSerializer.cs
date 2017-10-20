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

using System.Collections.Generic;
using System.Dynamic;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Serializer for <see cref="ExpandoObject"/>.
    /// </summary>
    /// <remarks>
    /// The use of <see cref="ExpandoObject"/> will serialize any <see cref="List{Object}"/> without type information. 
    /// To get the best experience out of using an <see cref="ExpandoObject"/>, any member wanting to be used
    /// as an array should use <see cref="List{Object}"/>.
    /// </remarks>
    public class ExpandoObjectSerializer : DynamicDocumentBaseSerializer<ExpandoObject>
    {
        // private fields
        private readonly IBsonSerializer<List<object>> _listSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpandoObjectSerializer"/> class.
        /// </summary>
        public ExpandoObjectSerializer()
        {
            _listSerializer = BsonSerializer.LookupSerializer<List<object>>();
        }

        /// <summary>
        /// Configures the deserialization context.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void ConfigureDeserializationContext(BsonDeserializationContext.Builder builder)
        {
            builder.DynamicDocumentSerializer = this;
            builder.DynamicArraySerializer = _listSerializer;
        }

        /// <summary>
        /// Configures the serialization context.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void ConfigureSerializationContext(BsonSerializationContext.Builder builder)
        {
            builder.IsDynamicType = t => t == typeof(ExpandoObject) || t == typeof(List<object>);
        }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <returns>
        /// A <see cref="ExpandoObject"/>.
        /// </returns>
        protected override ExpandoObject CreateDocument()
        {
            return new ExpandoObject();
        }

        /// <summary>
        /// Sets the value for the member.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="value">The value.</param>
        protected override void SetValueForMember(ExpandoObject document, string memberName, object value)
        {
            ((IDictionary<string, object>)document)[memberName] = value;
        }

        /// <summary>
        /// Tries to get the value for a member.  Returns true if the member should be serialized.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="memberValue">The member value.</param>
        /// <returns><c>true</c> if the member should be serialized; otherwise <c>false</c>.</returns>
        protected override bool TryGetValueForMember(ExpandoObject value, string memberName, out object memberValue)
        {
            return ((IDictionary<string, object>)value).TryGetValue(memberName, out memberValue);
        }
    }
}