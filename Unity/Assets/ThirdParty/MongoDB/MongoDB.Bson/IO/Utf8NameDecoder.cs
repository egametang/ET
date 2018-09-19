/* Copyright 2010-2015 MongoDB Inc.
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
    /// Represents a UTF8 name decoder.
    /// </summary>
    public class Utf8NameDecoder : INameDecoder
    {
        // private static fields
        private static readonly Utf8NameDecoder __instance = new Utf8NameDecoder();

        // public static properties
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static Utf8NameDecoder Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Decodes the name.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>
        /// The name.
        /// </returns>
        public string Decode(BsonStream stream, UTF8Encoding encoding)
        {
            var utf8 = stream.ReadCStringBytes();
            return Utf8Helper.DecodeUtf8String(utf8.Array, utf8.Offset, utf8.Count, encoding);
        }

        /// <summary>
        /// Informs the decoder of an already decoded name (so the decoder can change state if necessary).
        /// </summary>
        /// <param name="name">The name.</param>
        public void Inform(string name)
        {
        }
    }
}
