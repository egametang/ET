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
    public interface IGridFSBucket : IGridFSBucket<ObjectId>
    {
        // methods
        /// <summary>
        /// Finds matching entries from the files collection.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor of files collection documents.</returns>
        IAsyncCursor<GridFSFileInfo> Find(FilterDefinition<GridFSFileInfo> filter, GridFSFindOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Finds matching entries from the files collection.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor of files collection documents.</returns>
        Task<IAsyncCursor<GridFSFileInfo>> FindAsync(FilterDefinition<GridFSFileInfo> filter, GridFSFindOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Opens a Stream that can be used by the application to write data to a GridFS file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Stream.</returns>
        GridFSUploadStream OpenUploadStream(string filename, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Opens a Stream that can be used by the application to write data to a GridFS file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a Stream.</returns>
        Task<GridFSUploadStream> OpenUploadStreamAsync(string filename, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Uploads a file (or a new revision of a file) to GridFS.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The id of the new file.</returns>
        ObjectId UploadFromBytes(string filename, byte[] source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Uploads a file (or a new revision of a file) to GridFS.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the id of the new file.</returns>
        Task<ObjectId> UploadFromBytesAsync(string filename, byte[] source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Uploads a file (or a new revision of a file) to GridFS.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The id of the new file.</returns>
        ObjectId UploadFromStream(string filename, Stream source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Uploads a file (or a new revision of a file) to GridFS.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the id of the new file.</returns>
        Task<ObjectId> UploadFromStreamAsync(string filename, Stream source, GridFSUploadOptions options = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
