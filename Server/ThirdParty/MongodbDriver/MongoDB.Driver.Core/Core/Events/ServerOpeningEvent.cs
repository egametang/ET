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

using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs before a server is opened.
    /// </summary>
    public struct ServerOpeningEvent
    {
        private readonly ServerId _serverId;
        private readonly ServerSettings _serverSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerOpeningEvent"/> struct.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="serverSettings">The server settings.</param>
        public ServerOpeningEvent(ServerId serverId, ServerSettings serverSettings)
        {
            _serverId = serverId;
            _serverSettings = serverSettings;
        }

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        public ClusterId ClusterId
        {
            get { return _serverId.ClusterId; }
        }

        /// <summary>
        /// Gets the server identifier.
        /// </summary>
        public ServerId ServerId
        {
            get { return _serverId; }
        }

        /// <summary>
        /// Gets the server settings.
        /// </summary>
        public ServerSettings ServerSettings
        {
            get { return _serverSettings; }
        }
    }
}
