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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver.GridFS
{
    internal class GridFSSeekableDownloadStream<TFileId> : GridFSDownloadStreamBase<TFileId>
    {
        // private fields
        private byte[] _chunk;
        private readonly BsonValue _idAsBsonValue;
        private long _n = -1;
        private long _position;

        // constructors
        public GridFSSeekableDownloadStream(
            GridFSBucket<TFileId> bucket,
            IReadBinding binding,
            GridFSFileInfo<TFileId> fileInfo)
            : base(bucket, binding, fileInfo)
        {
            var idSerializer = bucket.Options.SerializerRegistry.GetSerializer<TFileId>();
            var idSerializationInfo = new BsonSerializationInfo("_id", idSerializer, typeof(TFileId));
            _idAsBsonValue = idSerializationInfo.SerializeValue(fileInfo.Id);
        }

        // public properties
        public override bool CanSeek
        {
            get { return true; }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                Ensure.IsGreaterThanOrEqualToZero(value, nameof(value));
                _position = value;
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
            long newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin: newPosition = offset; break;
                case SeekOrigin.Current: newPosition = _position + offset; break;
                case SeekOrigin.End: newPosition = Length + offset; break;
                default: throw new ArgumentException("Invalid origin.", "origin");
            }
            if (newPosition < 0)
            {
                throw new IOException("Position must be greater than or equal to zero.");
            }
            if (newPosition > Length)
            {
                throw new IOException("Position must be less than or equal to the length of the stream.");
            }
            Position = newPosition;
            return newPosition;
        }

        // private methods
        private FindOperation<BsonDocument> CreateGetChunkOperation(long n)
        {
            var chunksCollectionNamespace = Bucket.GetChunksCollectionNamespace();
            var messageEncoderSettings = Bucket.GetMessageEncoderSettings();
#pragma warning disable 618
            var filter = new BsonDocument
            {
                { "files_id", _idAsBsonValue },
                { "n", n }
            };
#pragma warning restore

            return new FindOperation<BsonDocument>(
                chunksCollectionNamespace,
                BsonDocumentSerializer.Instance,
                messageEncoderSettings)
            {
                Filter = filter,
                Limit = -1
            };

        }

        private void GetChunk(long n, CancellationToken cancellationToken)
        {
            var operation = CreateGetChunkOperation(n);
            using (var cursor = operation.Execute(Binding, cancellationToken))
            {
                var documents = cursor.ToList();
                _chunk = GetChunkHelper(n, documents);
                _n = n;
            }
        }

        private async Task GetChunkAsync(long n, CancellationToken cancellationToken)
        {
            var operation = CreateGetChunkOperation(n);
            using (var cursor = await operation.ExecuteAsync(Binding, cancellationToken).ConfigureAwait(false))
            {
                var documents = await cursor.ToListAsync().ConfigureAwait(false);
                _chunk = GetChunkHelper(n, documents);
                _n = n;
            }
        }

        private byte[] GetChunkHelper(long n, List<BsonDocument> documents)
        {
            if (documents.Count == 0)
            {
#pragma warning disable 618
                throw new GridFSChunkException(_idAsBsonValue, n, "missing");
#pragma warning restore
            }

            var document = documents[0];
            var data = document["data"].AsBsonBinaryData.Bytes;

            var chunkSizeBytes = FileInfo.ChunkSizeBytes;
            var lastChunk = FileInfo.Length / FileInfo.ChunkSizeBytes;
            var expectedChunkSize = n == lastChunk ? FileInfo.Length % chunkSizeBytes : chunkSizeBytes;
            if (data.Length != expectedChunkSize)
            {
#pragma warning disable 618
                throw new GridFSChunkException(_idAsBsonValue, n, "the wrong size");
#pragma warning restore
            }

            return data;
        }

        private ArraySegment<byte> GetSegment(CancellationToken cancellationToken)
        {
            var n = _position / FileInfo.ChunkSizeBytes;
            if (_n != n)
            {
                GetChunk(n, cancellationToken);
            }

            var segmentOffset = (int)(_position % FileInfo.ChunkSizeBytes);
            var segmentCount = _chunk.Length - segmentOffset;

            return new ArraySegment<byte>(_chunk, segmentOffset, segmentCount);
        }

        private async Task<ArraySegment<byte>> GetSegmentAsync(CancellationToken cancellationToken)
        {
            var n = _position / FileInfo.ChunkSizeBytes;
            if (_n != n)
            {
                await GetChunkAsync(n, cancellationToken).ConfigureAwait(false);
            }

            var segmentOffset = (int)(_position % FileInfo.ChunkSizeBytes);
            var segmentCount = _chunk.Length - segmentOffset;

            return new ArraySegment<byte>(_chunk, segmentOffset, segmentCount);
        }
    }
}
