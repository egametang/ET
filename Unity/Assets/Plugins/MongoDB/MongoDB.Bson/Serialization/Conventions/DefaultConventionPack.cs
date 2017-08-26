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

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Convention pack of defaults.
    /// </summary>
    public class DefaultConventionPack : IConventionPack
    {
        // private static fields
        private static readonly IConventionPack __defaultConventionPack = new DefaultConventionPack();

        // private fields
        private readonly IEnumerable<IConvention> _conventions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConventionPack" /> class.
        /// </summary>
        private DefaultConventionPack()
        {
            _conventions = new List<IConvention>
            {
                new ReadWriteMemberFinderConvention(),
                new NamedIdMemberConvention(new [] { "Id", "id", "_id" }),
                new NamedExtraElementsMemberConvention(new [] { "ExtraElements" }),
                new IgnoreExtraElementsConvention(false),
                new ImmutableTypeClassMapConvention(),
                new NamedParameterCreatorMapConvention(),
                new StringObjectIdIdGeneratorConvention(), // should be before LookupIdGeneratorConvention
                new LookupIdGeneratorConvention()
            };
        }

        // public static properties
        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static IConventionPack Instance
        {
            get { return __defaultConventionPack; }
        }

        // public properties
        /// <summary>
        /// Gets the conventions.
        /// </summary>
        public IEnumerable<IConvention> Conventions
        {
            get { return _conventions; }
        }
    }
}