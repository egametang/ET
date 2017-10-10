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
    internal class BsonDocumentWriterContext
    {
        // private fields
        private BsonDocumentWriterContext _parentContext;
        private ContextType _contextType;
        private BsonDocument _document;
        private BsonArray _array;
        private string _code;
        private string _name;

        // constructors
        internal BsonDocumentWriterContext(
            BsonDocumentWriterContext parentContext,
            ContextType contextType,
            BsonDocument document)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _document = document;
        }

        internal BsonDocumentWriterContext(
            BsonDocumentWriterContext parentContext,
            ContextType contextType,
            BsonArray array)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _array = array;
        }

        internal BsonDocumentWriterContext(
            BsonDocumentWriterContext parentContext,
            ContextType contextType,
            string code)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _code = code;
        }

        // internal properties
        internal BsonDocumentWriterContext ParentContext
        {
            get { return _parentContext; }
        }

        internal string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        internal ContextType ContextType
        {
            get { return _contextType; }
        }

        internal BsonDocument Document
        {
            get { return _document; }
        }

        internal BsonArray Array
        {
            get { return _array; }
        }

        internal string Code
        {
            get { return _code; }
        }
    }
}
