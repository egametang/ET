/* Copyright 2010-present MongoDB Inc.
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.ConnectionPools;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Events.Diagnostics;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents a cluster builder.
    /// </summary>
    public class ClusterBuilder
    {
        // constants
        private const string __traceSourceName = "MongoDB-SDAM";
        
        // fields
        private EventAggregator _eventAggregator;
        private ClusterSettings _clusterSettings;
        private ConnectionPoolSettings _connectionPoolSettings;
        private ConnectionSettings _connectionSettings;
        private SdamLoggingSettings _sdamLoggingSettings;
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
            _sdamLoggingSettings = new SdamLoggingSettings(null);
            _serverSettings = new ServerSettings();
            _connectionPoolSettings = new ConnectionPoolSettings();
            _connectionSettings = new ConnectionSettings();
            _tcpStreamSettings = new TcpStreamSettings();
            _streamFactoryWrapper = inner => inner;
            _eventAggregator = new EventAggregator();
        }

        // public methods
        /// <summary>
        /// Builds the cluster.
        /// </summary>
        /// <returns>A cluster.</returns>
        public ICluster BuildCluster()
        {
            var clusterFactory = CreateClusterFactory();
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
        /// Configures the SDAM logging settings.
        /// </summary>
        /// <param name="configurator">The SDAM logging settings configurator delegate.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public ClusterBuilder ConfigureSdamLogging(Func<SdamLoggingSettings, SdamLoggingSettings> configurator)
        {
            _sdamLoggingSettings = configurator(_sdamLoggingSettings);
            if (!_sdamLoggingSettings.IsLoggingEnabled)
            {
                return this;
            }
            var traceSource = new TraceSource(__traceSourceName, SourceLevels.All);
            traceSource.Listeners.Clear(); // remove the default listener
            var listener = _sdamLoggingSettings.ShouldLogToStdout
                ? new TextWriterTraceListener(Console.Out)
                : new TextWriterTraceListener(new FileStream(_sdamLoggingSettings.LogFilename, FileMode.Append));
            listener.TraceOutputOptions = TraceOptions.DateTime;
            traceSource.Listeners.Add(listener);
            return this.Subscribe(new TraceSourceSdamEventSubscriber(traceSource));
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

        // private methods
        private IClusterFactory CreateClusterFactory()
        {
            var serverFactory = CreateServerFactory();

            return new ClusterFactory(
                _clusterSettings,
                serverFactory,
                _eventAggregator);
        }

        private IConnectionPoolFactory CreateConnectionPoolFactory()
        {
            var streamFactory = CreateTcpStreamFactory(_tcpStreamSettings);

            var connectionFactory = new BinaryConnectionFactory(
                _connectionSettings,
                streamFactory,
                _eventAggregator);

            return new ExclusiveConnectionPoolFactory(
                _connectionPoolSettings,
                connectionFactory,
                _eventAggregator);
        }

        private ServerFactory CreateServerFactory()
        {
            var connectionPoolFactory = CreateConnectionPoolFactory();
            var serverMonitorFactory = CreateServerMonitorFactory();

            return new ServerFactory(
                _clusterSettings.ConnectionMode,
                _serverSettings,
                connectionPoolFactory,
                serverMonitorFactory,
                _eventAggregator);
        }

        private IServerMonitorFactory CreateServerMonitorFactory()
        {
            var serverMonitorConnectionSettings = _connectionSettings
                .With(authenticators: new IAuthenticator[] { });

            var heartbeatConnectTimeout = _tcpStreamSettings.ConnectTimeout;
            if (heartbeatConnectTimeout == TimeSpan.Zero || heartbeatConnectTimeout == Timeout.InfiniteTimeSpan)
            {
                heartbeatConnectTimeout = TimeSpan.FromSeconds(30);
            }
            var heartbeatSocketTimeout = _serverSettings.HeartbeatTimeout;
            if (heartbeatSocketTimeout == TimeSpan.Zero || heartbeatSocketTimeout == Timeout.InfiniteTimeSpan)
            {
                heartbeatSocketTimeout = heartbeatConnectTimeout;
            }
            var serverMonitorTcpStreamSettings = new TcpStreamSettings(_tcpStreamSettings)
                .With(
                    connectTimeout: heartbeatConnectTimeout,
                    readTimeout: heartbeatSocketTimeout,
                    writeTimeout: heartbeatSocketTimeout
                );

            var serverMonitorStreamFactory = CreateTcpStreamFactory(serverMonitorTcpStreamSettings);

            var serverMonitorConnectionFactory = new BinaryConnectionFactory(
                serverMonitorConnectionSettings,
                serverMonitorStreamFactory,
                new EventAggregator());

            return new ServerMonitorFactory(
                _serverSettings,
                serverMonitorConnectionFactory,
                _eventAggregator);
        }

        private IStreamFactory CreateTcpStreamFactory(TcpStreamSettings tcpStreamSettings)
        {
            var streamFactory = (IStreamFactory)new TcpStreamFactory(tcpStreamSettings);
            if (_sslStreamSettings != null)
            {
                streamFactory = new SslStreamFactory(_sslStreamSettings, streamFactory);
            }

            return _streamFactoryWrapper(streamFactory);
        }
    }
}