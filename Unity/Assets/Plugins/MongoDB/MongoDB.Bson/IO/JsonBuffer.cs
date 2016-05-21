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
    /// This class represents a JSON string buffer.
    /// </summary>
    public class JsonBuffer
    {
        // private fields
        private string _buffer;
        private int _position;

        // constructors
        /// <summary>
        /// Initializes a new instance of the JsonBuffer class.
        /// </summary>
        /// <param name="buffer">The string.</param>
        public JsonBuffer(string buffer)
        {
            _buffer = buffer;
            _position = 0;
        }

        // internal properties
        /// <summary>
        /// Gets the length of the JSON string.
        /// </summary>
        public int Length
        {
            get { return _buffer.Length; }
        }

        /// <summary>
        /// Gets or sets the current position.
        /// </summary>
        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }

        // public methods
        /// <summary>
        /// Reads a character from the buffer.
        /// </summary>
        /// <returns>The next character (or -1 if at the end of the buffer).</returns>
        public int Read()
        {
            return (_position >= _buffer.Length) ? -1 : _buffer[_position++];
        }

        /// <summary>
        /// Reads a substring from the buffer.
        /// </summary>
        /// <param name="start">The zero based index of the start of the substring.</param>
        /// <returns>The substring.</returns>
        public string Substring(int start)
        {
            return _buffer.Substring(start);
        }

        /// <summary>
        /// Reads a substring from the buffer.
        /// </summary>
        /// <param name="start">The zero based index of the start of the substring.</param>
        /// <param name="count">The number of characters in the substring.</param>
        /// <returns>The substring.</returns>
        public string Substring(int start, int count)
        {
            return _buffer.Substring(start, count);
        }

        /// <summary>
        /// Returns one character to the buffer (if the character matches the one at the current position the current position is moved back by one).
        /// </summary>
        /// <param name="c">The character to return.</param>
        public void UnRead(int c)
        {
            if (c != -1 && _buffer[_position - 1] == c)
            {
                _position -= 1;
            }
        }
    }
}
