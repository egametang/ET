/* Copyright 2015 MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents text search options.
    /// </summary>
    public class TextSearchOptions
    {
        // private fields
        private bool? _caseSensitive;
        private bool? _diacriticSensitive;
        private string _language;

        // public properties
        /// <summary>
        /// Gets or sets whether a text search should be case sensitive.
        /// </summary>
        public bool? CaseSensitive
        {
            get { return _caseSensitive; }
            set { _caseSensitive = value; }
        }

        /// <summary>
        /// Gets or sets whether a text search should be diacritic sensitive.
        /// </summary>
        public bool? DiacriticSensitive
        {
            get { return _diacriticSensitive; }
            set { _diacriticSensitive = value; }
        }

        /// <summary>
        /// Gets or sets the language for a text search.
        /// </summary>
        public string Language
        {
            get { return _language; }
            set { _language = value; }
        }
    }
}
