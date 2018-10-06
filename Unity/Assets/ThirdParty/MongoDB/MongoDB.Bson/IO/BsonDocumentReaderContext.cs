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
    internal class BsonDocumentReaderContext
    {
        // private fields
        private BsonDocumentReaderContext _parentContext;
        private ContextType _contextType;
        private BsonDocument _document;
        private BsonArray _array;
        private int _index;

        // constructors
        internal BsonDocumentReaderContext(
            BsonDocumentReaderContext parentContext,
            ContextType contextType,
            BsonArray array)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _array = array;
        }

        internal BsonDocumentReaderContext(
            BsonDocumentReaderContext parentContext,
            ContextType contextType,
            BsonDocument document)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _document = document;
        }

        // used by Clone
        private BsonDocumentReaderContext(
            BsonDocumentReaderContext parentContext,
            ContextType contextType,
            BsonDocument document,
            BsonArray array,
            int index)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _document = document;
            _array = array;
            _index = index;
        }

        // internal properties
        internal BsonArray Array
        {
            get { return _array; }
        }

        internal ContextType ContextType
        {
            get { return _contextType; }
        }

        internal BsonDocument Document
        {
            get { return _document; }
        }

        internal int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        // public methods
        /// <summary>
        /// Creates a clone of the context.
        /// </summary>
        /// <returns>A clone of the context.</returns>
        public BsonDocumentReaderContext Clone()
        {
            return new BsonDocumentReaderContext(_parentContext, _contextType, _document, _array, _index);
        }

        public bool TryGetNextElement(out BsonElement element)
        {
            if (_index < _document.ElementCount)
            {
                element = _document.GetElement(_index++);
                return true;
            }
            else
            {
                element = default(BsonElement);
                return false;
            }
        }

        public bool TryGetNextValue(out BsonValue value)
        {
            if (_index < _array.Count)
            {
                value = _array[_index++];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public BsonDocumentReaderContext PopContext()
        {
            return _parentContext;
        }
    }
}
