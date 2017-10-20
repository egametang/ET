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
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Represents a discriminator convention where the discriminator is an array of all the discriminators provided by the class maps of the root class down to the actual type.
    /// </summary>
    public class HierarchicalDiscriminatorConvention : StandardDiscriminatorConvention
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the HierarchicalDiscriminatorConvention class.
        /// </summary>
        /// <param name="elementName">The element name.</param>
        public HierarchicalDiscriminatorConvention(string elementName)
            : base(elementName)
        {
        }

        // public methods
        /// <summary>
        /// Gets the discriminator value for an actual type.
        /// </summary>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The discriminator value.</returns>
        public override BsonValue GetDiscriminator(Type nominalType, Type actualType)
        {
            // TODO: this isn't quite right, not all classes are serialized using  a class map serializer
            var classMap = BsonClassMap.LookupClassMap(actualType);
            if (actualType != nominalType || classMap.DiscriminatorIsRequired || classMap.HasRootClass)
            {
                if (classMap.HasRootClass && !classMap.IsRootClass)
                {
                    var values = new List<BsonValue>();
                    for (; !classMap.IsRootClass; classMap = classMap.BaseClassMap)
                    {
                        values.Add(classMap.Discriminator);
                    }
                    values.Add(classMap.Discriminator); // add the root class's discriminator
                    return new BsonArray(values.Reverse<BsonValue>()); // reverse to put leaf class last
                }
                else
                {
                    return classMap.Discriminator;
                }
            }

            return null;
        }
    }
}