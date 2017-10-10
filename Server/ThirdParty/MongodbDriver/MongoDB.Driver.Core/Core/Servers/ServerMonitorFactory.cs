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

using System.Net;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Servers
{
    internal class ServerMonitorFactory : IServerMonitorFactory
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly ServerSettings _settings;

        public ServerMonitorFactory(ServerSettings settings, IConnectionFactory connectionFactory, IEventSubscriber eventSubscriber)
        {
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _connectionFactory = Ensure.IsNotNull(connectionFactory, nameof(connectionFactory));
            _eventSubscriber = Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));
        }

        /// <inheritdoc/>
        public IServerMonitor Create(ServerId serverId, EndPoint endPoint)
        {
            return new ServerMonitor(serverId, endPoint, _connectionFactory, _settings.HeartbeatInterval, _settings.HeartbeatTimeout, _eventSubscriber);
        }
    }
}
