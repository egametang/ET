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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.GridFS
{
    /// <summary>
    /// Represents a GridFS bucket.
    /// </summary>
    public class GridFSBucket : GridFSBucket<ObjectId>, IGridFSBucket
    {
        // private fields
        private readonly GridFSBucket<BsonValue> _bsonValueBucket;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GridFSBucket" /> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="options">The options.</param>
        public GridFSBucket(IMongoDatabase database, GridFSBucketOptions options = null)
            : base(database, options)
        {
            _bsonValueBucket = new GridFSBucket<BsonValue>(database, options);
        }

        // methods
        /// <summary>
        /// Deletes a file from GridFS.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void Delete(BsonValue id, CancellationToken cancellationToken = default(CancellationToken))
        {
            _bsonValueBucket.Delete(id, cancellationToken);
        }

        /// <summary>
        /// Deletes a file from GridFS.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public Task DeleteAsync(BsonValue id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _bsonValueBucket.DeleteAsync(id, cancellationToken);
        }

        /// <summary>
        /// Downloads a file stored in GridFS and returns it as a byte array.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A byte array containing the contents of the file stored in GridFS.</returns>
        public byte[] DownloadAsBytes(BsonValue id, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _bsonValueBucket.DownloadAsBytes(id, options, cancellationToken);
        }

        /// <summary>
        /// Downloads a file stored in GridFS and returns it as a byte array.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a byte array containing the contents of the file stored in GridFS.</returns>
        public Task<byte[]> DownloadAsBytesAsync(BsonValue id, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _bsonValueBucket.DownloadAsBytesAsync(id, options, cancellationToken);
        }

        /// <summary>
        /// Downloads a file stored in GridFS and writes the contents to a stream.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void DownloadToStream(BsonValue id, Stream destination, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            _bsonValueBucket.DownloadToStream(id, destination, options, cancellationToken);
        }

        /// <summary>
        /// Downloads a file stored in GridFS and writes the contents to a stream.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public Task DownloadToStreamAsync(BsonValue id, Stream destination, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _bsonValueBucket.DownloadToStreamAsync(id, destination, options, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncCursor<GridFSFileInfo> Find(FilterDefinition<GridFSFileInfo> filter, GridFSFindOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(filter, nameof(filter));
            var wrappedFilter = WrapFilter(filter);
            var wrappedOptions = WrapFindOptions(options);
            var cursor = base.Find(wrappedFilter, wrappedOptions, cancellationToken);
            return new BatchTransformingAsyncCursor<GridFSFileInfo<ObjectId>, GridFSFileInfo>(cursor, TransformFileInfos);
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<GridFSFileInfo>> FindAsync(FilterDefinition<GridFSFileInfo> filter, GridFSFindOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(filter, nameof(filter));
            var wrappedFilter = WrapFilter(filter);
            var wrappedOptions = WrapFindOptions(options);
            var cursor = await base.FindAsync(wrappedFilter, wrappedOptions, cancellationToken).ConfigureAwait(false);
            return new BatchTransformingAsyncCursor<GridFSFileInfo<ObjectId>, GridFSFileInfo>(cursor, TransformFileInfos);
        }

        /// <summary>
        /// Opens a Stream that can be used by the application to read data from a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Stream.</returns>
        public GridFSDownloadStream OpenDownloadStream(BsonValue id, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var wrappedStream = _bsonValueBucket.OpenDownloadStream(id, options, cancellationToken);
            return new GridFSDownloadStream(wrappedStream);
        }

        /// <summary>
        /// Opens a Stream that can be used by the application to read data from a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a Stream.</returns>
        public async Task<GridFSDownloadStream> OpenDownloadStreamAsync(BsonValue id, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var wrappedStream = await _bsonValueBucket.OpenDownloadStreamAsync(id, options, cancellationToken).ConfigureAwait(false);
            return new GridFSDownloadStream(wrappedStream);
        }

        /// <inheritdoc />
        public GridFSUploadStream OpenUploadStream(string filename, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var id = ObjectId.GenerateNewId();
            var wrappedStream = base.OpenUploadStream(id, filename, options, cancellationToken);
            return new GridFSUploadStream(wrappedStream);
        }

        /// <inheritdoc />
        public async Task<GridFSUploadStream> OpenUploadStreamAsync(string filename, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var id = ObjectId.GenerateNewId();
            var wrappedStream = await base.OpenUploadStreamAsync(id, filename, options, cancellationToken).ConfigureAwait(false);
            return new GridFSUploadStream(wrappedStream);
        }

        /// <summary>
        /// Renames a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="newFilename">The new filename.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void Rename(BsonValue id, string newFilename, CancellationToken cancellationToken = default(CancellationToken))
        {
            _bsonValueBucket.Rename(id, newFilename, cancellationToken);
        }

        /// <summary>
        /// Renames a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="newFilename">The new filename.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public Task RenameAsync(BsonValue id, string newFilename, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _bsonValueBucket.RenameAsync(id, newFilename, cancellationToken);
        }

        /// <inheritdoc />
        public ObjectId UploadFromBytes(string filename, byte[] source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var id = ObjectId.GenerateNewId();
            UploadFromBytes(id, filename, source, options, cancellationToken);
            return id;
        }

        /// <inheritdoc />
        public async Task<ObjectId> UploadFromBytesAsync(string filename, byte[] source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var id = ObjectId.GenerateNewId();
            await UploadFromBytesAsync(id, filename, source, options, cancellationToken).ConfigureAwait(false);
            return id;
        }

        /// <inheritdoc />
        public ObjectId UploadFromStream(string filename, Stream source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var id = ObjectId.GenerateNewId();
            UploadFromStream(id, filename, source, options, cancellationToken);
            return id;
        }

        /// <inheritdoc />
        public async Task<ObjectId> UploadFromStreamAsync(string filename, Stream source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var id = ObjectId.GenerateNewId();
            await UploadFromStreamAsync(id, filename, source, options, cancellationToken).ConfigureAwait(false);
            return id;
        }

        // private methods
        private IEnumerable<GridFSFileInfo> TransformFileInfos(IEnumerable<GridFSFileInfo<ObjectId>> fileInfos)
        {
            return fileInfos.Select(fi => new GridFSFileInfo(fi.BackingDocument));
        }

        private FilterDefinition<GridFSFileInfo<ObjectId>> WrapFilter(FilterDefinition<GridFSFileInfo> filter)
        {
            var renderedFilter = filter.Render(GridFSFileInfoSerializer.Instance, BsonSerializer.SerializerRegistry);
            return new BsonDocumentFilterDefinition<GridFSFileInfo<ObjectId>>(renderedFilter);
        }

        private GridFSFindOptions<ObjectId> WrapFindOptions(GridFSFindOptions options)
        {
            if (options != null)
            {
                var renderedSort = options.Sort == null ? null : options.Sort.Render(GridFSFileInfoSerializer.Instance, BsonSerializer.SerializerRegistry);
                var wrappedSort = renderedSort == null ? null : new BsonDocumentSortDefinition<GridFSFileInfo<ObjectId>>(renderedSort);
                return new GridFSFindOptions<ObjectId>
                {
                    BatchSize = options.BatchSize,
                    Limit = options.Limit,
                    MaxTime = options.MaxTime,
                    NoCursorTimeout = options.NoCursorTimeout,
                    Skip = options.Skip,
                    Sort = wrappedSort
                };
            }
            else
            {
                return null;
            }
        }
    }
}
