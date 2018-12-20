/* Copyright 2017-present MongoDB Inc.
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

using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// A serializer for BatchableSource that serializes a fixed count of items.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public class FixedCountBatchableSourceSerializer<TItem> : SerializerBase<BatchableSource<TItem>>
    {
        // private fields
        private readonly int _count;
        private readonly IElementNameValidator _itemElementNameValidator;
        private readonly IBsonSerializer<TItem> _itemSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FixedCountBatchableSourceSerializer{TITem}" /> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <param name="itemElementNameValidator">The item element name validator.</param>
        /// <param name="count">The count.</param>
        public FixedCountBatchableSourceSerializer(IBsonSerializer<TItem> itemSerializer, IElementNameValidator itemElementNameValidator, int count)
        {
            _itemSerializer = Ensure.IsNotNull(itemSerializer, nameof(itemSerializer));
            _itemElementNameValidator = Ensure.IsNotNull(itemElementNameValidator, nameof(itemElementNameValidator));
            _count = Ensure.IsBetween(count, 0, int.MaxValue, nameof(count));
        }

        // public methods
        /// <inheritdoc />
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BatchableSource<TItem> value)
        {
            Ensure.IsNotNull(value, nameof(value));
            var writer = context.Writer;

            writer.PushElementNameValidator(_itemElementNameValidator);
            try
            {
                for (var i = 0; i < _count; i++)
                {
                    var item = value.Items[value.Offset + i];
                    _itemSerializer.Serialize(context, item);
                }
            }
            finally
            {
                writer.PopElementNameValidator();
            }
        }
    }
}
