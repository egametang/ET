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

namespace MongoDB.Bson.IO
{
    internal class BsonBinaryReaderContext
    {
        // private fields
        private BsonBinaryReaderContext _parentContext;
        private ContextType _contextType;
        private int _startPosition;
        private int _size;

        // constructors
        internal BsonBinaryReaderContext(
            BsonBinaryReaderContext parentContext,
            ContextType contextType,
            int startPosition, 
            int size)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _startPosition = startPosition;
            _size = size;
        }

        // internal properties
        internal ContextType ContextType
        {
            get { return _contextType; }
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

        public BsonBinaryReaderContext PopContext(int position)
        {
            int actualSize = position - _startPosition;
            if (actualSize != _size)
            {
                var message = string.Format("Expected size to be {0}, not {1}.", _size, actualSize);
                throw new Exception(message);
            }
            return _parentContext;
        }
    }
}
