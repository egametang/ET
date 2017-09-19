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
using MongoDB.Bson.IO;

namespace MongoDB.Shared
{
    internal static class ApplicationNameHelper
    {
        public static string EnsureApplicationNameIsValid(string applicationName, string paramName)
        {
            string message;
            if (!IsApplicationNameValid(applicationName, out message))
            { 
                throw new ArgumentException(message, paramName);
            }

            return applicationName;
        }

        public static bool IsApplicationNameValid(string applicationName, out string message)
        {
            if (applicationName != null)
            {
                var utf8 = Utf8Encodings.Strict.GetBytes(applicationName);
                if (utf8.Length > 128)
                {
                    message = "Application name exceeds 128 bytes after encoding to UTF8.";
                    return false;
                }
            }

            message = null;
            return true;
        }
    }
}
