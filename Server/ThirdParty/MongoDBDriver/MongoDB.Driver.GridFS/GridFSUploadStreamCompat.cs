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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongoDB.Driver.GridFS
{
    /// <summary>
    /// Represents a Stream used by the application to write data to a GridFS file.
    /// </summary>
    public class GridFSUploadStream : DelegatingStream
    {
        // fields
        private readonly GridFSUploadStream<ObjectId> _wrappedStream;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GridFSUploadStream"/> class.
        /// </summary>
        /// <param name="wrappedStream">The wrapped stream.</param>
        internal GridFSUploadStream(GridFSUploadStream<ObjectId> wrappedStream)
            : base(wrappedStream)
        {
            _wrappedStream = wrappedStream;
        }

        // public properties
        /// <summary>
        /// Gets the id of the file being added to GridFS.
        /// </summary>
        /// <value>
        /// The id of the file being added to GridFS.
        /// </value>
        public ObjectId Id
        {
            get { return _wrappedStream.Id; }
        }

        // public methods
        /// <summary>
        /// Aborts an upload operation.
        /// </summary>
        /// <remarks>
        /// Any partial results already written to the server are deleted when Abort is called.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void Abort(CancellationToken cancellationToken = default(CancellationToken))
        {
            _wrappedStream.Abort(cancellationToken);
        }

        /// <summary>
        /// Aborts an upload operation.
        /// </summary>
        /// <remarks>
        /// Any partial results already written to the server are deleted when AbortAsync is called.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public Task AbortAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _wrappedStream.AbortAsync(cancellationToken);
        }

#if NETSTANDARD1_5 || NETSTANDARD1_6
        /// <summary>
        /// Closes the GridFS stream.
        /// </summary>
        public virtual void Close()
        {
            _wrappedStream.Close();
        }
#endif

        /// <summary>
        /// Closes the Stream and completes the upload operation.
        /// </summary>
        /// <remarks>
        /// Any data remaining in the Stream is flushed to the server and the GridFS files collection document is written.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void Close(CancellationToken cancellationToken)
        {
            _wrappedStream.Close(cancellationToken);
        }

        /// <summary>
        /// Closes the Stream and completes the upload operation.
        /// </summary>
        /// <remarks>
        /// Any data remaining in the Stream is flushed to the server and the GridFS files collection document is written.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _wrappedStream.CloseAsync(cancellationToken);
        }
    }
}
