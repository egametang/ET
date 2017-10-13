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
using System.Net.Sockets;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents settings for a server.
    /// </summary>
    public class ServerSettings
    {
        #region static
        // public static methods
        /// <summary>
        /// Gets the default heartbeat interval.
        /// </summary>
        public static TimeSpan DefaultHeartbeatInterval => TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets the default heartbeat timeout.
        /// </summary>
        public static TimeSpan DefaultHeartbeatTimeout => TimeSpan.FromSeconds(10);
        #endregion

        // fields
        private readonly TimeSpan _heartbeatInterval;
        private readonly TimeSpan _heartbeatTimeout;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSettings"/> class.
        /// </summary>
        /// <param name="heartbeatInterval">The heartbeat interval.</param>
        /// <param name="heartbeatTimeout">The heartbeat timeout.</param>
        public ServerSettings(
            Optional<TimeSpan> heartbeatInterval = default(Optional<TimeSpan>),
            Optional<TimeSpan> heartbeatTimeout = default(Optional<TimeSpan>))
        {
            _heartbeatInterval = Ensure.IsInfiniteOrGreaterThanOrEqualToZero(heartbeatInterval.WithDefault(DefaultHeartbeatInterval), "heartbeatInterval");
            _heartbeatTimeout = Ensure.IsInfiniteOrGreaterThanOrEqualToZero(heartbeatTimeout.WithDefault(DefaultHeartbeatTimeout), "heartbeatTimeout");
        }

        // properties
        /// <summary>
        /// Gets the heartbeat interval.
        /// </summary>
        /// <value>
        /// The heartbeat interval.
        /// </value>
        public TimeSpan HeartbeatInterval
        {
            get { return _heartbeatInterval; }
        }

        /// <summary>
        /// Gets the heartbeat timeout.
        /// </summary>
        /// <value>
        /// The heartbeat timeout.
        /// </value>
        public TimeSpan HeartbeatTimeout
        {
            get { return _heartbeatTimeout; }
        }

        // methods
        /// <summary>
        /// Returns a new ServerSettings instance with some settings changed.
        /// </summary>
        /// <param name="heartbeatInterval">The heartbeat interval.</param>
        /// <param name="heartbeatTimeout">The heartbeat timeout.</param>
        /// <returns>A new ServerSettings instance.</returns>
        public ServerSettings With(
            Optional<TimeSpan> heartbeatInterval = default(Optional<TimeSpan>),
            Optional<TimeSpan> heartbeatTimeout = default(Optional<TimeSpan>))
        {
            return new ServerSettings(
                heartbeatInterval: heartbeatInterval.WithDefault(_heartbeatInterval),
                heartbeatTimeout: heartbeatTimeout.WithDefault(_heartbeatTimeout));
        }
    }
}
