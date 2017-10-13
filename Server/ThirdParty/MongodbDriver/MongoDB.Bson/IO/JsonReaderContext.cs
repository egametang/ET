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
    internal class JsonReaderContext
    {
        // private fields
        private JsonReaderContext _parentContext;
        private ContextType _contextType;

        // constructors
        // used by Clone
        private JsonReaderContext()
        {
        }

        internal JsonReaderContext(JsonReaderContext parentContext, ContextType contextType)
        {
            _parentContext = parentContext;
            _contextType = contextType;
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
        public JsonReaderContext Clone()
        {
            return new JsonReaderContext(_parentContext, _contextType);
        }

        public JsonReaderContext PopContext()
        {
            return _parentContext;
        }
    }
}
