/* Copyright 2010-2014 MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a bookmark that can be used to return a reader to the current position and state.
    /// </summary>
    public abstract class BsonReaderBookmark
    {
        // private fields
        private BsonReaderState _state;
        private BsonType _currentBsonType;
        private string _currentName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonReaderBookmark class.
        /// </summary>
        /// <param name="state">The state of the reader.</param>
        /// <param name="currentBsonType">The current BSON type.</param>
        /// <param name="currentName">The name of the current element.</param>
        protected BsonReaderBookmark(BsonReaderState state, BsonType currentBsonType, string currentName)
        {
            _state = state;
            _currentBsonType = currentBsonType;
            _currentName = currentName;
        }

        // public properties
        /// <summary>
        /// Gets the current state of the reader.
        /// </summary>
        public BsonReaderState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the current BsonType;
        /// </summary>
        public BsonType CurrentBsonType
        {
            get { return _currentBsonType; }
        }

        /// <summary>
        /// Gets the name of the current element.
        /// </summary>
        public string CurrentName
        {
            get { return _currentName; }
        }
    }
}
