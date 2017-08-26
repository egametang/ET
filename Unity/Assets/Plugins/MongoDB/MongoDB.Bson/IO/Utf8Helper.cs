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
    /// Represents a class that has some helper methods for decoding UTF8 strings.
    /// </summary>
    public static class Utf8Helper
    {
        // private static fields
        private static readonly string[] __asciiStringTable = new string[128];

        // static constructor
        static Utf8Helper()
        {
            for (int i = 0; i < __asciiStringTable.Length; ++i)
            {
                __asciiStringTable[i] = new string((char)i, 1);
            }
        }

        // public static methods
        /// <summary>
        /// Decodes a UTF8 string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>The decoded string.</returns>
        public static string DecodeUtf8String(byte[] bytes, int index, int count, UTF8Encoding encoding)
        {
            switch (count)
            {
                // special case empty strings
                case 0:
                    return "";

                // special case single character strings
                case 1:
                    var byte1 = (int)bytes[index];
                    if (byte1 < __asciiStringTable.Length)
                    {
                        return __asciiStringTable[byte1];
                    }
                    break;
            }

            return encoding.GetString(bytes, index, count);
        }
    }
}
