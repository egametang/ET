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
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for enumerable values.
    /// </summary>
    public class EnumerableSerializer : EnumerableSerializerBase, IBsonArraySerializer
    {
        // private static fields
        private static EnumerableSerializer __instance = new EnumerableSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the EnumerableSerializer class.
        /// </summary>
        public EnumerableSerializer()
            : base(new ArraySerializationOptions())
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the EnumerableSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static EnumerableSerializer Instance
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
            ((IList)instance).Add(item);
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The instance.</returns>
        /// <exception cref="BsonSerializationException">
        /// </exception>
        protected override object CreateInstance(Type actualType)
        {
            string message;

            if (actualType.IsInterface)
            {
                // in the case of an interface pick a reasonable class that implements that interface
                if (actualType == typeof(IEnumerable) || actualType == typeof(ICollection) || actualType == typeof(IList))
                {
                    return new ArrayList();
                }
            }
            else
            {
                if (actualType == typeof(ArrayList))
                {
                    return new ArrayList();
                }
                else if (typeof(IEnumerable).IsAssignableFrom(actualType))
                {
                    var instance = Activator.CreateInstance(actualType);
                    var list = instance as IList;
                    if (list == null)
                    {
                        message = string.Format("Enumerable class {0} does not implement IList so it can't be deserialized.", BsonUtils.GetFriendlyTypeName(actualType));
                        throw new BsonSerializationException(message);
                    }
                    return list;
                }
            }

            message = string.Format("EnumerableSerializer can't be used with type {0}.", BsonUtils.GetFriendlyTypeName(actualType));
            throw new BsonSerializationException(message);
        }

        /// <summary>
        /// Enumerates the items.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The items.</returns>
        protected override IEnumerable EnumerateItemsInSerializationOrder(object instance)
        {
            return (IEnumerable)instance;
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
    /// Represents a serializer for enumerable values.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public class EnumerableSerializer<T> : EnumerableSerializerBase<T>, IBsonArraySerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the EnumerableSerializer class.
        /// </summary>
        public EnumerableSerializer()
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
            ((ICollection<T>)instance).Add(item);
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The instance.</returns>
        /// <exception cref="BsonSerializationException">
        /// </exception>
        protected override object CreateInstance(Type actualType)
        {
            string message;

            if (actualType.IsInterface)
            {
                // in the case of an interface pick a reasonable class that implements that interface
                if (actualType == typeof(IEnumerable<T>) || actualType == typeof(ICollection<T>) || actualType == typeof(IList<T>))
                {
                    return new List<T>();
                }
            }
            else
            {
                if (actualType == typeof(List<T>))
                {
                    return new List<T>();
                }
                else if (typeof(IEnumerable<T>).IsAssignableFrom(actualType))
                {
                    var instance = (IEnumerable<T>)Activator.CreateInstance(actualType);
                    var collection = instance as ICollection<T>;
                    if (collection == null)
                    {
                        message = string.Format("Enumerable class {0} does not implement ICollection<T> so it can't be deserialized.", BsonUtils.GetFriendlyTypeName(actualType));
                        throw new BsonSerializationException(message);
                    }
                    return collection;
                }
            }

            message = string.Format("EnumerableSerializer<{0}> can't be used with type {1}.", BsonUtils.GetFriendlyTypeName(typeof(T)), BsonUtils.GetFriendlyTypeName(actualType));
            throw new BsonSerializationException(message);
        }

        /// <summary>
        /// Enumerates the items.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The items.</returns>
        protected override IEnumerable<T> EnumerateItemsInSerializationOrder(object instance)
        {
            return (IEnumerable<T>)instance;
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

