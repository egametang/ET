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
using System.Linq;
using MongoDB.Bson;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents an immutable URL style connection string. See also MongoUrlBuilder.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoUrl : IEquatable<MongoUrl>
    {
        // private static fields
        private static object __staticLock = new object();
        private static Dictionary<string, MongoUrl> __cache = new Dictionary<string, MongoUrl>();

        // private fields
        private readonly string _applicationName;
        private readonly string _authenticationMechanism;
        private readonly IEnumerable<KeyValuePair<string, string>> _authenticationMechanismProperties;
        private readonly string _authenticationSource;
        private readonly ConnectionMode _connectionMode;
        private readonly TimeSpan _connectTimeout;
        private readonly string _databaseName;
        private readonly bool? _fsync;
        private readonly GuidRepresentation _guidRepresentation;
        private readonly TimeSpan _heartbeatInterval;
        private readonly TimeSpan _heartbeatTimeout;
        private readonly bool _ipv6;
        private readonly bool? _journal;
        private readonly TimeSpan _maxConnectionIdleTime;
        private readonly TimeSpan _maxConnectionLifeTime;
        private readonly int _maxConnectionPoolSize;
        private readonly int _minConnectionPoolSize;
        private readonly string _password;
        private readonly ReadConcernLevel? _readConcernLevel;
        private readonly ReadPreference _readPreference;
        private readonly string _replicaSetName;
        private readonly TimeSpan _localThreshold;
        private readonly IEnumerable<MongoServerAddress> _servers;
        private readonly TimeSpan _serverSelectionTimeout;
        private readonly TimeSpan _socketTimeout;
        private readonly string _username;
        private readonly bool _useSsl;
        private readonly bool _verifySslCertificate;
        private readonly WriteConcern.WValue _w;
        private readonly double _waitQueueMultiple;
        private readonly int _waitQueueSize;
        private readonly TimeSpan _waitQueueTimeout;
        private readonly TimeSpan? _wTimeout;
        private readonly string _url;

        // constructors
        /// <summary>
        /// Creates a new instance of MongoUrl.
        /// </summary>
        /// <param name="url">The URL containing the settings.</param>
        public MongoUrl(string url)
        {
            var builder = new MongoUrlBuilder(url); // parses url
            _applicationName = builder.ApplicationName;
            _authenticationMechanism = builder.AuthenticationMechanism;
            _authenticationMechanismProperties = builder.AuthenticationMechanismProperties;
            _authenticationSource = builder.AuthenticationSource;
            _connectionMode = builder.ConnectionMode;
            _connectTimeout = builder.ConnectTimeout;
            _databaseName = builder.DatabaseName;
            _fsync = builder.FSync;
            _guidRepresentation = builder.GuidRepresentation;
            _heartbeatInterval = builder.HeartbeatInterval;
            _heartbeatTimeout = builder.HeartbeatTimeout;
            _ipv6 = builder.IPv6;
            _journal = builder.Journal;
            _localThreshold = builder.LocalThreshold;
            _maxConnectionIdleTime = builder.MaxConnectionIdleTime;
            _maxConnectionLifeTime = builder.MaxConnectionLifeTime;
            _maxConnectionPoolSize = builder.MaxConnectionPoolSize;
            _minConnectionPoolSize = builder.MinConnectionPoolSize;
            _password = builder.Password;
            _readConcernLevel = builder.ReadConcernLevel;
            _readPreference = builder.ReadPreference;
            _replicaSetName = builder.ReplicaSetName;
            _servers = builder.Servers;
            _serverSelectionTimeout = builder.ServerSelectionTimeout;
            _socketTimeout = builder.SocketTimeout;
            _username = builder.Username;
            _useSsl = builder.UseSsl;
            _verifySslCertificate = builder.VerifySslCertificate;
            _w = builder.W;
            _waitQueueMultiple = builder.WaitQueueMultiple;
            _waitQueueSize = builder.WaitQueueSize;
            _waitQueueTimeout = builder.WaitQueueTimeout;
            _wTimeout = builder.WTimeout;
            _url = builder.ToString(); // keep canonical form
        }

        // public properties
        /// <summary>
        /// Gets the application name.
        /// </summary>
        public string ApplicationName
        {
            get { return _applicationName; }
        }

        /// <summary>
        /// Gets the authentication mechanism.
        /// </summary>
        public string AuthenticationMechanism
        {
            get { return _authenticationMechanism; }
        }

        /// <summary>
        /// Gets the authentication mechanism properties.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> AuthenticationMechanismProperties
        {
            get { return _authenticationMechanismProperties; }
        }

        /// <summary>
        /// Gets the authentication source.
        /// </summary>
        public string AuthenticationSource
        {
            get { return _authenticationSource; }
        }

        /// <summary>
        /// Gets the actual wait queue size (either WaitQueueSize or WaitQueueMultiple x MaxConnectionPoolSize).
        /// </summary>
        public int ComputedWaitQueueSize
        {
            get
            {
                if (_waitQueueMultiple == 0.0)
                {
                    return _waitQueueSize;
                }
                else
                {
                    return (int)(_waitQueueMultiple * _maxConnectionPoolSize);
                }
            }
        }

        /// <summary>
        /// Gets the connection mode.
        /// </summary>
        public ConnectionMode ConnectionMode
        {
            get { return _connectionMode; }
        }

        /// <summary>
        /// Gets the connect timeout.
        /// </summary>
        public TimeSpan ConnectTimeout
        {
            get { return _connectTimeout; }
        }

        /// <summary>
        /// Gets the optional database name.
        /// </summary>
        public string DatabaseName
        {
            get { return _databaseName; }
        }

        /// <summary>
        /// Gets the FSync component of the write concern.
        /// </summary>
        public bool? FSync
        {
            get { return _fsync; }
        }

        /// <summary>
        /// Gets the representation to use for Guids.
        /// </summary>
        public GuidRepresentation GuidRepresentation
        {
            get { return _guidRepresentation; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has authentication settings.
        /// </summary>
        public bool HasAuthenticationSettings
        {
            get
            {
                return
                    _username != null ||
                    _password != null ||
                    _authenticationMechanism != null ||
                    _authenticationSource != null;
            }              
        }

        /// <summary>
        /// Gets the heartbeat interval.
        /// </summary>
        public TimeSpan HeartbeatInterval
        {
            get { return _heartbeatInterval; }
        }

        /// <summary>
        /// Gets the heartbeat timeout.
        /// </summary>
        public TimeSpan HeartbeatTimeout
        {
            get { return _heartbeatTimeout; }
        }

        /// <summary>
        /// Gets a value indicating whether to use IPv6.
        /// </summary>
        public bool IPv6
        {
            get { return _ipv6; }
        }

        /// <summary>
        /// Gets the Journal component of the write concern.
        /// </summary>
        public bool? Journal
        {
            get { return _journal; }
        }

        /// <summary>
        /// Gets the local threshold.
        /// </summary>
        public TimeSpan LocalThreshold
        {
            get { return _localThreshold; }
        }

        /// <summary>
        /// Gets the max connection idle time.
        /// </summary>
        public TimeSpan MaxConnectionIdleTime
        {
            get { return _maxConnectionIdleTime; }
        }

        /// <summary>
        /// Gets the max connection life time.
        /// </summary>
        public TimeSpan MaxConnectionLifeTime
        {
            get { return _maxConnectionLifeTime; }
        }

        /// <summary>
        /// Gets the max connection pool size.
        /// </summary>
        public int MaxConnectionPoolSize
        {
            get { return _maxConnectionPoolSize; }
        }

        /// <summary>
        /// Gets the min connection pool size.
        /// </summary>
        public int MinConnectionPoolSize
        {
            get { return _minConnectionPoolSize; }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password
        {
            get { return _password; }
        }

        /// <summary>
        /// Gets the read concern level.
        /// </summary>
        public ReadConcernLevel? ReadConcernLevel
        {
            get { return _readConcernLevel; }
        }

        /// <summary>
        /// Gets the read preference.
        /// </summary>
        public ReadPreference ReadPreference
        {
            get { return _readPreference; }
        }

        /// <summary>
        /// Gets the name of the replica set.
        /// </summary>
        public string ReplicaSetName
        {
            get { return _replicaSetName; }
        }

        /// <summary>
        /// Gets the address of the server (see also Servers if using more than one address).
        /// </summary>
        public MongoServerAddress Server
        {
            get { return (_servers == null) ? null : _servers.Single(); }
        }

        /// <summary>
        /// Gets the list of server addresses (see also Server if using only one address).
        /// </summary>
        public IEnumerable<MongoServerAddress> Servers
        {
            get { return _servers; }
        }

        /// <summary>
        /// Gets the server selection timeout.
        /// </summary>
        public TimeSpan ServerSelectionTimeout
        {
            get { return _serverSelectionTimeout; }
        }

        /// <summary>
        /// Gets the socket timeout.
        /// </summary>
        public TimeSpan SocketTimeout
        {
            get { return _socketTimeout; }
        }

        /// <summary>
        /// Gets the URL (in canonical form).
        /// </summary>
        public string Url
        {
            get { return _url; }
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username
        {
            get { return _username; }
        }

        /// <summary>
        /// Gets a value indicating whether to use SSL.
        /// </summary>
        public bool UseSsl
        {
            get { return _useSsl; }
        }

        /// <summary>
        /// Gets a value indicating whether to verify an SSL certificate.
        /// </summary>
        public bool VerifySslCertificate
        {
            get { return _verifySslCertificate; }
        }

        /// <summary>
        /// Gets the W component of the write concern.
        /// </summary>
        public WriteConcern.WValue W
        {
            get { return _w; }
        }

        /// <summary>
        /// Gets the wait queue multiple (the actual wait queue size will be WaitQueueMultiple x MaxConnectionPoolSize).
        /// </summary>
        public double WaitQueueMultiple
        {
            get { return _waitQueueMultiple; }
        }

        /// <summary>
        /// Gets the wait queue size.
        /// </summary>
        public int WaitQueueSize
        {
            get { return _waitQueueSize; }
        }

        /// <summary>
        /// Gets the wait queue timeout.
        /// </summary>
        public TimeSpan WaitQueueTimeout
        {
            get { return _waitQueueTimeout; }
        }

        /// <summary>
        /// Gets the WTimeout component of the write concern.
        /// </summary>
        public TimeSpan? WTimeout
        {
            get { return _wTimeout; }
        }

        // public operators
        /// <summary>
        /// Compares two MongoUrls.
        /// </summary>
        /// <param name="lhs">The first URL.</param>
        /// <param name="rhs">The other URL.</param>
        /// <returns>True if the two URLs are equal (or both null).</returns>
        public static bool operator ==(MongoUrl lhs, MongoUrl rhs)
        {
            return object.Equals(lhs, rhs);
        }

        /// <summary>
        /// Compares two MongoUrls.
        /// </summary>
        /// <param name="lhs">The first URL.</param>
        /// <param name="rhs">The other URL.</param>
        /// <returns>True if the two URLs are not equal (or one is null and the other is not).</returns>
        public static bool operator !=(MongoUrl lhs, MongoUrl rhs)
        {
            return !(lhs == rhs);
        }

        // public static methods
        /// <summary>
        /// Clears the URL cache. When a URL is parsed it is stored in the cache so that it doesn't have to be
        /// parsed again. There is rarely a need to call this method.
        /// </summary>
        public static void ClearCache()
        {
            __cache.Clear();
        }

        /// <summary>
        /// Creates an instance of MongoUrl (might be an existing existence if the same URL has been used before).
        /// </summary>
        /// <param name="url">The URL containing the settings.</param>
        /// <returns>An instance of MongoUrl.</returns>
        public static MongoUrl Create(string url)
        {
            // cache previously seen urls to avoid repeated parsing
            lock (__staticLock)
            {
                MongoUrl mongoUrl;
                if (!__cache.TryGetValue(url, out mongoUrl))
                {
                    mongoUrl = new MongoUrl(url);
                    var canonicalUrl = mongoUrl.ToString();
                    if (canonicalUrl != url)
                    {
                        if (__cache.ContainsKey(canonicalUrl))
                        {
                            mongoUrl = __cache[canonicalUrl]; // use existing MongoUrl
                        }
                        else
                        {
                            __cache[canonicalUrl] = mongoUrl; // cache under canonicalUrl also
                        }
                    }
                    __cache[url] = mongoUrl;
                }
                return mongoUrl;
            }
        }

        // public methods
        /// <summary>
        /// Compares two MongoUrls.
        /// </summary>
        /// <param name="rhs">The other URL.</param>
        /// <returns>True if the two URLs are equal.</returns>
        public bool Equals(MongoUrl rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _url == rhs._url; // this works because URL is in canonical form
        }

        /// <summary>
        /// Compares two MongoUrls.
        /// </summary>
        /// <param name="obj">The other URL.</param>
        /// <returns>True if the two URLs are equal.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MongoUrl); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the credential.
        /// </summary>
        /// <returns>The credential (or null if the URL has not authentication settings).</returns>
        public MongoCredential GetCredential()
        {
            if (HasAuthenticationSettings)
            {
                return MongoCredential.FromComponents(
                    _authenticationMechanism,
                    _authenticationSource ?? _databaseName,
                    _username,
                    _password);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return _url.GetHashCode(); // this works because URL is in canonical form
        }

        /// <summary>
        /// Returns a WriteConcern value based on this instance's settings and a default enabled value.
        /// </summary>
        /// <param name="enabledDefault">The default enabled value.</param>
        /// <returns>A WriteConcern.</returns>
        public WriteConcern GetWriteConcern(bool enabledDefault)
        {
            if (_w == null && !_wTimeout.HasValue && !_fsync.HasValue && !_journal.HasValue)
            {
                return enabledDefault ? WriteConcern.Acknowledged : WriteConcern.Unacknowledged;
            }

            return new WriteConcern(_w, _wTimeout, _fsync, _journal);
        }

        /// <summary>
        /// Returns the canonical URL based on the settings in this MongoUrlBuilder.
        /// </summary>
        /// <returns>The canonical URL.</returns>
        public override string ToString()
        {
            return _url;
        }

        // private methods
        private bool AnyWriteConcernSettingsAreSet()
        {
            return _fsync != null || _journal != null || _w != null || _wTimeout != null;
        }
    }
}
