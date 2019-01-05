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

namespace MongoDB.Driver.GridFS
{
    /// <summary>
    /// Represents options for a GridFS download operation.
    /// </summary>
    public class GridFSDownloadOptions
    {
        // fields
        private bool? _checkMD5;
        private bool? _seekable;

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether to check the MD5 value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the MD5 value should be checked; otherwise, <c>false</c>.
        /// </value>
        public bool? CheckMD5
        {
            get { return _checkMD5; }
            set { _checkMD5 = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the returned Stream supports seeking.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the returned Stream supports seeking; otherwise, <c>false</c>.
        /// </value>
        public bool? Seekable
        {
            get { return _seekable; }
            set { _seekable = value; }
        }
    }
}
