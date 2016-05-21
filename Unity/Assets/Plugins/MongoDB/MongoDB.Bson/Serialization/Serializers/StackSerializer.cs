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
using System.Linq;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Stacks.
    /// </summary>
    public class StackSerializer : EnumerableSerializerBase, IBsonArraySerializer
    {
        // private static fields
        private static StackSerializer __instance = new StackSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the StackSerializer class.
        /// </summary>
        public StackSerializer()
            : base(new ArraySerializationOptions())
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the StackSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static StackSerializer Instance
        {
            get { return __instance; }
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="item">The item.</param>
        protected override void AddItem(object instance, object item)
        {
            ((Stack)instance).Push(item);
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The instance.</returns>
        protected override object CreateInstance(Type actualType)
        {
            return new Stack();
        }

        /// <summary>
        /// Enumerates the items.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The items.</returns>
        protected override IEnumerable EnumerateItemsInSerializationOrder(object instance)
        {
            return ((IEnumerable)instance).Cast<object>().Reverse();
        }

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The result.</returns>
        protected override object FinalizeResult(object instance, Type actualType)
        {
            return instance;
        }
    }

    /// <summary>
    /// Represents a serializer for Stacks.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public class StackSerializer<T> : EnumerableSerializerBase<T>, IBsonArraySerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the StackSerializer class.
        /// </summary>
        public StackSerializer()
            : base(new ArraySerializationOptions())
        {
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="item">The item.</param>
        protected override void AddItem(object instance, T item)
        {
            ((Stack<T>)instance).Push(item);
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The instance.</returns>
        protected override object CreateInstance(Type actualType)
        {
            return new Stack<T>();
        }

        /// <summary>
        /// Enumerates the items.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The items.</returns>
        protected override IEnumerable<T> EnumerateItemsInSerializationOrder(object instance)
        {
            return ((IEnumerable<T>)instance).Reverse();
        }

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The result.</returns>
        protected override object FinalizeResult(object instance, Type actualType)
        {
            return instance;
        }
    }
}
