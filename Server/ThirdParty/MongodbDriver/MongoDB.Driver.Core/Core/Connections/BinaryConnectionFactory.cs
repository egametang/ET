/* Copyright 2013-2016 MongoDB Inc.
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
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Connections
{
    /// <summary>
    /// Represents a factory of BinaryConnections.
    /// </summary>
    internal class BinaryConnectionFactory : IConnectionFactory
    {
        // fields
        private readonly IConnectionInitializer _connectionInitializer;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly ConnectionSettings _settings;
        private readonly IStreamFactory _streamFactory;

        // constructors
        public BinaryConnectionFactory(ConnectionSettings settings, IStreamFactory streamFactory, IEventSubscriber eventSubscriber)
        {
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _streamFactory = Ensure.IsNotNull(streamFactory, nameof(streamFactory));
            _eventSubscriber = Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));
            _connectionInitializer = new ConnectionInitializer(settings.ApplicationName);
        }

        // methods
        public IConnection CreateConnection(ServerId serverId, EndPoint endPoint)
        {
            Ensure.IsNotNull(serverId, nameof(serverId));
            Ensure.IsNotNull(endPoint, nameof(endPoint));
            return new BinaryConnection(serverId, endPoint, _settings, _streamFactory, _connectionInitializer, _eventSubscriber);
        }
    }
}
