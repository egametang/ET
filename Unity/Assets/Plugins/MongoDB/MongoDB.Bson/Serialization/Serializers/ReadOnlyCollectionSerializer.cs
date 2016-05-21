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
using System.Collections.ObjectModel;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for ReadOnlyCollections.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public class ReadOnlyCollectionSerializer<T> : EnumerableSerializerBase<T>, IBsonArraySerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the ReadOnlyCollectionSerializer class.
        /// </summary>
        public ReadOnlyCollectionSerializer()
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
            ((List<T>)instance).Add(item);
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The instance.</returns>
        protected override object CreateInstance(Type actualType)
        {
            return new List<T>();
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
        /// <exception cref="BsonSerializationException"></exception>
        protected override object FinalizeResult(object instance, Type actualType)
        {
            var list = (List<T>)instance;
            if (actualType == typeof(ReadOnlyCollection<T>))
            {
                return new ReadOnlyCollection<T>(list);
            }
            else if (typeof(ReadOnlyCollection<T>).IsAssignableFrom(actualType))
            {
                return (ReadOnlyCollection<T>)Activator.CreateInstance(actualType, list);
            }

            var message = string.Format("ReadOnlyCollectionSerializer<{0}> can't be used with type {1}.", BsonUtils.GetFriendlyTypeName(typeof(T)), BsonUtils.GetFriendlyTypeName(actualType));
            throw new BsonSerializationException(message);
        }

        /// <summary>
        /// Gets the discriminator.
        /// </summary>
        /// <param name="nominalType">Type nominal type.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The discriminator (or null if no discriminator is needed).</returns>
        protected override string GetDiscriminator(Type nominalType, Type actualType)
        {
            if (nominalType != actualType)
            {
                if (nominalType == typeof(object))
                {
                    return TypeNameDiscriminator.GetDiscriminator(actualType);
                }
                else
                {
                    return actualType.Name;
                }
            }

            return null;
        }
    }
}
