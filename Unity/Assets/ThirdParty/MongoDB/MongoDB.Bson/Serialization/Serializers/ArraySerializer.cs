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

using System.Collections.Generic;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for one-dimensional arrays.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements.</typeparam>
    public class ArraySerializer<TItem> :
        EnumerableSerializerBase<TItem[], TItem>,
        IBsonArraySerializer,
        IChildSerializerConfigurable
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ArraySerializer{TItem}"/> class.
        /// </summary>
        public ArraySerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArraySerializer{TItem}"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        public ArraySerializer(IBsonSerializer<TItem> itemSerializer)
            : base(itemSerializer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArraySerializer{TItem}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public ArraySerializer(IBsonSerializerRegistry serializerRegistry)
            : base(serializerRegistry)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public ArraySerializer<TItem> WithItemSerializer(IBsonSerializer<TItem> itemSerializer)
        {
            return new ArraySerializer<TItem>(itemSerializer);
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <param name="item">The item.</param>
        protected override void AddItem(object accumulator, TItem item)
        {
            ((List<TItem>)accumulator).Add(item);
        }

        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected override object CreateAccumulator()
        {
            return new List<TItem>();
        }

        /// <summary>
        /// Enumerates the items in serialization order.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The items.</returns>
        protected override IEnumerable<TItem> EnumerateItemsInSerializationOrder(TItem[] value)
        {
            return (IEnumerable<TItem>)value;
        }

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <returns>The result.</returns>
        protected override TItem[] FinalizeResult(object accumulator)
        {
            return ((List<TItem>)accumulator).ToArray();
        }

        // explicit interface implementations
        IBsonSerializer IChildSerializerConfigurable.ChildSerializer
        {
            get { return ItemSerializer; }
        }

        IBsonSerializer IChildSerializerConfigurable.WithChildSerializer(IBsonSerializer childSerializer)
        {
            return WithItemSerializer((IBsonSerializer<TItem>)childSerializer);
        }
    }
}
