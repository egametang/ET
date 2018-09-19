/* Copyright 2016 MongoDB Inc.
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
* 
*/

using System;
using System.Linq;
using System.Reflection;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Maps a fully immutable type. This will include anonymous types.
    /// </summary>
    public class ImmutableTypeClassMapConvention : ConventionBase, IClassMapConvention
    {
        /// <inheritdoc />
        public void Apply(BsonClassMap classMap)
        {
            var typeInfo = classMap.ClassType.GetTypeInfo();
            if (typeInfo.IsAbstract)
            {
                return;
            }

            if (typeInfo.GetConstructor(Type.EmptyTypes) != null)
            {
                return;
            }

            foreach (var ctor in typeInfo.GetConstructors())
            {
                var parameters = ctor.GetParameters();
                var properties = typeInfo.GetProperties();
                if (parameters.Length != properties.Length)
                {
                    continue;
                }

                var matches = parameters
                    .GroupJoin(properties,
                        parameter => parameter.Name,
                        property => property.Name,
                        (parameter, props) => new { parameter, properties = props },
                        StringComparer.OrdinalIgnoreCase);

                if (matches.Any(m => m.properties.Count() != 1 || m.properties.ElementAt(0).CanWrite))
                {
                    continue;
                }

                classMap.MapConstructor(ctor);
            }
        }
    }
}
