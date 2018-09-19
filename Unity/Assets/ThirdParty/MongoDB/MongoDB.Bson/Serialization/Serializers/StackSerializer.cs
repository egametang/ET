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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Stacks.
    /// </summary>
    public class StackSerializer :
        EnumerableSerializerBase<Stack>,
        IChildSerializerConfigurable,
        IBsonArraySerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StackSerializer"/> class.
        /// </summary>
        public StackSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackSerializer"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        public StackSerializer(IBsonSerializer itemSerializer)
            : base(itemSerializer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackSerializer" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public StackSerializer(IBsonSerializerRegistry serializerRegistry)
            : base(serializerRegistry)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public StackSerializer WithItemSerializer(IBsonSerializer itemSerializer)
        {
            return new StackSerializer(itemSerializer);
        }

       // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <param name="item">The item.</param>
        protected override void AddItem(object accumulator, object item)
        {
            ((Stack)accumulator).Push(item);
        }

        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected override object CreateAccumulator()
        {
            return new Stack();
        }

        /// <summary>
        /// Enumerates the items in serialization order.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The items.</returns>
        protected override IEnumerable EnumerateItemsInSerializationOrder(Stack value)
        {
            return value.Cast<object>().Reverse();
        }

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <returns>The result.</returns>
        protected override Stack FinalizeResult(object accumulator)
        {
            return (Stack)accumulator;
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
    /// Represents a serializer for Stacks.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements.</typeparam>
    public class StackSerializer<TItem> :
        EnumerableSerializerBase<Stack<TItem>, TItem>,
        IChildSerializerConfigurable,
        IBsonArraySerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StackSerializer{TItem}"/> class.
        /// </summary>
        public StackSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackSerializer{TItem}"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        public StackSerializer(IBsonSerializer<TItem> itemSerializer)
            : base(itemSerializer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackSerializer{TItem}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public StackSerializer(IBsonSerializerRegistry serializerRegistry)
            : base(serializerRegistry)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public StackSerializer<TItem> WithItemSerializer(IBsonSerializer<TItem> itemSerializer)
        {
            return new StackSerializer<TItem>(itemSerializer);
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <param name="item">The item.</param>
        protected override void AddItem(object accumulator, TItem item)
        {
            ((Stack<TItem>)accumulator).Push(item);
        }

        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected override object CreateAccumulator()
        {
            return new Stack<TItem>();
        }

        /// <summary>
        /// Enumerates the items in serialization order.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The items.</returns>
        protected override IEnumerable<TItem> EnumerateItemsInSerializationOrder(Stack<TItem> value)
        {
            return value.Reverse();
        }

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <returns>The result.</returns>
        protected override Stack<TItem> FinalizeResult(object accumulator)
        {
            return (Stack<TItem>)accumulator;
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
