/* Copyright 2019-present MongoDB Inc.
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

using System.Globalization;

namespace MongoDB.Shared
{
    internal static class HexUtils
    {
        public static bool IsValidHexDigit(char c)
        {
            return
                c >= '0' && c <= '9' ||
                c >= 'a' && c <= 'f' ||
                c >= 'A' && c <= 'F';
        }

        public static bool IsValidHexString(string s)
        {
            for (var i = 0; i < s.Length; i++)
            {
                if (!IsValidHexDigit(s[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static int ParseInt32(string value)
        {
            return int.Parse(value, NumberStyles.HexNumber);
        }
    }
}
