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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders
{
    /// <summary>
    /// Represents the names of different encoder settings.
    /// </summary>
    public static class MessageEncoderSettingsName
    {
        // encoder settings used by the binary encoders
        /// <summary>
        /// The name of the FixOldBinarySubTypeOnInput setting.
        /// </summary>
        public const string FixOldBinarySubTypeOnInput = "FixOldBinarySubTypeOnInput";

        /// <summary>
        /// The name of the FixOldBinarySubTypeOnOutput setting.
        /// </summary>
        public const string FixOldBinarySubTypeOnOutput = "FixOldBinarySubTypeOnOutput";

        /// <summary>
        /// The name of the FixOldDateTimeMaxValueOnInput setting.
        /// </summary>
        public const string FixOldDateTimeMaxValueOnInput = "FixOldDateTimeMaxValueOnInput";

        /// <summary>
        /// The name of the GuidRepresentation setting.
        /// </summary>
        public const string GuidRepresentation = "GuidRepresentation";

        /// <summary>
        /// The name of the MaxDocumentSize setting.
        /// </summary>
        public const string MaxDocumentSize = "MaxDocumentSize";

        /// <summary>
        /// The name of the MaxSerializationDepth setting.
        /// </summary>
        public const string MaxSerializationDepth = "MaxSerializationDepth";

        /// <summary>
        /// The name of the ReadEncoding setting.
        /// </summary>
        public const string ReadEncoding = "ReadEncoding";

        /// <summary>
        /// The name of the WriteEncoding setting.
        /// </summary>
        public const string WriteEncoding = "WriteEncoding";

        // additional encoder settings used by the JSON encoders
        /// <summary>
        /// The name of the Indent setting.
        /// </summary>
        public const string Indent = "Indent";

        /// <summary>
        /// The name of the IndentChars setting.
        /// </summary>
        public const string IndentChars = "IndentChars";

        /// <summary>
        /// The name of the NewLineChars setting.
        /// </summary>
        public const string NewLineChars = "NewLineChars";

        /// <summary>
        /// The name of the OutputMode setting.
        /// </summary>
        public const string OutputMode = "OutputMode";

        /// <summary>
        /// The name of the ShellVersion setting.
        /// </summary>
        public const string ShellVersion = "ShellVersion";

        // other encoders (if any) might use additional settings
    }

    /// <summary>
    /// Represents settings for message encoders.
    /// </summary>
    public class MessageEncoderSettings : IEnumerable<KeyValuePair<string, object>>
    {
        // fields
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        // methods
        /// <summary>
        /// Adds a setting.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>The settings.</returns>
        public MessageEncoderSettings Add<T>(string name, T value)
        {
            _settings.Add(name, value);
            return this;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _settings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets a setting, or a default value if the setting does not exist.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the setting, or a default value if the setting does not exist.</returns>
        public T GetOrDefault<T>(string name, T defaultValue)
        {
            object value;
            if (_settings.TryGetValue(name, out value))
            {
                return (T)value;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
