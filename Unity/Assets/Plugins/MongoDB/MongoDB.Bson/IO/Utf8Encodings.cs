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

using System.Text;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a singleton instance of a strict UTF8Encoding.
    /// </summary>
    public static class Utf8Encodings
    {
        // private static fields
        private static readonly UTF8Encoding __lenient = new UTF8Encoding(false, false);
        private static readonly UTF8Encoding __strict = new UTF8Encoding(false, true);

        // public static properties
        /// <summary>
        /// Gets the lenient instance.
        /// </summary>
        public static UTF8Encoding Lenient
        {
            get { return __lenient; }
        }

        /// <summary>
        /// Gets the strict instance.
        /// </summary>
        public static UTF8Encoding Strict
        {
            get { return __strict; }
        }
    }
}
