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
    /// Represents a Stream used by the application to read data from a GridFS file.
    /// </summary>
    public class GridFSDownloadStream : DelegatingStream
    {
        // fields
        private readonly GridFSDownloadStream<BsonValue> _wrappedStream;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GridFSDownloadStream"/> class.
        /// </summary>
        /// <param name="wrappedStream">The wrapped stream.</param>
        public GridFSDownloadStream(GridFSDownloadStream<BsonValue> wrappedStream)
            : base(wrappedStream)
        {
            _wrappedStream = wrappedStream;
        }

        // public properties
        /// <summary>
        /// Gets the files collection document.
        /// </summary>
        /// <value>
        /// The files collection document.
        /// </value>
        public GridFSFileInfo FileInfo
        {
            get
            {
                var wrappedFileInfo = _wrappedStream.FileInfo;
                return new GridFSFileInfo(wrappedFileInfo.BackingDocument);
            }
        }
    }
}
