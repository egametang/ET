/* Copyright 2016-present MongoDB Inc.
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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;
using MongoDB.Shared;

namespace MongoDB.Driver.GridFS
{
    internal class GridFSForwardOnlyDownloadStream<TFileId> : GridFSDownloadStreamBase<TFileId>
    {
        // private fields
        private List<BsonDocument> _batch;
        private long _batchPosition;
        private readonly bool _checkMD5;
        private IAsyncCursor<BsonDocument> _cursor;
        private bool _disposed;
        private readonly BsonValue _idAsBsonValue;
        private readonly int _lastChunkNumber;
        private readonly int _lastChunkSize;
        private readonly IncrementalMD5 _md5;
        private int _nextChunkNumber;
        private long _position;

        // constructors
        public GridFSForwardOnlyDownloadStream(
            GridFSBucket<TFileId> bucket,
            IReadBinding binding,
            GridFSFileInfo<TFileId> fileInfo,
            bool checkMD5)
            : base(bucket, binding, fileInfo)
        {
            _checkMD5 = checkMD5;
            if (_checkMD5)
            {
                _md5 = IncrementalMD5.Create();
            }

            _lastChunkNumber = (int)((fileInfo.Length - 1) / fileInfo.ChunkSizeBytes);
            _lastChunkSize = (int)(fileInfo.Length % fileInfo.ChunkSizeBytes);

            if (_lastChunkSize == 0)
            {
                _lastChunkSize = fileInfo.ChunkSizeBytes;
            }

            var idSerializer = bucket.Options.SerializerRegistry.GetSerializer<TFileId>();
            var idSerializationInfo = new BsonSerializationInfo("_id", idSerializer, typeof(TFileId));
            _idAsBsonValue = idSerializationInfo.SerializeValue(fileInfo.Id);
        }

        // public properties
        public override bool CanSeek
        {
            get { return false; }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        // methods
        public override int Read(byte[] buffer, int offset, int count)
        {
            Ensure.IsNotNull(buffer, nameof(buffer));
            Ensure.IsBetween(offset, 0, buffer.Length, nameof(offset));
            Ensure.IsBetween(count, 0, buffer.Length - offset, nameof(count));
            ThrowIfDisposed();

            var bytesRead = 0;
            while (count > 0 && _position < FileInfo.Length)
            {
                var segment = GetSegment(CancellationToken.None);

                var partialCount = Math.Min(count, segment.Count);
                Buffer.BlockCopy(segment.Array, segment.Offset, buffer, offset, partialCount);

                bytesRead += partialCount;
                offset += partialCount;
                count -= partialCount;
                _position += partialCount;
            }

            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(buffer, nameof(buffer));
            Ensure.IsBetween(offset, 0, buffer.Length, nameof(offset));
            Ensure.IsBetween(count, 0, buffer.Length - offset, nameof(count));
            ThrowIfDisposed();

            var bytesRead = 0;
            while (count > 0 && _position < FileInfo.Length)
            {
                var segment = await GetSegmentAsync(cancellationToken).ConfigureAwait(false);

                var partialCount = Math.Min(count, segment.Count);
                Buffer.BlockCopy(segment.Array, segment.Offset, buffer, offset, partialCount);

                bytesRead += partialCount;
                offset += partialCount;
                count -= partialCount;
                _position += partialCount;
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        // protected methods
        protected override void CloseImplementation(CancellationToken cancellationToken)
        {
            if (_checkMD5 && _position == FileInfo.Length)
            {
                var md5 = BsonUtils.ToHexString(_md5.GetHashAndReset());
                if (!md5.Equals(FileInfo.MD5, StringComparison.OrdinalIgnoreCase))
                {
#pragma warning disable 618
                    throw new GridFSMD5Exception(_idAsBsonValue);
#pragma warning restore
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            CloseIfNotAlreadyClosedFromDispose(disposing);

            if (!_disposed)
            {
                if (disposing)
                {
                    if (_cursor != null)
                    {
                        _cursor.Dispose();
                    }
                    if (_md5 != null)
                    {
                        _md5.Dispose();
                    }
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        protected override void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            base.ThrowIfDisposed();
        }

        // private methods
        private FindOperation<BsonDocument> CreateFirstBatchOperation()
        {
            var chunksCollectionNamespace = Bucket.GetChunksCollectionNamespace();
            var messageEncoderSettings = Bucket.GetMessageEncoderSettings();
#pragma warning disable 618
            var filter = new BsonDocument("files_id", _idAsBsonValue);
#pragma warning restore
            var sort = new BsonDocument("n", 1);

            return new FindOperation<BsonDocument>(
                chunksCollectionNamespace,
                BsonDocumentSerializer.Instance,
                messageEncoderSettings)
            {
                Filter = filter,
                Sort = sort
            };
        }

        private void GetFirstBatch(CancellationToken cancellationToken)
        {
            var operation = CreateFirstBatchOperation();
            _cursor = operation.Execute(Binding, cancellationToken);
            GetNextBatch(cancellationToken);
        }

        private async Task GetFirstBatchAsync(CancellationToken cancellationToken)
        {
            var operation = CreateFirstBatchOperation();
            _cursor = await operation.ExecuteAsync(Binding, cancellationToken).ConfigureAwait(false);
            await GetNextBatchAsync(cancellationToken).ConfigureAwait(false);
        }

        private void GetNextBatch(CancellationToken cancellationToken)
        {
            List<BsonDocument> batch;
            do
            {
                var hasMore = _cursor.MoveNext(cancellationToken);
                batch = hasMore ? _cursor.Current.ToList() : null;
            }
            while (batch != null && batch.Count == 0);

            ProcessNextBatch(batch);
        }

        private async Task GetNextBatchAsync(CancellationToken cancellationToken)
        {
            List<BsonDocument> batch;
            do
            {
                var hasMore = await _cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false);
                batch = hasMore ? _cursor.Current.ToList() : null;
            }
            while (batch != null && batch.Count == 0);

            ProcessNextBatch(batch);
        }

        private void ProcessNextBatch(List<BsonDocument> batch)
        {
            if (batch == null)
            {
#pragma warning disable 618
                throw new GridFSChunkException(_idAsBsonValue, _nextChunkNumber, "missing");
#pragma warning restore
            }

            var previousBatch = _batch;
            _batch = batch;

            if (previousBatch != null)
            {
                _batchPosition += previousBatch.Count * FileInfo.ChunkSizeBytes; ;
            }

            var lastChunkInBatch = _batch.Last();
            if (lastChunkInBatch["n"].ToInt32() == _lastChunkNumber + 1 && lastChunkInBatch["data"].AsBsonBinaryData.Bytes.Length == 0)
            {
                _batch.RemoveAt(_batch.Count - 1);
            }

            foreach (var chunk in _batch)
            {
                var n = chunk["n"].ToInt32();
                var bytes = chunk["data"].AsBsonBinaryData.Bytes;

                if (n != _nextChunkNumber)
                {
#pragma warning disable 618
                    throw new GridFSChunkException(_idAsBsonValue, _nextChunkNumber, "missing");
#pragma warning restore
                }
                _nextChunkNumber++;

                var expectedChunkSize = n == _lastChunkNumber ? _lastChunkSize : FileInfo.ChunkSizeBytes;
                if (bytes.Length != expectedChunkSize)
                {
#pragma warning disable 618
                    throw new GridFSChunkException(_idAsBsonValue, _nextChunkNumber, "the wrong size");
#pragma warning restore
                }

                if (_checkMD5)
                {
                    _md5.AppendData(bytes, 0, bytes.Length);
                }
            }
        }

        private ArraySegment<byte> GetSegment(CancellationToken cancellationToken)
        {
            var batchIndex = (int)((_position - _batchPosition) / FileInfo.ChunkSizeBytes);

            if (_cursor == null)
            {
                GetFirstBatch(cancellationToken);
            }
            else if (batchIndex == _batch.Count)
            {
                GetNextBatch(cancellationToken);
                batchIndex = 0;
            }

            return GetSegmentHelper(batchIndex);
        }

        private async Task<ArraySegment<byte>> GetSegmentAsync(CancellationToken cancellationToken)
        {
            var batchIndex = (int)((_position - _batchPosition) / FileInfo.ChunkSizeBytes);

            if (_cursor == null)
            {
                await GetFirstBatchAsync(cancellationToken).ConfigureAwait(false);
            }
            else if (batchIndex == _batch.Count)
            {
                await GetNextBatchAsync(cancellationToken).ConfigureAwait(false);
                batchIndex = 0;
            }

            return GetSegmentHelper(batchIndex);
        }

        private ArraySegment<byte> GetSegmentHelper(int batchIndex)
        {
            var bytes = _batch[batchIndex]["data"].AsBsonBinaryData.Bytes;
            var segmentOffset = (int)(_position % FileInfo.ChunkSizeBytes);
            var segmentCount = bytes.Length - segmentOffset;
            return new ArraySegment<byte>(bytes, segmentOffset, segmentCount);
        }
    }
}
