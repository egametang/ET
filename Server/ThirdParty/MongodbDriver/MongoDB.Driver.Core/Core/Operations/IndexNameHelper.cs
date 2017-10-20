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

using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents helper methods for index names.
    /// </summary>
    public static class IndexNameHelper
    {
        // static methods
        /// <summary>
        /// Gets the name of the index derived from the keys specification.
        /// </summary>
        /// <param name="keys">The keys specification.</param>
        /// <returns>The name of the index.</returns>
        public static string GetIndexName(BsonDocument keys)
        {
            Ensure.IsNotNull(keys, nameof(keys));
            var sb = new StringBuilder();

            foreach (var element in keys)
            {
                var value = element.Value;
                string direction;
                switch (value.BsonType)
                {
                    case BsonType.Double:
                    case BsonType.Int32:
                    case BsonType.Int64:
                        direction = value.ToInt32().ToString();
                        break;
                    case BsonType.String:
                        direction = value.ToString().Replace(' ', '_');
                        break;
                    default:
                        direction = "x";
                        break;
                }

                if (sb.Length > 0)
                {
                    sb.Append("_");
                }
                sb.Append(element.Name.Replace(' ', '_'));
                sb.Append("_");
                sb.Append(direction);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the name of the index derived from the key names.
        /// </summary>
        /// <param name="keyNames">The key names.</param>
        /// <returns>The name of the index.</returns>
        public static string GetIndexName(string[] keyNames)
        {
            Ensure.IsNotNull(keyNames, nameof(keyNames));
            var sb = new StringBuilder();

            foreach (var name in keyNames)
            {
                if (sb.Length > 0)
                {
                    sb.Append("_");
                }
                sb.Append(name.Replace(' ', '_'));
                sb.Append("_1");
            }

            return sb.ToString();
        }
    }
}
