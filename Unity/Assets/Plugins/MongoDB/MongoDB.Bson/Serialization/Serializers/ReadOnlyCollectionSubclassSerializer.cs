/* Copyright 2010-2015 MongoDB Inc.
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

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for a subclass of ReadOnlyCollection.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class ReadOnlyCollectionSubclassSerializer<TValue, TItem> : EnumerableInterfaceImplementerSerializerBase<TValue, TItem> where TValue : ReadOnlyCollection<TItem>
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollectionSubclassSerializer{TValue, TItem}"/> class.
        /// </summary>
        public ReadOnlyCollectionSubclassSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollectionSubclassSerializer{TValue, TItem}"/> class.
        /// </summary>
        /// <param name="itemSerializer"></param>
        public ReadOnlyCollectionSubclassSerializer(IBsonSerializer<TItem> itemSerializer)
            : base(itemSerializer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollectionSubclassSerializer{TValue, TItem}"/> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public ReadOnlyCollectionSubclassSerializer(IBsonSerializerRegistry serializerRegistry)
            : base(serializerRegistry)
        {
        }

        // protected methods
        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected override object CreateAccumulator()
        {
            return new List<TItem>();
        }

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <returns>The final result.</returns>
        protected override TValue FinalizeResult(object accumulator)
        {
            // the subclass must have a constructor that takes an IList<T> to wrap
            return (TValue)Activator.CreateInstance(typeof(TValue), accumulator);
        }
    }
}
