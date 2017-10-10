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

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// A serializer registry.
    /// </summary>
    public interface IBsonSerializerRegistry
    {
        /// <summary>
        /// Gets the serializer for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The serializer.</returns>
        IBsonSerializer GetSerializer(Type type);

        /// <summary>
        /// Gets the serializer for the specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The serializer.</returns>
        IBsonSerializer<T> GetSerializer<T>();
    }
}
