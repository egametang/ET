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

using System;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// An informational event used for logging Server Discovery and Monitoring (SDAM) events. 
    /// </summary>
    public struct SdamInformationEvent
    {
        private readonly Lazy<string> _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="SdamInformationEvent"/> struct.
        /// </summary>
        /// <param name="createMessage">Function that creates the message to log.</param>
        public SdamInformationEvent(Func<string> createMessage) 
            : this(new Lazy<string>(createMessage))
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SdamInformationEvent"/> struct.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public SdamInformationEvent(Lazy<string> message)
        {
            _message = message;
        }
        
        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message => _message.Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return $@"{{type: ""{GetType().Name}"", message: ""{Message}"" }}";
        }
    }
}
