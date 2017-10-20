/* Copyright 2013-2015 MongoDB Inc.
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

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a deserializer for find and modify result values.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class FindAndModifyValueDeserializer<TResult> : SerializerBase<TResult>
    {
        // fields
        private readonly IBsonSerializer<TResult> _resultSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FindAndModifyValueDeserializer{TResult}"/> class.
        /// </summary>
        /// <param name="valueSerializer">The value serializer.</param>
        public FindAndModifyValueDeserializer(IBsonSerializer<TResult> valueSerializer)
        {
            _resultSerializer = new ElementDeserializer<TResult>("value", valueSerializer, deserializeNull: false);
        }

        // methods
        /// <inheritdoc/>
        public override TResult Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return _resultSerializer.Deserialize(context);
        }
    }
}
