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

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents settings for a connection.
    /// </summary>
    public class ConnectionSettings
    {
        #region static
        // static fields
        private static readonly IReadOnlyList<IAuthenticator> __noAuthenticators = new IAuthenticator[0];
        #endregion

        // fields
        private readonly string _applicationName;
        private readonly IReadOnlyList<IAuthenticator> _authenticators;
        private readonly TimeSpan _maxIdleTime;
        private readonly TimeSpan _maxLifeTime;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionSettings" /> class.
        /// </summary>
        /// <param name="authenticators">The authenticators.</param>
        /// <param name="maxIdleTime">The maximum idle time.</param>
        /// <param name="maxLifeTime">The maximum life time.</param>
        /// <param name="applicationName">The application name.</param>
        public ConnectionSettings(
            Optional<IEnumerable<IAuthenticator>> authenticators = default(Optional<IEnumerable<IAuthenticator>>),
            Optional<TimeSpan> maxIdleTime = default(Optional<TimeSpan>),
            Optional<TimeSpan> maxLifeTime = default(Optional<TimeSpan>),
            Optional<string> applicationName = default(Optional<string>))
        {
            _authenticators = Ensure.IsNotNull(authenticators.WithDefault(__noAuthenticators), "authenticators").ToList();
            _maxIdleTime = Ensure.IsGreaterThanZero(maxIdleTime.WithDefault(TimeSpan.FromMinutes(10)), "maxIdleTime");
            _maxLifeTime = Ensure.IsGreaterThanZero(maxLifeTime.WithDefault(TimeSpan.FromMinutes(30)), "maxLifeTime");
            _applicationName = ApplicationNameHelper.EnsureApplicationNameIsValid(applicationName.WithDefault(null), nameof(applicationName));
        }

        // properties
        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName
        {
            get { return _applicationName; }
        }

        /// <summary>
        /// Gets the authenticators.
        /// </summary>
        /// <value>
        /// The authenticators.
        /// </value>
        public IReadOnlyList<IAuthenticator> Authenticators
        {
            get { return _authenticators; }
        }

        /// <summary>
        /// Gets the maximum idle time.
        /// </summary>
        /// <value>
        /// The maximum idle time.
        /// </value>
        public TimeSpan MaxIdleTime
        {
            get { return _maxIdleTime; }
        }

        /// <summary>
        /// Gets the maximum life time.
        /// </summary>
        /// <value>
        /// The maximum life time.
        /// </value>
        public TimeSpan MaxLifeTime
        {
            get { return _maxLifeTime; }
        }

        // methods
        /// <summary>
        /// Returns a new ConnectionSettings instance with some settings changed.
        /// </summary>
        /// <param name="authenticators">The authenticators.</param>
        /// <param name="maxIdleTime">The maximum idle time.</param>
        /// <param name="maxLifeTime">The maximum life time.</param>
        /// <param name="applicationName">The application name.</param>
        /// <returns>A new ConnectionSettings instance.</returns>
        public ConnectionSettings With(
            Optional<IEnumerable<IAuthenticator>> authenticators = default(Optional<IEnumerable<IAuthenticator>>),
            Optional<TimeSpan> maxIdleTime = default(Optional<TimeSpan>),
            Optional<TimeSpan> maxLifeTime = default(Optional<TimeSpan>),
            Optional<string> applicationName = default(Optional<string>))
        {
            return new ConnectionSettings(
                authenticators: Optional.Enumerable(authenticators.WithDefault(_authenticators)),
                maxIdleTime: maxIdleTime.WithDefault(_maxIdleTime),
                maxLifeTime: maxLifeTime.WithDefault(_maxLifeTime),
                applicationName: applicationName.WithDefault(_applicationName));
        }
    }
}
