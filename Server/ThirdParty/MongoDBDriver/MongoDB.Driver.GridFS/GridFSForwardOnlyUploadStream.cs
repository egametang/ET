/* Copyright 2015-present MongoDB Inc.
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
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;
using MongoDB.Shared;

namespace MongoDB.Driver.GridFS
{
    internal class GridFSForwardOnlyUploadStream<TFileId> : GridFSUploadStream<TFileId>
    {
        #region static
        // private static fields
        private static readonly Task __completedTask = Task.FromResult(true);
        #endregion

        // fields
        private bool _aborted;
        private readonly List<string> _aliases;
        private List<byte[]> _batch;
        private long _batchPosition;
        private int _batchSize;
        private readonly IWriteBinding _binding;
        private readonly GridFSBucket<TFileId> _bucket;
        private readonly int _chunkSizeBytes;
        private bool _closed;
        private readonly string _contentType;
        private readonly bool _disableMD5;
        private bool _disposed;
        private readonly string _filename;
        private readonly TFileId _id;
        private readonly BsonValue _idAsBsonValue;
        private long _length;
        private readonly IncrementalMD5 _md5;
        private readonly BsonDocument _metadata;

        // constructors
        public GridFSForwardOnlyUploadStream(
            GridFSBucket<TFileId> bucket,
            IWriteBinding binding,
            TFileId id,
            string filename,
            BsonDocument metadata,
            IEnumerable<string> aliases,
            string contentType,
            int chunkSizeBytes,
            int batchSize,
            bool disableMD5)
        {
            _bucket = bucket;
            _binding = binding;
            _id = id;
            _filename = filename;
            _metadata = metadata; // can be null
            _aliases = aliases == null ? null : aliases.ToList(); // can be null
            _contentType = contentType; // can be null
            _chunkSizeBytes = chunkSizeBytes;
            _batchSize = batchSize;

            _batch = new List<byte[]>();
            _md5 = disableMD5 ? null : IncrementalMD5.Create();
            _disableMD5 = disableMD5;

            var idSerializer = bucket.Options.SerializerRegistry.GetSerializer<TFileId>();
            var idSerializationInfo = new BsonSerializationInfo("_id", idSerializer, typeof(TFileId));
            _idAsBsonValue = idSerializationInfo.SerializeValue(id);
        }

        // properties
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override TFileId Id
        {
            get { return _id; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return _length;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        // methods
        public override void Abort(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_aborted)
            {
                return;
            }
            ThrowIfClosedOrDisposed();
            _aborted = true;

            var operation = CreateAbortOperation();
            operation.Execute(_binding, cancellationToken);
        }

        public override async Task AbortAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_aborted)
            {
                return;
            }
            ThrowIfClosedOrDisposed();
            _aborted = true;

            var operation = CreateAbortOperation();
            await operation.ExecuteAsync(_binding, cancellationToken).ConfigureAwait(false);
        }

        public override void Close(CancellationToken cancellationToken)
        {
            try
            {
                CloseIfNotAlreadyClosed(cancellationToken);
            }
            finally
            {
                Dispose();
            }
        }

        public override async Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await CloseIfNotAlreadyClosedAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                Dispose();
            }
        }

        public override void Flush()
        {
            // do nothing
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            // do nothing
            return __completedTask;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfAbortedClosedOrDisposed();
            while (count > 0)
            {
                var chunk = GetCurrentChunk(CancellationToken.None);
                var partialCount = Math.Min(count, chunk.Count);
                Buffer.BlockCopy(buffer, offset, chunk.Array, chunk.Offset, partialCount);
                offset += partialCount;
                count -= partialCount;
                _length += partialCount;
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfAbortedClosedOrDisposed();
            while (count > 0)
            {
                var chunk = await GetCurrentChunkAsync(cancellationToken).ConfigureAwait(false);
                var partialCount = Math.Min(count, chunk.Count);
                Buffer.BlockCopy(buffer, offset, chunk.Array, chunk.Offset, partialCount);
                offset += partialCount;
                count -= partialCount;
                _length += partialCount;
            }
        }

        // private methods
        private void CloseIfNotAlreadyClosed(CancellationToken cancellationToken)
        {
            if (!_closed)
            {
                try
                {
                    CloseImplementation(cancellationToken);
                }
                finally
                {
                    _closed = true;
                }
            }
        }

        private async Task CloseIfNotAlreadyClosedAsync(CancellationToken cancellationToken)
        {
            if (!_closed)
            {
                try
                {
                    await CloseImplementationAsync(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    _closed = true;
                }
            }
        }

        private void CloseIfNotAlreadyClosedFromDispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    CloseIfNotAlreadyClosed(CancellationToken.None);
                }
                catch
                {
                    // ignore any exceptions from CloseIfNotAlreadyClosed when called from Dispose
                }
            }
        }

        private void CloseImplementation(CancellationToken cancellationToken)
        {
            if (!_aborted)
            {
                WriteFinalBatch(cancellationToken);
                WriteFilesCollectionDocument(cancellationToken);
            }
        }

        private async Task CloseImplementationAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_aborted)
            {
                await WriteFinalBatchAsync(cancellationToken).ConfigureAwait(false);
                await WriteFilesCollectionDocumentAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private BulkMixedWriteOperation CreateAbortOperation()
        {
            var chunksCollectionNamespace = _bucket.GetChunksCollectionNamespace();
            var filter = new BsonDocument("files_id", _idAsBsonValue);
            var deleteRequest = new DeleteRequest(filter) { Limit = 0 };
            var requests = new WriteRequest[] { deleteRequest };
            var messageEncoderSettings = _bucket.GetMessageEncoderSettings();
            return new BulkMixedWriteOperation(chunksCollectionNamespace, requests, messageEncoderSettings)
            {
                WriteConcern = _bucket.Options.WriteConcern
            };
        }

        private BsonDocument CreateFilesCollectionDocument()
        {
            var uploadDateTime = DateTime.UtcNow;

            return new BsonDocument
            {
                { "_id", _idAsBsonValue },
                { "length", _length },
                { "chunkSize", _chunkSizeBytes },
                { "uploadDate", uploadDateTime },
                { "md5", () => BsonUtils.ToHexString(_md5.GetHashAndReset()), !_disableMD5 },
                { "filename", _filename },
                { "contentType", _contentType, _contentType != null },
                { "aliases", () => new BsonArray(_aliases.Select(a => new BsonString(a))), _aliases != null },
                { "metadata", _metadata, _metadata != null }
            };
        }

        private IEnumerable<BsonDocument> CreateWriteBatchChunkDocuments()
        {
            var chunkDocuments = new List<BsonDocument>();

            var n = (int)(_batchPosition / _chunkSizeBytes);
            foreach (var chunk in _batch)
            {
                var chunkDocument = new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "files_id", _idAsBsonValue },
                    { "n", n++ },
                    { "data", new BsonBinaryData(chunk, BsonBinarySubType.Binary) }
                };
                chunkDocuments.Add(chunkDocument);

                _batchPosition += chunk.Length;
                _md5?.AppendData(chunk, 0, chunk.Length);
            }

            return chunkDocuments;
        }

        protected override void Dispose(bool disposing)
        {
            CloseIfNotAlreadyClosedFromDispose(disposing);

            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                {
                    if (_md5 != null)
                    {
                        _md5.Dispose();
                    }

                    _binding.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private IMongoCollection<BsonDocument> GetChunksCollection()
        {
            return GetCollection("chunks");
        }

        private IMongoCollection<BsonDocument> GetCollection(string suffix)
        {
            var database = _bucket.Database;
            var collectionName = _bucket.Options.BucketName + "." + suffix;
            var writeConcern = _bucket.Options.WriteConcern ?? database.Settings.WriteConcern;
            var settings = new MongoCollectionSettings { WriteConcern = writeConcern };
            return database.GetCollection<BsonDocument>(collectionName, settings);
        }

        private ArraySegment<byte> GetCurrentChunk(CancellationToken cancellationToken)
        {
            var batchIndex = (int)((_length - _batchPosition) / _chunkSizeBytes);

            if (batchIndex == _batchSize)
            {
                WriteBatch(cancellationToken);
                _batch.Clear();
                batchIndex = 0;
            }

            return GetCurrentChunkSegment(batchIndex);
        }

        private async Task<ArraySegment<byte>> GetCurrentChunkAsync(CancellationToken cancellationToken)
        {
            var batchIndex = (int)((_length - _batchPosition) / _chunkSizeBytes);

            if (batchIndex == _batchSize)
            {
                await WriteBatchAsync(cancellationToken).ConfigureAwait(false);
                _batch.Clear();
                batchIndex = 0;
            }

            return GetCurrentChunkSegment(batchIndex);
        }

        private ArraySegment<byte> GetCurrentChunkSegment(int batchIndex)
        {
            if (_batch.Count <= batchIndex)
            {
                _batch.Add(new byte[_chunkSizeBytes]);
            }

            var chunk = _batch[batchIndex];
            var offset = (int)(_length % _chunkSizeBytes);
            var count = _chunkSizeBytes - offset;
            return new ArraySegment<byte>(chunk, offset, count);
        }

        private IMongoCollection<BsonDocument> GetFilesCollection()
        {
            return GetCollection("files");
        }

        private void ThrowIfAbortedClosedOrDisposed()
        {
            if (_aborted)
            {
                throw new InvalidOperationException("The upload was aborted.");
            }
            ThrowIfClosedOrDisposed();
        }

        private void ThrowIfClosedOrDisposed()
        {
            if (_closed)
            {
                throw new InvalidOperationException("The stream is closed.");
            }
            ThrowIfDisposed();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void TruncateFinalChunk()
        {
            var finalChunkSize = (int)(_length % _chunkSizeBytes);
            if (finalChunkSize > 0)
            {
                var finalChunk = _batch[_batch.Count - 1];
                if (finalChunk.Length != finalChunkSize)
                {
                    var truncatedFinalChunk = new byte[finalChunkSize];
                    Buffer.BlockCopy(finalChunk, 0, truncatedFinalChunk, 0, finalChunkSize);
                    _batch[_batch.Count - 1] = truncatedFinalChunk;
                }
            }
        }

        private void WriteBatch(CancellationToken cancellationToken)
        {
            var chunksCollection = GetChunksCollection();
            var chunkDocuments = CreateWriteBatchChunkDocuments();
            chunksCollection.InsertMany(chunkDocuments, cancellationToken: cancellationToken);
            _batch.Clear();
        }

        private async Task WriteBatchAsync(CancellationToken cancellationToken)
        {
            var chunksCollection = GetChunksCollection();
            var chunkDocuments = CreateWriteBatchChunkDocuments();
            await chunksCollection.InsertManyAsync(chunkDocuments, cancellationToken: cancellationToken).ConfigureAwait(false);
            _batch.Clear();
        }

        private void WriteFilesCollectionDocument(CancellationToken cancellationToken)
        {
            var filesCollection = GetFilesCollection();
            var filesCollectionDocument = CreateFilesCollectionDocument();
            filesCollection.InsertOne(filesCollectionDocument, cancellationToken: cancellationToken);
        }

        private async Task WriteFilesCollectionDocumentAsync(CancellationToken cancellationToken)
        {
            var filesCollection = GetFilesCollection();
            var filesCollectionDocument = CreateFilesCollectionDocument();
            await filesCollection.InsertOneAsync(filesCollectionDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private void WriteFinalBatch(CancellationToken cancellationToken)
        {
            if (_batch.Count > 0)
            {
                TruncateFinalChunk();
                WriteBatch(cancellationToken);
            }
        }

        private async Task WriteFinalBatchAsync(CancellationToken cancellationToken)
        {
            if (_batch.Count > 0)
            {
                TruncateFinalChunk();
                await WriteBatchAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
