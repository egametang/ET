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

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A post processing convention that wraps a delegate.
    /// </summary>
    public class DelegatePostProcessingConvention : ConventionBase, IPostProcessingConvention
    {
        // private fields
        private readonly Action<BsonClassMap> _action;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatePostProcessingConvention" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The delegate.</param>
        public DelegatePostProcessingConvention(string name, Action<BsonClassMap> action)
            : base(name)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            _action = action;
        }

        // public methods
        /// <summary>
        /// Applies a post processing modification to the class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public void PostProcess(BsonClassMap classMap)
        {
            _action(classMap);
        }
    }
}
