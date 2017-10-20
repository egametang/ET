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
    public class BsonBinaryReaderBookmark : BsonReaderBookmark
    {
        // private fields
        private BsonBinaryReaderContext _context;
        private long _position;

        // constructors
        internal BsonBinaryReaderBookmark(
            BsonReaderState state,
            BsonType currentBsonType,
            string currentName,
            BsonBinaryReaderContext context,
            long position)
            : base(state, currentBsonType, currentName)
        {
            _context = context.Clone();
            _position = position;
        }

        // internal properties
        internal long Position
        {
            get { return _position; }
        }

        // internal methods
        internal BsonBinaryReaderContext CloneContext()
        {
            return _context.Clone();
        }
    }
}
