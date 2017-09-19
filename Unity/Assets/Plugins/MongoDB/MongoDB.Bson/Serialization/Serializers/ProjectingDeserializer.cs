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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Wraps a serializer and projects using a function.
    /// </summary>
    /// <typeparam name="TFrom">The type of from.</typeparam>
    /// <typeparam name="TTo">The type of to.</typeparam>
    public class ProjectingDeserializer<TFrom, TTo> : SerializerBase<TTo>
    {
        // private fields
        private readonly IBsonSerializer<TFrom> _fromSerializer;
        private readonly Func<TFrom, TTo> _projector;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectingDeserializer{TFrom, TTo}"/> class.
        /// </summary>
        /// <param name="fromSerializer">From serializer.</param>
        /// <param name="projector">The projector.</param>
        public ProjectingDeserializer(IBsonSerializer<TFrom> fromSerializer, Func<TFrom, TTo> projector)
        {
            _fromSerializer = fromSerializer;
            _projector = projector;
        }

        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override TTo Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var from = _fromSerializer.Deserialize(context);
            return _projector(from);
        }
    }
}
