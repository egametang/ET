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
using System.Collections.ObjectModel;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for readonly collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class ReadOnlyCollectionSerializer<TItem> :
        EnumerableInterfaceImplementerSerializerBase<ReadOnlyCollection<TItem>, TItem>
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollectionSerializer{TItem}"/> class.
        /// </summary>
        public ReadOnlyCollectionSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollectionSerializer{TItem}"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        public ReadOnlyCollectionSerializer(IBsonSerializer<TItem> itemSerializer)
            : base(itemSerializer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollectionSerializer{TItem}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public ReadOnlyCollectionSerializer(IBsonSerializerRegistry serializerRegistry)
            : base(serializerRegistry)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public ReadOnlyCollectionSerializer<TItem> WithItemSerializer(IBsonSerializer<TItem> itemSerializer)
        {
            return new ReadOnlyCollectionSerializer<TItem>(itemSerializer);
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
        protected override ReadOnlyCollection<TItem> FinalizeResult(object accumulator)
        {
            return ((List<TItem>)accumulator).AsReadOnly();
        }
    }
}
