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
    internal class JsonWriterContext
    {
        // private fields
        private JsonWriterContext _parentContext;
        private ContextType _contextType;
        private string _indentation;
        private bool _hasElements = false;

        // constructors
        internal JsonWriterContext(JsonWriterContext parentContext, ContextType contextType, string indentChars)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _indentation = (parentContext == null) ? indentChars : parentContext.Indentation + indentChars;
        }

        // internal properties
        internal JsonWriterContext ParentContext
        {
            get { return _parentContext; }
        }

        internal ContextType ContextType
        {
            get { return _contextType; }
        }

        internal string Indentation
        {
            get { return _indentation; }
        }

        internal bool HasElements
        {
            get { return _hasElements; }
            set { _hasElements = value; }
        }
    }
}
