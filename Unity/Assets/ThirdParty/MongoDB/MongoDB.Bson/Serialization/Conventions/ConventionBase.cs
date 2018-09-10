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
    /// Base class for a convention.
    /// </summary>
    public abstract class ConventionBase : IConvention
    {
        // private fields
        private readonly string _name;

        // constructors
        /// <summary>
        /// Initializes a new instance of the ConventionBase class.
        /// </summary>
        protected ConventionBase()
        {
            _name = GetName(this.GetType());
        }

        /// <summary>
        /// Initializes a new instance of the ConventionBase class.
        /// </summary>
        /// <param name="name">The name of the convention.</param>
        protected ConventionBase(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            _name = name;
        }

        // public properties
        /// <summary>
        /// Gets the name of the convention.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        // private static methods
        private static string GetName(Type type)
        {
            var name = type.Name;
            if (name.EndsWith("Convention"))
            {
                return name.Substring(0, name.Length - 10);
            }

            return name;
        }
    }
}