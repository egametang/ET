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

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents settings for SDAM logging.
    /// </summary>
    public class SdamLoggingSettings
    {   
        private readonly string _logFilename;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SdamLoggingSettings"/> class.
        /// </summary>
        /// <param name="logFilename">The filename to log to. An empty string or null  will disable logging.
        /// "stdout" sneds output to stdout. </param>
        public SdamLoggingSettings(Optional<string> logFilename = default(Optional<string>))
        {
            _logFilename = logFilename.WithDefault(null);
        }
        
        /// <summary>
        /// The filename to log to.
        /// </summary>
        public string LogFilename => _logFilename;
        
        // methods
        /// <summary>
        /// Returns a new SdamLoggingSettings instance with some settings changed.
        /// </summary>
        /// <param name="logFilename">The filename.</param>
        /// <returns>A new SdamLoggingSettings instance.</returns>
        public SdamLoggingSettings With(Optional<string> logFilename = default(Optional<string>))
        {
            return new SdamLoggingSettings(logFilename: logFilename.WithDefault(_logFilename));
        }
        
        /// <summary>
        /// Whether or not SDAM logging is enabled.
        /// </summary>
        internal bool IsLoggingEnabled => !string.IsNullOrEmpty(_logFilename);

        /// <summary>
        /// Whether or not logging should be written to stdout.
        /// </summary>
        internal bool ShouldLogToStdout => _logFilename == "stdout";
    }
}