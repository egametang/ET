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

using System.Collections;
using System.Collections.Generic;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Queues.
    /// </summary>
    public class QueueSerializer :
        EnumerableSerializerBase<Queue>,
        IChildSerializerConfigurable,
        IBsonArraySerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSerializer"/> class.
        /// </summary>
        public QueueSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSerializer"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        public QueueSerializer(IBsonSerializer itemSerializer)
            : base(itemSerializer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSerializer" /> class.
        /// </summary>
        /// <param name="serializerRegistry"></param>
        public QueueSerializer(IBsonSerializerRegistry serializerRegistry)
            : base(serializerRegistry)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public QueueSerializer WithItemSerializer(IBsonSerializer itemSerializer)
        {
            return new QueueSerializer(itemSerializer);
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <param name="item">The item.</param>
        protected override void AddItem(object accumulator, object item)
        {
            ((Queue)accumulator).Enqueue(item);
        }

        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected override object CreateAccumulator()
        {
            return new Queue();
        }

        /// <summary>
        /// Enumerates the items.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The items.</returns>
        protected override IEnumerable EnumerateItemsInSerializationOrder(Queue value)
        {
            return value;
        }

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="accumulator">The instance.</param>
        /// <returns>The result.</returns>
        protected override Queue FinalizeResult(object accumulator)
        {
            return (Queue)accumulator;
        }

        // explicit interface implementations
        IBsonSerializer IChildSerializerConfigurable.ChildSerializer
        {
            get { return ItemSerializer; }
        }

        IBsonSerializer IChildSerializerConfigurable.WithChildSerializer(IBsonSerializer childSerializer)
        {
            return WithItemSerializer(childSerializer);
        }
    }

    /// <summary>
    /// Represents a serializer for Queues.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements.</typeparam>
    public class QueueSerializer<TItem> :
        EnumerableSerializerBase<Queue<TItem>, TItem>,
        IChildSerializerConfigurable,
        IBsonArraySerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSerializer{TItem}"/> class.
        /// </summary>
        public QueueSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSerializer{TItem}"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        public QueueSerializer(IBsonSerializer<TItem> itemSerializer)
            : base(itemSerializer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSerializer{TItem}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public QueueSerializer(IBsonSerializerRegistry serializerRegistry)
            : base(serializerRegistry)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public QueueSerializer<TItem> WithItemSerializer(IBsonSerializer<TItem> itemSerializer)
        {
            return new QueueSerializer<TItem>(itemSerializer);
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <param name="item">The item.</param>
        protected override void AddItem(object accumulator, TItem item)
        {
            ((Queue<TItem>)accumulator).Enqueue(item);
        }

        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected override object CreateAccumulator()
        {
            return new Queue<TItem>();
        }

        /// <summary>
        /// Enumerates the items in serialization order.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The items.</returns>
        protected override IEnumerable<TItem> EnumerateItemsInSerializationOrder(Queue<TItem> value)
        {
            return value;
        }

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <returns>The result.</returns>
        protected override Queue<TItem> FinalizeResult(object accumulator)
        {
            return (Queue<TItem>)accumulator;
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
