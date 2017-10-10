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

using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents settings for an SSL stream.
    /// </summary>
    public class SslStreamSettings
    {
        // fields
        private readonly bool _checkCertificateRevocation;
        private readonly IEnumerable<X509Certificate> _clientCertificates;
        private readonly LocalCertificateSelectionCallback _clientCertificateSelectionCallback;
        private readonly SslProtocols _enabledSslProtocols;
        private readonly RemoteCertificateValidationCallback _serverCertificateValidationCallback;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SslStreamSettings"/> class.
        /// </summary>
        /// <param name="checkCertificateRevocation">Whether to check for certificate revocation.</param>
        /// <param name="clientCertificates">The client certificates.</param>
        /// <param name="clientCertificateSelectionCallback">The client certificate selection callback.</param>
        /// <param name="enabledProtocols">The enabled protocols.</param>
        /// <param name="serverCertificateValidationCallback">The server certificate validation callback.</param>
        public SslStreamSettings(
            Optional<bool> checkCertificateRevocation = default(Optional<bool>),
            Optional<IEnumerable<X509Certificate>> clientCertificates = default(Optional<IEnumerable<X509Certificate>>),
            Optional<LocalCertificateSelectionCallback> clientCertificateSelectionCallback = default(Optional<LocalCertificateSelectionCallback>),
            Optional<SslProtocols> enabledProtocols = default(Optional<SslProtocols>),
            Optional<RemoteCertificateValidationCallback> serverCertificateValidationCallback = default(Optional<RemoteCertificateValidationCallback>))
        {
            _checkCertificateRevocation = checkCertificateRevocation.WithDefault(true);
            _clientCertificates = Ensure.IsNotNull(clientCertificates.WithDefault(Enumerable.Empty<X509Certificate>()), "clientCertificates").ToList();
            _clientCertificateSelectionCallback = clientCertificateSelectionCallback.WithDefault(null);
            _enabledSslProtocols = enabledProtocols.WithDefault(SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls);
            _serverCertificateValidationCallback = serverCertificateValidationCallback.WithDefault(null);
        }

        // properties
        /// <summary>
        /// Gets a value indicating whether to check for certificate revocation.
        /// </summary>
        /// <value>
        /// <c>true</c> if certificate should be checked for revocation; otherwise, <c>false</c>.
        /// </value>
        public bool CheckCertificateRevocation
        {
            get { return _checkCertificateRevocation; }
        }

        /// <summary>
        /// Gets the client certificates.
        /// </summary>
        /// <value>
        /// The client certificates.
        /// </value>
        public IEnumerable<X509Certificate> ClientCertificates
        {
            get { return _clientCertificates; }
        }

        /// <summary>
        /// Gets the client certificate selection callback.
        /// </summary>
        /// <value>
        /// The client certificate selection callback.
        /// </value>
        public LocalCertificateSelectionCallback ClientCertificateSelectionCallback
        {
            get { return _clientCertificateSelectionCallback; }
        }

        /// <summary>
        /// Gets the enabled SSL protocols.
        /// </summary>
        /// <value>
        /// The enabled SSL protocols.
        /// </value>
        public SslProtocols EnabledSslProtocols
        {
            get { return _enabledSslProtocols; }
        }

        /// <summary>
        /// Gets the server certificate validation callback.
        /// </summary>
        /// <value>
        /// The server certificate validation callback.
        /// </value>
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get { return _serverCertificateValidationCallback; }
        }

        // methods
        /// <summary>
        /// Returns a new SsslStreamSettings instance with some settings changed.
        /// </summary>
        /// <param name="checkCertificateRevocation">Whether to check certificate revocation.</param>
        /// <param name="clientCertificates">The client certificates.</param>
        /// <param name="clientCertificateSelectionCallback">The client certificate selection callback.</param>
        /// <param name="enabledProtocols">The enabled protocols.</param>
        /// <param name="serverCertificateValidationCallback">The server certificate validation callback.</param>
        /// <returns>A new SsslStreamSettings instance.</returns>
        public SslStreamSettings With(
            Optional<bool> checkCertificateRevocation = default(Optional<bool>),
            Optional<IEnumerable<X509Certificate>> clientCertificates = default(Optional<IEnumerable<X509Certificate>>),
            Optional<LocalCertificateSelectionCallback> clientCertificateSelectionCallback = default(Optional<LocalCertificateSelectionCallback>),
            Optional<SslProtocols> enabledProtocols = default(Optional<SslProtocols>),
            Optional<RemoteCertificateValidationCallback> serverCertificateValidationCallback = default(Optional<RemoteCertificateValidationCallback>))
        {
            return new SslStreamSettings(
                checkCertificateRevocation: checkCertificateRevocation.WithDefault(_checkCertificateRevocation),
                clientCertificates: Optional.Enumerable(clientCertificates.WithDefault(_clientCertificates)),
                clientCertificateSelectionCallback: clientCertificateSelectionCallback.WithDefault(_clientCertificateSelectionCallback),
                enabledProtocols: enabledProtocols.WithDefault(_enabledSslProtocols),
                serverCertificateValidationCallback: serverCertificateValidationCallback.WithDefault(_serverCertificateValidationCallback));
        }
    }
}