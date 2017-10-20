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
    public class JsonReaderBookmark : BsonReaderBookmark
    {
        // private fields
        private JsonReaderContext _context;
        private JsonToken _currentToken;
        private BsonValue _currentValue;
        private JsonToken _pushedToken;
        private int _position;

        // constructors
        internal JsonReaderBookmark(
            BsonReaderState state,
            BsonType currentBsonType,
            string currentName,
            JsonReaderContext context,
            JsonToken currentToken,
            BsonValue currentValue,
            JsonToken pushedToken,
            int position)
            : base(state, currentBsonType, currentName)
        {
            _context = context.Clone();
            _currentToken = currentToken;
            _currentValue = currentValue;
            _pushedToken = pushedToken;
            _position = position;
        }

        // internal properties
        internal JsonToken CurrentToken
        {
            get { return _currentToken; }
        }

        internal BsonValue CurrentValue
        {
            get { return _currentValue; }
        }

        internal int Position
        {
            get { return _position; }
        }

        internal JsonToken PushedToken
        {
            get { return _pushedToken; }
        }

        // internal methods
        internal JsonReaderContext CloneContext()
        {
            return _context.Clone();
        }
    }
}
