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

using System;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// A serializer for BatchableSource that serializes as much of the BatchableSource as fits in the max batch count and size.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public class SizeLimitingBatchableSourceSerializer<TItem> : SerializerBase<BatchableSource<TItem>>
    {
        // private fields
        private readonly IElementNameValidator _itemElementNameValidator;
        private readonly IBsonSerializer<TItem> _itemSerializer;
        private readonly int _maxBatchCount;
        private readonly int _maxBatchSize;
        private readonly int _maxItemSize;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SizeLimitingBatchableSourceSerializer{TItem}" /> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <param name="itemElementNameValidator">The item element name validator.</param>
        /// <param name="maxBatchCount">The maximum batch count.</param>
        /// <param name="maxItemSize">The maximum size of a serialized item.</param>
        /// <param name="maxBatchSize">The maximum size of the batch.</param>
        public SizeLimitingBatchableSourceSerializer(
            IBsonSerializer<TItem> itemSerializer, 
            IElementNameValidator itemElementNameValidator, 
            int maxBatchCount,
            int maxItemSize, 
            int maxBatchSize)
        {
            _itemSerializer = Ensure.IsNotNull(itemSerializer, nameof(itemSerializer));
            _itemElementNameValidator = Ensure.IsNotNull(itemElementNameValidator, nameof(itemElementNameValidator));
            _maxBatchCount = Ensure.IsGreaterThanZero(maxBatchCount, nameof(maxBatchCount));
            _maxItemSize = Ensure.IsGreaterThanZero(maxItemSize, nameof(maxItemSize));
            _maxBatchSize = Ensure.IsGreaterThanZero(maxBatchSize, nameof(maxBatchSize));
        }

        // public methods
        /// <inheritdoc />
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BatchableSource<TItem> value)
        {
            Ensure.IsNotNull(value, nameof(value));

            var writer = context.Writer;
            while (writer is WrappingBsonWriter)
            {
                writer = ((WrappingBsonWriter)writer).Wrapped;
            }

            var binaryWriter = writer as BsonBinaryWriter;
            var startPosition = binaryWriter?.Position;

            writer.PushSettings(s => { var bs = s as BsonBinaryWriterSettings; if (bs != null) { bs.MaxDocumentSize = _maxItemSize; } });
            writer.PushElementNameValidator(_itemElementNameValidator);
            try
            {
                var batchCount = Math.Min(value.Count, _maxBatchCount);
                if (batchCount != value.Count && !value.CanBeSplit)
                {
                    throw new ArgumentException("Batch is too large.");
                }

                for (var i = 0; i < batchCount; i++)
                {
                    var itemPosition = binaryWriter?.Position;

                    var item = value.Items[value.Offset + i];
                    _itemSerializer.Serialize(context, args, item);

                    // always process at least one item
                    if (i > 0)
                    {
                        var batchSize = binaryWriter?.Position - startPosition;
                        if (batchSize > _maxBatchSize)
                        {
                            if (value.CanBeSplit)
                            {
                                binaryWriter.BaseStream.Position = itemPosition.Value; // remove the last item
                                binaryWriter.BaseStream.SetLength(itemPosition.Value);
                                value.SetProcessedCount(i);
                                return;
                            }
                            else
                            {
                                throw new ArgumentException("Batch is too large.");
                            }
                        }
                    }
                }
                value.SetProcessedCount(batchCount);
            }
            finally
            {
                writer.PopElementNameValidator();
                writer.PopSettings();
            }
        }
    }
}
