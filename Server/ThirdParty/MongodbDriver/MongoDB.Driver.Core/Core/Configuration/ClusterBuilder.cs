/* Copyright 2010-2015 MongoDB Inc.
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
using System.Linq;
using System.Reflection;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.ConnectionPools;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents a cluster builder.
    /// </summary>
    public class ClusterBuilder
    {
        // fields
        private EventAggregator _eventAggregator;
        private ClusterSettings _clusterSettings;
        private ConnectionPoolSettings _connectionPoolSettings;
        private ConnectionSettings _connectionSettings;
        private ServerSettings _serverSettings;
        private SslStreamSettings _sslStreamSettings;
        private Func<IStreamFactory, IStreamFactory> _streamFactoryWrapper;
        private TcpStreamSettings _tcpStreamSettings;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterBuilder"/> class.
        /// </summary>
        public ClusterBuilder()
        {
            _clusterSettings = new ClusterSettings();
            _serverSettings = new ServerSettings();
            _connectionPoolSettings = new ConnectionPoolSettings();
            _connectionSettings = new ConnectionSettings();
            _tcpStreamSettings = new TcpStreamSettings();
            _streamFactoryWrapper = inner => inner;
            _eventAggregator = new EventAggregator();
        }

        // methods
        /// <summary>
        /// Builds the cluster.
        /// </summary>
        /// <returns>A cluster.</returns>
        public ICluster BuildCluster()
        {
            IStreamFactory streamFactory = new TcpStreamFactory(_tcpStreamSettings);
            if (_sslStreamSettings != null)
            {
                streamFactory = new SslStreamFactory(_sslStreamSettings, streamFactory);
            }

            streamFactory = _streamFactoryWrapper(streamFactory);

            var connectionFactory = new BinaryConnectionFactory(
                _connectionSettings,
                streamFactory,
                _eventAggregator);

            var connectionPoolFactory = new ExclusiveConnectionPoolFactory(
                _connectionPoolSettings,
                connectionFactory,
                _eventAggregator);

            var serverMonitorFactory = new ServerMonitorFactory(
                _serverSettings,
                connectionFactory,
                _eventAggregator);

            var serverFactory = new ServerFactory(
                _clusterSettings.ConnectionMode,
                _serverSettings,
                connectionPoolFactory,
                serverMonitorFactory,
                _eventAggregator);

            var clusterFactory = new ClusterFactory(
                _clusterSettings,
                serverFactory,
                _eventAggregator);

            return clusterFactory.CreateCluster();
        }

        /// <summary>
        /// Configures the cluster settings.
        /// </summary>
        /// <param name="configurator">The cluster settings configurator delegate.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder ConfigureCluster(Func<ClusterSettings, ClusterSettings> configurator)
        {
            Ensure.IsNotNull(configurator, nameof(configurator));

            _clusterSettings = configurator(_clusterSettings);
            return this;
        }

        /// <summary>
        /// Configures the connection settings.
        /// </summary>
        /// <param name="configurator">The connection settings configurator delegate.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder ConfigureConnection(Func<ConnectionSettings, ConnectionSettings> configurator)
        {
            Ensure.IsNotNull(configurator, nameof(configurator));

            _connectionSettings = configurator(_connectionSettings);
            return this;
        }

        /// <summary>
        /// Configures the connection pool settings.
        /// </summary>
        /// <param name="configurator">The connection pool settings configurator delegate.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder ConfigureConnectionPool(Func<ConnectionPoolSettings, ConnectionPoolSettings> configurator)
        {
            Ensure.IsNotNull(configurator, nameof(configurator));

            _connectionPoolSettings = configurator(_connectionPoolSettings);
            return this;
        }

        /// <summary>
        /// Configures the server settings.
        /// </summary>
        /// <param name="configurator">The server settings configurator delegate.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder ConfigureServer(Func<ServerSettings, ServerSettings> configurator)
        {
            _serverSettings = configurator(_serverSettings);
            return this;
        }

        /// <summary>
        /// Configures the SSL stream settings.
        /// </summary>
        /// <param name="configurator">The SSL stream settings configurator delegate.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder ConfigureSsl(Func<SslStreamSettings, SslStreamSettings> configurator)
        {
            _sslStreamSettings = configurator(_sslStreamSettings ?? new SslStreamSettings());
            return this;
        }

        /// <summary>
        /// Configures the TCP stream settings.
        /// </summary>
        /// <param name="configurator">The TCP stream settings configurator delegate.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder ConfigureTcp(Func<TcpStreamSettings, TcpStreamSettings> configurator)
        {
            Ensure.IsNotNull(configurator, nameof(configurator));

            _tcpStreamSettings = configurator(_tcpStreamSettings);
            return this;
        }

        /// <summary>
        /// Registers a stream factory wrapper.
        /// </summary>
        /// <param name="wrapper">The stream factory wrapper.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder RegisterStreamFactory(Func<IStreamFactory, IStreamFactory> wrapper)
        {
            Ensure.IsNotNull(wrapper, nameof(wrapper));

            _streamFactoryWrapper = inner => wrapper(_streamFactoryWrapper(inner));
            return this;
        }

        /// <summary>
        /// Subscribes to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder Subscribe<TEvent>(Action<TEvent> handler)
        {
            Ensure.IsNotNull(handler, nameof(handler));
            _eventAggregator.Subscribe(handler);
            return this;
        }

        /// <summary>
        /// Subscribes the specified subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder Subscribe(IEventSubscriber subscriber)
        {
            Ensure.IsNotNull(subscriber, nameof(subscriber));

            _eventAggregator.Subscribe(subscriber);
            return this;
        }
    }
}