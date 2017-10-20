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

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Represents a registry of conventions.
    /// </summary>
    public static class ConventionRegistry
    {
        // private static fields
        private readonly static List<ConventionPackContainer> __conventionPacks = new List<ConventionPackContainer>();
        private readonly static object __lock = new object();

        // static constructors
        static ConventionRegistry()
        {
            Register("__defaults__", DefaultConventionPack.Instance, t => true);
            Register("__attributes__", AttributeConventionPack.Instance, t => true);
        }

        // public static methods
        /// <summary>
        /// Looks up the effective set of conventions that apply to a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The conventions for that type.</returns>
        public static IConventionPack Lookup(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            lock (__lock)
            {
                var pack = new ConventionPack();

                // append any attribute packs (usually just one) at the end so attributes are processed last
                var attributePacks = new List<IConventionPack>();
                foreach (var container in __conventionPacks)
                {
                    if (container.Filter(type))
                    {

                        if (container.Name == "__attributes__")
                        {
                            attributePacks.Add(container.Pack);
                        }
                        else
                        {
                            pack.Append(container.Pack);
                        }
                    }
                }

                foreach (var attributePack in attributePacks)
                {
                    pack.Append(attributePack);
                }

                return pack;
            }
        }

        /// <summary>
        /// Registers the conventions.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="conventions">The conventions.</param>
        /// <param name="filter">The filter.</param>
        public static void Register(string name, IConventionPack conventions, Func<Type, bool> filter)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (conventions == null)
            {
                throw new ArgumentNullException("conventions");
            }
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            lock (__lock)
            {
                var container = new ConventionPackContainer
                {
                    Filter = filter,
                    Name = name,
                    Pack = conventions
                };

                __conventionPacks.Add(container);
            }
        }

        /// <summary>
        /// Removes the conventions specified by the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks>Removing a convention allows the removal of the special __defaults__ conventions 
        /// and the __attributes__ conventions for those who want to completely customize the 
        /// experience.</remarks>
        public static void Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            lock (__lock)
            {
                __conventionPacks.RemoveAll(x => x.Name == name);
            }
        }

        // private class
        private class ConventionPackContainer
        {
            public Func<Type, bool> Filter;
            public string Name;
            public IConventionPack Pack;
        }
    }
}