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
    /// Represents a Stream used by the application to write data to a GridFS file.
    /// </summary>
    /// <typeparam name="TFileId">The type of the file identifier.</typeparam>
    public abstract class GridFSUploadStream<TFileId> : Stream
    {
        // constructors
        internal GridFSUploadStream()
        {
        }

        // public properties
        /// <summary>
        /// Gets the id of the file being added to GridFS.
        /// </summary>
        /// <value>
        /// The id of the file being added to GridFS.
        /// </value>
        public abstract TFileId Id { get; }

        // public methods
        /// <summary>
        /// Aborts an upload operation.
        /// </summary>
        /// <remarks>
        /// Any partial results already written to the server are deleted when Abort is called.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        public abstract void Abort(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Aborts an upload operation.
        /// </summary>
        /// <remarks>
        /// Any partial results already written to the server are deleted when AbortAsync is called.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public abstract Task AbortAsync(CancellationToken cancellationToken = default(CancellationToken));

#if NETSTANDARD1_5 || NETSTANDARD1_6
        /// <summary>
        /// Closes the GridFS stream.
        /// </summary>
        public virtual void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#endif

        /// <summary>
        /// Closes the Stream and completes the upload operation.
        /// </summary>
        /// <remarks>
        /// Any data remaining in the Stream is flushed to the server and the GridFS files collection document is written.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        public abstract void Close(CancellationToken cancellationToken);

        /// <summary>
        /// Closes the Stream and completes the upload operation.
        /// </summary>
        /// <remarks>
        /// Any data remaining in the Stream is flushed to the server and the GridFS files collection document is written.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public abstract Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
