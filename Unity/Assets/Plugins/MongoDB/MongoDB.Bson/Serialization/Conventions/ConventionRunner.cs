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
    /// Runs the conventions against a BsonClassMap and its BsonMemberMaps.
    /// </summary>
    public class ConventionRunner
    {
        // private fields
        private readonly IEnumerable<IConvention> _conventions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConventionRunner" /> class.
        /// </summary>
        /// <param name="conventions">The conventions.</param>
        public ConventionRunner(IConventionPack conventions)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException("conventions");
            }

            _conventions = conventions.Conventions.ToList();
        }

        // public methods
        /// <summary>
        /// Applies a modification to the class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public void Apply(BsonClassMap classMap)
        {
            foreach (var convention in _conventions.OfType<IClassMapConvention>())
            {
                convention.Apply(classMap);
            }

            foreach (var convention in _conventions.OfType<IMemberMapConvention>())
            {
                foreach (var memberMap in classMap.DeclaredMemberMaps)
                {
                    convention.Apply(memberMap);
                }
            }

            foreach (var convention in _conventions.OfType<ICreatorMapConvention>())
            {
                foreach (var creatorMap in classMap.CreatorMaps)
                {
                    convention.Apply(creatorMap);
                }
            }

            foreach (var convention in _conventions.OfType<IPostProcessingConvention>())
            {
                convention.PostProcess(classMap);
            }
        }
    }
}