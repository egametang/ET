/* Copyright 2013-2015 MongoDB Inc.
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
using System.Text;

namespace MongoDB.Driver.Core.Misc
{
    internal static class StringBuilderExtensionMethods
    {
        public static StringBuilder AppendIf(this StringBuilder sb, bool condition, string value)
        {
            if (condition)
            {
                return sb.Append(value);
            }

            return sb;
        }

        public static StringBuilder AppendFormatIf(this StringBuilder sb, bool condition, string format, params object[] args)
        {
            if (condition)
            {
                return sb.AppendFormat(format, args);
            }

            return sb;
        }
    }
}
