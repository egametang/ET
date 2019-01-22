/* Copyright 2018–present MongoDB Inc.
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

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// Per RFC5802: https://tools.ietf.org/html/rfc5802
    /// "SCRAM is a SASL mechanism whose client response and server challenge
    /// messages are text-based messages containing one or more attribute-
    /// value pairs separated by commas. Each attribute has a one-letter
    /// name."  
    /// </summary>
    internal static class SaslMapParser
    {
        private const int EOF = -1;

        public static IDictionary<char, string> Parse(string text)
        {
            IDictionary<char, string> dict = new Dictionary<char, string>();

            using (var reader = new StringReader(text))
            {
                while (reader.Peek() != EOF)
                {
                    dict.Add(ReadKeyValue(reader));
                    if (reader.Peek() == ',')
                    {
                        Read(reader, ',');
                    }
                }
            }

            return dict;
        }

        private static KeyValuePair<char, string> ReadKeyValue(TextReader reader)
        {
            var key = ReadKey(reader);
            Read(reader, '=');
            var value = ReadValue(reader);
            return new KeyValuePair<char, string>(key, value);
        }

        private static char ReadKey(TextReader reader)
        {
            // keys are of length 1.
            return (char)reader.Read();
        }

        private static void Read(TextReader reader, char expected)
        {
            var ch = (char)reader.Read();
            if (ch != expected)
            {
                throw new IOException(string.Format("Expected {0} but found {1}.", expected, ch));
            }
        }

        private static string ReadValue(TextReader reader)
        {
            var sb = new StringBuilder();
            var ch = reader.Peek();
            while (ch != ',' && ch != EOF)
            {
                sb.Append((char)reader.Read());
                ch = reader.Peek();
            }

            return sb.ToString();
        }
    }
}
