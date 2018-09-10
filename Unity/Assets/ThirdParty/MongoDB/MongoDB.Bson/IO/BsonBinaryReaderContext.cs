/* Copyright 2010-2016 MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    internal class BsonBinaryReaderContext
    {
        // private fields
        private readonly BsonBinaryReaderContext _parentContext;
        private readonly ContextType _contextType;
        private readonly long _startPosition;
        private readonly long _size;
        private string _currentElementName;
        private int _currentArrayIndex = -1;

        // constructors
        internal BsonBinaryReaderContext(
            BsonBinaryReaderContext parentContext,
            ContextType contextType,
            long startPosition,
            long size)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _startPosition = startPosition;
            _size = size;
        }

        // public properties
        public ContextType ContextType
        {
            get { return _contextType; }
        }

        public int CurrentArrayIndex
        {
            get { return _currentArrayIndex; }
            set { _currentArrayIndex = value; }
        }

        public string CurrentElementName
        {
            get { return _currentElementName; }
            set { _currentElementName = value; }
        }

        public BsonBinaryReaderContext ParentContext
        {
            get { return _parentContext; }
        }

        // public methods
        /// <summary>
        /// Creates a clone of the context.
        /// </summary>
        /// <returns>A clone of the context.</returns>
        public BsonBinaryReaderContext Clone()
        {
            return new BsonBinaryReaderContext(_parentContext, _contextType, _startPosition, _size);
        }

        public BsonBinaryReaderContext PopContext(long position)
        {
            var actualSize = position - _startPosition;
            if (actualSize != _size)
            {
                var message = string.Format("Expected size to be {0}, not {1}.", _size, actualSize);
                throw new FormatException(message);
            }
            return _parentContext;
        }
    }
}
