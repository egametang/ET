/* Copyright 2016 MongoDB Inc.
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
    internal static class CStringUtf8Encoding
    {
        public static int GetBytes(string value, byte[] bytes, int byteIndex, UTF8Encoding fallbackEncoding)
        {
            var charLength = value.Length;
            var initialByteIndex = byteIndex;

            for (var charIndex = 0; charIndex < charLength; charIndex++)
            {
                var c = (int)value[charIndex];
                if (c == 0)
                {
                    throw new ArgumentException("A CString cannot contain null bytes.", "value");
                }
                else if (c <= 0x7f)
                {
                    bytes[byteIndex++] = (byte)c;
                }
                else if (c <= 0x7ff)
                {
                    var byte1 = 0xc0 | (c >> 6);
                    var byte2 = 0x80 | (c & 0x3f);
                    bytes[byteIndex++] = (byte)byte1;
                    bytes[byteIndex++] = (byte)byte2;
                }
                else if (c <= 0xd7ff || c >= 0xe000)
                {
                    var byte1 = 0xe0 | (c >> 12);
                    var byte2 = 0x80 | ((c >> 6) & 0x3f);
                    var byte3 = 0x80 | (c & 0x3f);
                    bytes[byteIndex++] = (byte)byte1;
                    bytes[byteIndex++] = (byte)byte2;
                    bytes[byteIndex++] = (byte)byte3;
                }
                else
                {
                    // let fallback encoding handle surrogate pairs
                    var bytesWritten = fallbackEncoding.GetBytes(value, 0, value.Length, bytes, byteIndex);
                    if (Array.IndexOf<byte>(bytes, 0, initialByteIndex, bytesWritten) != -1)
                    {
                        throw new ArgumentException("A CString cannot contain null bytes.", "value");
                    }
                    return bytesWritten;
                }
            }

            return byteIndex - initialByteIndex;
        }

        public static int GetMaxByteCount(int charCount)
        {
            return charCount * 3;
        }
    }
}
