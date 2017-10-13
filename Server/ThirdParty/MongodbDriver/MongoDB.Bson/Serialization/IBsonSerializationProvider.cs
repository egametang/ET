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

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// An interface implemented by serialization providers.
    /// </summary>
    public interface IBsonSerializationProvider
    {
        /// <summary>
        /// Gets a serializer for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A serializer.</returns>
        IBsonSerializer GetSerializer(Type type);
    }

    /// <summary>
    /// An interface implemented by serialization providers that are aware of registries.
    /// </summary>
    /// <remarks>
    /// This interface was added to preserve backward compatability (changing IBsonSerializationProvider would have been a backward breaking change).
    /// </remarks>
    public interface IRegistryAwareBsonSerializationProvider : IBsonSerializationProvider
    {
        /// <summary>
        /// Gets a serializer for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>
        /// A serializer.
        /// </returns>
        IBsonSerializer GetSerializer(Type type, IBsonSerializerRegistry serializerRegistry);
    }
}
