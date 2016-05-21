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
    /// Represents the class map serialization provider.
    /// </summary>
    internal class BsonClassMapSerializationProvider : IBsonSerializationProvider
    {
        /// <summary>
        /// Gets the serializer for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The serializer.</returns>
        public IBsonSerializer GetSerializer(Type type)
        {
            if ((type.IsClass || (type.IsValueType && !type.IsPrimitive)) &&
                !typeof(Array).IsAssignableFrom(type) &&
                !typeof(Enum).IsAssignableFrom(type))
            {
                var classMap = BsonClassMap.LookupClassMap(type);
                return new BsonClassMapSerializer(classMap);
            }

            return null;
        }
    }
}
