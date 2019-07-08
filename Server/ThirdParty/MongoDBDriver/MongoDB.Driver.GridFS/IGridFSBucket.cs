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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongoDB.Driver.GridFS
{
    /// <summary>
    /// Represents a GridFS system bucket.
    /// </summary>
    /// <typeparam name="TFileId">The type of the file identifier.</typeparam>
    public interface IGridFSBucket<TFileId>
    {
        // properties
        /// <summary>
        /// Gets the database where the GridFS files are stored.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        IMongoDatabase Database { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        ImmutableGridFSBucketOptions Options { get; }

        // methods
        /// <summary>
        /// Deletes a file from GridFS.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void Delete(TFileId id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes a file from GridFS.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task DeleteAsync(TFileId id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a file stored in GridFS and returns it as a byte array.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The contents of the file stored in GridFS.</returns>
        byte[] DownloadAsBytes(TFileId id, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a file stored in GridFS and returns it as a byte array.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a byte array containing the contents of the file stored in GridFS.</returns>
        Task<byte[]> DownloadAsBytesAsync(TFileId id, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a file stored in GridFS and returns it as a byte array.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A byte array containing the contents of the file stored in GridFS.</returns>
        byte[] DownloadAsBytesByName(string filename, GridFSDownloadByNameOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a file stored in GridFS and returns it as a byte array.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a byte array containing the contents of the file stored in GridFS.</returns>
        Task<byte[]> DownloadAsBytesByNameAsync(string filename, GridFSDownloadByNameOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a file stored in GridFS and writes the contents to a stream.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DownloadToStream(TFileId id, Stream destination, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a file stored in GridFS and writes the contents to a stream.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task DownloadToStreamAsync(TFileId id, Stream destination, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a file stored in GridFS and writes the contents to a stream.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DownloadToStreamByName(string filename, Stream destination, GridFSDownloadByNameOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Downloads a file stored in GridFS and writes the contents to a stream.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task DownloadToStreamByNameAsync(string filename, Stream destination, GridFSDownloadByNameOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops the files and chunks collections associated with this GridFS bucket.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        void Drop(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops the files and chunks collections associated with this GridFS bucket.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task DropAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Finds matching entries from the files collection.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor of files collection documents.</returns>
        IAsyncCursor<GridFSFileInfo<TFileId>> Find(FilterDefinition<GridFSFileInfo<TFileId>> filter, GridFSFindOptions<TFileId> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Finds matching entries from the files collection.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor of files collection documents.</returns>
        Task<IAsyncCursor<GridFSFileInfo<TFileId>>> FindAsync(FilterDefinition<GridFSFileInfo<TFileId>> filter, GridFSFindOptions<TFileId> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Opens a Stream that can be used by the application to read data from a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Stream.</returns>
        GridFSDownloadStream<TFileId> OpenDownloadStream(TFileId id, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Opens a Stream that can be used by the application to read data from a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a Stream.</returns>
        Task<GridFSDownloadStream<TFileId>> OpenDownloadStreamAsync(TFileId id, GridFSDownloadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Opens a Stream that can be used by the application to read data from a GridFS file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Stream.</returns>
        GridFSDownloadStream<TFileId> OpenDownloadStreamByName(string filename, GridFSDownloadByNameOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Opens a Stream that can be used by the application to read data from a GridFS file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a Stream.</returns>
        Task<GridFSDownloadStream<TFileId>> OpenDownloadStreamByNameAsync(string filename, GridFSDownloadByNameOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Opens a Stream that can be used by the application to write data to a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Stream.</returns>
        GridFSUploadStream<TFileId> OpenUploadStream(TFileId id, string filename, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Opens a Stream that can be used by the application to write data to a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a Stream.</returns>
        Task<GridFSUploadStream<TFileId>> OpenUploadStreamAsync(TFileId id, string filename, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Renames a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="newFilename">The new filename.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void Rename(TFileId id, string newFilename, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Renames a GridFS file.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="newFilename">The new filename.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task RenameAsync(TFileId id, string newFilename, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Uploads a file (or a new revision of a file) to GridFS.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void UploadFromBytes(TFileId id, string filename, byte[] source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Uploads a file (or a new revision of a file) to GridFS.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task UploadFromBytesAsync(TFileId id, string filename, byte[] source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Uploads a file (or a new revision of a file) to GridFS.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The id of the new file.</returns>
        void UploadFromStream(TFileId id, string filename, Stream source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Uploads a file (or a new revision of a file) to GridFS.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task UploadFromStreamAsync(TFileId id, string filename, Stream source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
