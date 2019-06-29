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

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// A change stream cursor.
    /// </summary>
    /// <typeparam name="TDocument">The type of the output documents.</typeparam>
    /// <seealso cref="MongoDB.Driver.IAsyncCursor{TOutput}" />
    internal sealed class ChangeStreamCursor<TDocument> : IAsyncCursor<TDocument>
    {
        // private fields
        private readonly IReadBinding _binding;
        private readonly IChangeStreamOperation<TDocument> _changeStreamOperation;
        private IEnumerable<TDocument> _current;
        private IAsyncCursor<RawBsonDocument> _cursor;
        private bool _disposed;
        private IBsonSerializer<TDocument> _documentSerializer;

        // public properties
        /// <inheritdoc />
        public IEnumerable<TDocument> Current => _current;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStreamCursor{TDocument}" /> class.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="changeStreamOperation">The change stream operation.</param>
        public ChangeStreamCursor(
            IAsyncCursor<RawBsonDocument> cursor,
            IBsonSerializer<TDocument> documentSerializer,
            IReadBinding binding,
            IChangeStreamOperation<TDocument> changeStreamOperation)
        {
            _cursor = Ensure.IsNotNull(cursor, nameof(cursor));
            _documentSerializer = Ensure.IsNotNull(documentSerializer, nameof(documentSerializer));
            _binding = Ensure.IsNotNull(binding, nameof(binding));
            _changeStreamOperation = Ensure.IsNotNull(changeStreamOperation, nameof(changeStreamOperation));
        }

        // public methods
        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cursor.Dispose();
                _binding.Dispose();
            }
        }

        /// <inheritdoc/>
        public bool MoveNext(CancellationToken cancellationToken = default(CancellationToken))
        {
            bool hasMore;
            while (true)
            {
                try
                {
                    hasMore = _cursor.MoveNext(cancellationToken);
                    break;
                }
                catch (Exception ex) when (RetryabilityHelper.IsResumableChangeStreamException(ex))
                {
                    var newCursor = _changeStreamOperation.Resume(_binding, cancellationToken);
                    _cursor.Dispose();
                    _cursor = newCursor;
                }
            }

            ProcessBatch(hasMore);
            return hasMore;
        }

        /// <inheritdoc/>
        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
        {          
            bool hasMore;
            while (true)
            {
                try
                {
                    hasMore = await _cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false);
                    break;
                }
                catch (Exception ex) when (RetryabilityHelper.IsResumableChangeStreamException(ex))
                {
                    var newCursor = await _changeStreamOperation.ResumeAsync(_binding, cancellationToken).ConfigureAwait(false);
                    _cursor.Dispose();
                    _cursor = newCursor;
                }
            }

            ProcessBatch(hasMore);
            return hasMore;
        }

        // private methods
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private TDocument DeserializeDocument(RawBsonDocument rawDocument)
        {
            using (var stream = new ByteBufferStream(rawDocument.Slice, ownsBuffer: false))
            using (var reader = new BsonBinaryReader(stream))
            {
                var context = BsonDeserializationContext.CreateRoot(reader);
                return _documentSerializer.Deserialize(context);
            }
        }

        private IEnumerable<TDocument> DeserializeDocuments(IEnumerable<RawBsonDocument> rawDocuments)
        {
            var documents = new List<TDocument>();
            RawBsonDocument lastRawDocument = null;

            foreach (var rawDocument in rawDocuments)
            {
                if (!rawDocument.Contains("_id"))
                {
                    throw new MongoClientException("Cannot provide resume functionality when the resume token is missing.");
                }

                var document = DeserializeDocument(rawDocument);
                documents.Add(document);

                lastRawDocument = rawDocument;
            }

            if (lastRawDocument != null)
            {
                _changeStreamOperation.ResumeAfter = lastRawDocument["_id"].DeepClone().AsBsonDocument;
                _changeStreamOperation.StartAtOperationTime = null;
            }

            return documents;
        }

        private void ProcessBatch(bool hasMore)
        {
            if (hasMore)
            {
                try
                {
                    _current = DeserializeDocuments(_cursor.Current);
                }
                finally
                {
                    foreach (var rawDocument in _cursor.Current)
                    {
                        rawDocument.Dispose();
                    }
                }
            }
            else
            {
                _current = null;
            }
        }
    }
}
