/* Copyright 2010-2016 MongoDB Inc.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Events.Diagnostics;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Extension methods for a ClusterBuilder.
    /// </summary>
    public static class ClusterBuilderExtensions
    {
        /// <summary>
        /// Configures a cluster builder from a connection string.
        /// </summary>
        /// <param name="builder">The cluster builder.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public static ClusterBuilder ConfigureWithConnectionString(this ClusterBuilder builder, string connectionString)
        {
            Ensure.IsNotNull(builder, nameof(builder));
            Ensure.IsNotNullOrEmpty(connectionString, nameof(connectionString));

            var parsedConnectionString = new ConnectionString(connectionString);
            return ConfigureWithConnectionString(builder, parsedConnectionString);
        }

        /// <summary>
        /// Configures a cluster builder from a connection string.
        /// </summary>
        /// <param name="builder">The cluster builder.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public static ClusterBuilder ConfigureWithConnectionString(this ClusterBuilder builder, ConnectionString connectionString)
        {
            Ensure.IsNotNull(builder, nameof(builder));
            Ensure.IsNotNull(connectionString, nameof(connectionString));

            // TCP
            if (connectionString.ConnectTimeout != null)
            {
                builder = builder.ConfigureTcp(s => s.With(connectTimeout: connectionString.ConnectTimeout.Value));
            }
            if (connectionString.HeartbeatInterval.HasValue)
            {
                builder = builder.ConfigureServer(s => s.With(heartbeatInterval: connectionString.HeartbeatInterval.Value));
            }
            if (connectionString.HeartbeatTimeout.HasValue)
            {
                builder = builder.ConfigureServer(s => s.With(heartbeatTimeout: connectionString.HeartbeatTimeout.Value));
            }
            if (connectionString.Ipv6.HasValue && connectionString.Ipv6.Value)
            {
                builder = builder.ConfigureTcp(s => s.With(addressFamily: AddressFamily.InterNetworkV6));
            }

            if (connectionString.SocketTimeout != null)
            {
                builder = builder.ConfigureTcp(s => s.With(
                    readTimeout: connectionString.SocketTimeout.Value,
                    writeTimeout: connectionString.SocketTimeout.Value));
            }

            if (connectionString.Ssl != null)
            {
                builder = builder.ConfigureSsl(ssl =>
                {
                    if (!connectionString.SslVerifyCertificate.GetValueOrDefault(true))
                    {
                        ssl = ssl.With(
                            serverCertificateValidationCallback: new RemoteCertificateValidationCallback(AcceptAnySslCertificate));
                    }

                    return ssl;
                });
            }

            // Connection
            if (connectionString.Username != null)
            {
                var authenticator = CreateAuthenticator(connectionString);
                builder = builder.ConfigureConnection(s => s.With(authenticators: new[] { authenticator }));
            }
            if (connectionString.ApplicationName != null)
            {
                builder = builder.ConfigureConnection(s => s.With(applicationName: connectionString.ApplicationName));
            }
            if (connectionString.MaxIdleTime != null)
            {
                builder = builder.ConfigureConnection(s => s.With(maxIdleTime: connectionString.MaxIdleTime.Value));
            }
            if (connectionString.MaxLifeTime != null)
            {
                builder = builder.ConfigureConnection(s => s.With(maxLifeTime: connectionString.MaxLifeTime.Value));
            }

            // Connection Pool
            if (connectionString.MaxPoolSize != null)
            {
                builder = builder.ConfigureConnectionPool(s => s.With(maxConnections: connectionString.MaxPoolSize.Value));
            }
            if (connectionString.MinPoolSize != null)
            {
                builder = builder.ConfigureConnectionPool(s => s.With(minConnections: connectionString.MinPoolSize.Value));
            }
            if (connectionString.WaitQueueSize != null)
            {
                builder = builder.ConfigureConnectionPool(s => s.With(waitQueueSize: connectionString.WaitQueueSize.Value));
            }
            else if (connectionString.WaitQueueMultiple != null)
            {
                var maxConnections = connectionString.MaxPoolSize ?? new ConnectionPoolSettings().MaxConnections;
                var waitQueueSize = (int)Math.Round(maxConnections * connectionString.WaitQueueMultiple.Value);
                builder = builder.ConfigureConnectionPool(s => s.With(waitQueueSize: waitQueueSize));
            }
            if (connectionString.WaitQueueTimeout != null)
            {
                builder = builder.ConfigureConnectionPool(s => s.With(waitQueueTimeout: connectionString.WaitQueueTimeout.Value));
            }

            // Server

            // Cluster
            if (connectionString.Hosts.Count > 0)
            {
                builder = builder.ConfigureCluster(s => s.With(endPoints: Optional.Enumerable(connectionString.Hosts)));
            }
            if (connectionString.ReplicaSet != null)
            {
                builder = builder.ConfigureCluster(s => s.With(
                    replicaSetName: connectionString.ReplicaSet));
            }
            if (connectionString.ServerSelectionTimeout != null)
            {
                builder = builder.ConfigureCluster(s => s.With(serverSelectionTimeout: connectionString.ServerSelectionTimeout.Value));
            }

            return builder;
        }

        private static bool AcceptAnySslCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors
        )
        {
            return true;
        }

        private static IAuthenticator CreateAuthenticator(ConnectionString connectionString)
        {
            if (connectionString.Password != null)
            {
                var defaultSource = GetDefaultSource(connectionString);

                var credential = new UsernamePasswordCredential(
                        connectionString.AuthSource ?? connectionString.DatabaseName ?? defaultSource,
                        connectionString.Username,
                        connectionString.Password);

                if (connectionString.AuthMechanism == null)
                {
                    return new DefaultAuthenticator(credential);
                }
                else if (connectionString.AuthMechanism == MongoDBCRAuthenticator.MechanismName)
                {
                    return new MongoDBCRAuthenticator(credential);
                }
                else if (connectionString.AuthMechanism == ScramSha1Authenticator.MechanismName)
                {
                    return new ScramSha1Authenticator(credential);
                }
                else if (connectionString.AuthMechanism == PlainAuthenticator.MechanismName)
                {
                    return new PlainAuthenticator(credential);
                }
                else if (connectionString.AuthMechanism == GssapiAuthenticator.MechanismName)
                {
                    return new GssapiAuthenticator(credential, connectionString.AuthMechanismProperties);
                }
            }
            else
            {
                if (connectionString.AuthMechanism == MongoDBX509Authenticator.MechanismName)
                {
                    return new MongoDBX509Authenticator(connectionString.Username);
                }
                else if (connectionString.AuthMechanism == GssapiAuthenticator.MechanismName)
                {
                    return new GssapiAuthenticator(connectionString.Username, connectionString.AuthMechanismProperties);
                }
            }

            throw new NotSupportedException("Unable to create an authenticator.");
        }

        private static string GetDefaultSource(ConnectionString connectionString)
        {
            if (connectionString.AuthMechanism != null && connectionString.AuthMechanism.Equals("GSSAPI", StringComparison.OrdinalIgnoreCase))
            {
                return "$external";
            }

            return "admin";
        }

#if NET45
        /// <summary>
        /// Configures the cluster to write performance counters.
        /// </summary>
        /// <param name="builder">The cluster builder.</param>
        /// <param name="applicationName">The name of the application.</param>
        /// <param name="install">if set to <c>true</c> install the performance counters first.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public static ClusterBuilder UsePerformanceCounters(this ClusterBuilder builder, string applicationName, bool install = false)
        {
            Ensure.IsNotNull(builder, nameof(builder));

            if (install)
            {
                PerformanceCounterEventSubscriber.InstallPerformanceCounters();
            }

            var subscriber = new PerformanceCounterEventSubscriber(applicationName);
            return builder.Subscribe(subscriber);
        }
#endif

        /// <summary>
        /// Configures the cluster to trace events to the specified <paramref name="traceSource"/>.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="traceSource">The trace source.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public static ClusterBuilder TraceWith(this ClusterBuilder builder, TraceSource traceSource)
        {
            Ensure.IsNotNull(builder, nameof(builder));
            Ensure.IsNotNull(traceSource, nameof(traceSource));

            return builder.Subscribe(new TraceSourceEventSubscriber(traceSource));
        }

        /// <summary>
        /// Configures the cluster to trace command events to the specified <paramref name="traceSource"/>.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="traceSource">The trace source.</param>
        /// <returns>A reconfigured cluster builder.</returns>
        public static ClusterBuilder TraceCommandsWith(this ClusterBuilder builder, TraceSource traceSource)
        {
            Ensure.IsNotNull(builder, nameof(builder));
            Ensure.IsNotNull(traceSource, nameof(traceSource));

            return builder.Subscribe(new TraceSourceCommandEventSubscriber(traceSource));
        }
    }
}