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
using System.Collections;
using System.Collections.Generic;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A mutable pack of conventions.
    /// </summary>
    public class ConventionPack : IConventionPack, IEnumerable<IConvention>
    {
        // private fields
        private readonly List<IConvention> _conventions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConventionPack" /> class.
        /// </summary>
        public ConventionPack()
        {
            _conventions = new List<IConvention>();
        }

        // public properties
        /// <summary>
        /// Gets the conventions.
        /// </summary>
        public IEnumerable<IConvention> Conventions
        {
            get { return _conventions; }
        }

        // public methods
        /// <summary>
        /// Adds the specified convention.
        /// </summary>
        /// <param name="convention">The convention.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void Add(IConvention convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException("convention");
            }

            _conventions.Add(convention);
        }

        /// <summary>
        /// Adds a class map convention created using the specified action upon a class map.
        /// </summary>
        /// <param name="name">The name of the convention.</param>
        /// <param name="action">The action the convention should take upon the class map.</param>
        public void AddClassMapConvention(string name, Action<BsonClassMap> action)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            Add(new DelegateClassMapConvention(name, action));
        }

        /// <summary>
        /// Adds a member map convention created using the specified action upon a member map.
        /// </summary>
        /// <param name="name">The name of the convention.</param>
        /// <param name="action">The action the convention should take upon the member map.</param>
        public void AddMemberMapConvention(string name, Action<BsonMemberMap> action)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            Add(new DelegateMemberMapConvention(name, action));
        }

        /// <summary>
        /// Adds a post processing convention created using the specified action upon a class map.
        /// </summary>
        /// <param name="name">The name of the convention.</param>
        /// <param name="action">The action the convention should take upon the class map.</param>
        public void AddPostProcessingConvention(string name, Action<BsonClassMap> action)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            Add(new DelegatePostProcessingConvention(name, action));
        }

        /// <summary>
        /// Adds a range of conventions.
        /// </summary>
        /// <param name="conventions">The conventions.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void AddRange(IEnumerable<IConvention> conventions)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException("conventions");
            }

            _conventions.AddRange(conventions);
        }

        /// <summary>
        /// Appends the conventions in another pack to the end of this pack.
        /// </summary>
        /// <param name="other">The other pack.</param>
        public void Append(IConventionPack other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            AddRange(other.Conventions);
        }

        /// <summary>
        /// Gets an enumerator for the conventions.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<IConvention> GetEnumerator()
        {
            return _conventions.GetEnumerator();
        }

        /// <summary>
        /// Inserts the convention after another convention specified by the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="convention">The convention.</param>
        public void InsertAfter(string name, IConvention convention)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (convention == null)
            {
                throw new ArgumentNullException("convention");
            }

            var index = _conventions.FindIndex(x => x.Name == name);
            if (index == -1)
            {
                var message = string.Format("Unable to find a convention by the name of '{0}'.", name);
                throw new ArgumentOutOfRangeException("name", message);
            }

            _conventions.Insert(index + 1, convention);
        }

        /// <summary>
        /// Inserts the convention before another convention specified by the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="convention">The convention.</param>
        public void InsertBefore(string name, IConvention convention)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (convention == null)
            {
                throw new ArgumentNullException("convention");
            }

            var index = _conventions.FindIndex(x => x.Name == name);
            if (index == -1)
            {
                var message = string.Format("Unable to find a convention by the name of '{0}'.", name);
                throw new ArgumentOutOfRangeException("name", message);
            }

            _conventions.Insert(index, convention);
        }

        /// <summary>
        /// Removes the named convention.
        /// </summary>
        /// <param name="name">The name of the convention.</param>
        public void Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            _conventions.RemoveAll(x => x.Name == name);
        }

        // explicit interface implementations
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _conventions.GetEnumerator();
        }
    }
}