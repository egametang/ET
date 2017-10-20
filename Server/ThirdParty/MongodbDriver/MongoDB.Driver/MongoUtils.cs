/* Copyright 2010-2017 MongoDB Inc.
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
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;

namespace MongoDB.Driver
{
    /// <summary>
    /// Various static utility methods.
    /// </summary>
    public static class MongoUtils
    {
        // public static methods
        /// <summary>
        /// Gets the MD5 hash of a string.
        /// </summary>
        /// <param name="text">The string to get the MD5 hash of.</param>
        /// <returns>The MD5 hash.</returns>
        public static string Hash(string text)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BsonUtils.ToHexString(hash);
            }
        }

        /// <summary>
        /// Creates a TimeSpan from microseconds.
        /// </summary>
        /// <param name="microseconds">The microseconds.</param>
        /// <returns>The TimeSpan.</returns>
        public static TimeSpan TimeSpanFromMicroseconds(long microseconds)
        {
            return TimeSpan.FromTicks(microseconds * 10); // there are 10 ticks in a microsecond
        }

        /// <summary>
        /// Converts a string to camel case by lower casing the first letter (only the first letter is modified).
        /// </summary>
        /// <param name="value">The string to camel case.</param>
        /// <returns>The camel cased string.</returns>
        public static string ToCamelCase(string value)
        {
            return value.Length == 0 ? "" : value.Substring(0, 1).ToLower() + value.Substring(1);
        }

        // internal methods
        /// <summary>
        /// Should only be used when the safety of the data cannot be guaranteed.  For instance,
        /// when the secure string is a password used in a plain text protocol.
        /// </summary>
        /// <param name="secureString">The secure string.</param>
        /// <returns>The CLR string.</returns>
        internal static string ToInsecureString(SecureString secureString)
        {
            if (secureString == null || secureString.Length == 0)
            {
                return "";
            }
            else
            {
#if NET45
                var secureStringIntPtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
#else
                var secureStringIntPtr = SecureStringMarshal.SecureStringToGlobalAllocUnicode(secureString);
#endif
                try
                {
                    return Marshal.PtrToStringUni(secureStringIntPtr, secureString.Length);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(secureStringIntPtr);
                }
            }
        }
    }
}
