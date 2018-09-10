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

using System;
using System.IO;
using System.Text;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a wrapper around a TextReader to provide some buffering functionality.
    /// </summary>
    internal class JsonBuffer
    {
        // private fields
        private readonly StringBuilder _buffer;
        private int _position;
        private readonly TextReader _reader;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonBuffer"/> class.
        /// </summary>
        /// <param name="json">The json.</param>
        public JsonBuffer(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException("json");
            }
            _buffer = new StringBuilder(json);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonBuffer" /> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public JsonBuffer(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            _buffer = new StringBuilder(256); // start out with a reasonable initial capacity
            _reader = reader;
        }

        // public properties
        /// <summary>
        /// Gets or sets the current position.
        /// </summary>
        public int Position
        {
            get { return _position; }
            set
            {
                if (value < 0 || value > _buffer.Length)
                {
                    var message = string.Format("Invalid position: {0}.", value);
                    throw new ArgumentOutOfRangeException("value", message);
                }
                _position = value;
            }
        }

        // public methods
        /// <summary>
        /// Gets a snippet of a maximum length from the buffer (usually to include in an error message).
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>The snippet.</returns>
        public string GetSnippet(int start, int maxLength)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start", "Start cannot be negative.");
            }
            if (maxLength < 0)
            {
                throw new ArgumentOutOfRangeException("maxLength", "MaxLength cannot be negative.");
            }
            if (start > _position)
            {
                throw new ArgumentOutOfRangeException("start", "Start is beyond current position.");
            }
            var availableCount = _position - start;
            var count = Math.Min(availableCount, maxLength);
            return _buffer.ToString(start, count);
        }

        /// <summary>
        /// Gets a substring from the buffer.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        /// <returns>The substring.</returns>
        public string GetSubstring(int start, int count)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start", "Start cannot be negative.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count cannot be negative.");
            }
            if (start > _position)
            {
                throw new ArgumentOutOfRangeException("start", "Start is beyond current position.");
            }
            if (start + count > _position)
            {
                throw new ArgumentOutOfRangeException("start", "End of substring is beyond current position.");
            }
            return _buffer.ToString(start, count);
        }

        /// <summary>
        /// Reads the next character from the text reader and advances the character position by one character.
        /// </summary>
        /// <returns>
        /// The next character from the text reader, or -1 if no more characters are available. The default implementation returns -1.
        /// </returns>
        public int Read()
        {
            ReadMoreIfAtEndOfBuffer();
            return _position >= _buffer.Length ? -1 : _buffer[_position++];
        }

        /// <summary>
        /// Resets the buffer (clears everything up to the current position).
        /// </summary>
        public void ResetBuffer()
        {
            // only trim the buffer if enough space will be reclaimed to make it worthwhile
            var minimumTrimCount = 256; // TODO: make configurable?
            if (_position >= minimumTrimCount)
            {
                _buffer.Remove(0, _position);
                _position = 0;
            }
        }

        /// <summary>
        /// Unreads one character (moving the current Position back one position).
        /// </summary>
        /// <param name="c">The character.</param>
        public void UnRead(int c)
        {
            if (_position == 0)
            {
                throw new InvalidOperationException("Unread called when nothing has been read.");
            }

            if (c == -1)
            {
                if (_position != _buffer.Length)
                {
                    throw new InvalidOperationException("Unread called with -1 when position is not at the end of the buffer.");
                }
            }
            else
            {
                if (_buffer[_position - 1] != c)
                {
                    throw new InvalidOperationException("Unread called with a character that does not match what is in the buffer.");
                }
                _position -= 1;
            }
        }

        // private methods
        private void ReadMoreIfAtEndOfBuffer()
        {
            if (_position >= _buffer.Length)
            {
                if (_reader != null)
                {
                    var blockSize = 1024; // TODO: make configurable?
                    var block = new char[blockSize];
                    var actualCount = _reader.ReadBlock(block, 0, blockSize);

                    if (actualCount > 0)
                    {
                        _buffer.Append(block, 0, actualCount);
                    }
                }
            }
        }
    }
}
