/* Copyright 2017-present MongoDB Inc.
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

using MongoDB.Bson;

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for a $changeStream stage.
    /// </summary>
    public class ChangeStreamStageOptions
    {
        // private fields
        private bool? _allChangesForCluster;
        private ChangeStreamFullDocumentOption _fullDocument = ChangeStreamFullDocumentOption.Default;
        private BsonDocument _resumeAfter;
        private BsonTimestamp _startAtOperationTime;

        // public properties
        /// <summary>
        /// Gets or sets whether to include all changes for an entire cluster in the change stream.
        /// </summary>
        /// <value>
        /// Whether to include all changes for an entire cluster in the change stream.
        /// </value>
        public bool? AllChangesForCluster
        {
            get { return _allChangesForCluster; }
            set { _allChangesForCluster = value; }
        }
        /// <summary>
        /// Gets or sets the full document.
        /// </summary>
        /// <value>
        /// The full document.
        /// </value>
        public ChangeStreamFullDocumentOption FullDocument
        {
            get { return _fullDocument; }
            set { _fullDocument = value; }
        }

        /// <summary>
        /// Gets or sets the resume after.
        /// </summary>
        /// <value>
        /// The resume after.
        /// </value>
        public BsonDocument ResumeAfter
        {
            get { return _resumeAfter; }
            set { _resumeAfter = value; }
        }

        /// <summary>
        /// Gets or sets the start at operation time.
        /// </summary>
        /// <value>
        /// The start at operation time.
        /// </value>
        public BsonTimestamp StartAtOperationTime
        {
            get { return _startAtOperationTime; }
            set { _startAtOperationTime = value; }
        }
    }
}
