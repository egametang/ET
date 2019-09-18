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

namespace MongoDB.Driver.GridFS
{
    /// <summary>
    /// Represents a Stream used by the application to read data from a GridFS file.
    /// </summary>
    /// <typeparam name="TFileId">The type of the file identifier.</typeparam>
    public abstract class GridFSDownloadStream<TFileId> : Stream
    {
        // constructors
        internal GridFSDownloadStream()
        {
        }

        // public properties
        /// <summary>
        /// Gets the files collection document.
        /// </summary>
        /// <value>
        /// The files collection document.
        /// </value>
        public abstract GridFSFileInfo<TFileId> FileInfo { get; }

        // public methods
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
        /// Closes the GridFS stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public abstract void Close(CancellationToken cancellationToken);

        /// <summary>
        /// Closes the GridFS stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public abstract Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
