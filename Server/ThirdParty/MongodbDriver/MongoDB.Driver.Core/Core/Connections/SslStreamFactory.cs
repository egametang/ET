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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Connections
{
    internal class SslStreamFactory : IStreamFactory
    {
        // fields
        private readonly SslStreamSettings _settings;
        private readonly IStreamFactory _wrapped;

        public SslStreamFactory(SslStreamSettings settings, IStreamFactory wrapped)
        {
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _wrapped = Ensure.IsNotNull(wrapped, nameof(wrapped));
        }

        // public methods
        public Stream CreateStream(EndPoint endPoint, CancellationToken cancellationToken)
        {
            var stream = _wrapped.CreateStream(endPoint, cancellationToken);
            try
            {
                var sslStream = CreateSslStream(stream);
                var targetHost = GetTargetHost(endPoint);
                var clientCertificates = new X509CertificateCollection(_settings.ClientCertificates.ToArray());
#if NETSTANDARD1_5 || NETSTANDARD1_6
                sslStream.AuthenticateAsClientAsync(targetHost, clientCertificates, _settings.EnabledSslProtocols, _settings.CheckCertificateRevocation).GetAwaiter().GetResult();
#else
                sslStream.AuthenticateAsClient(targetHost, clientCertificates, _settings.EnabledSslProtocols, _settings.CheckCertificateRevocation);
#endif       
                return sslStream;
            }
            catch
            {
                DisposeStreamIgnoringExceptions(stream);
                throw;
            }
        }

        public async Task<Stream> CreateStreamAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            var stream = await _wrapped.CreateStreamAsync(endPoint, cancellationToken).ConfigureAwait(false);
            try
            {
                var sslStream = CreateSslStream(stream);
                var targetHost = GetTargetHost(endPoint);
                var clientCertificates = new X509CertificateCollection(_settings.ClientCertificates.ToArray());
                await sslStream.AuthenticateAsClientAsync(targetHost, clientCertificates, _settings.EnabledSslProtocols, _settings.CheckCertificateRevocation).ConfigureAwait(false);
                return sslStream;
            }
            catch
            {
                DisposeStreamIgnoringExceptions(stream);
                throw;
            }
        }

        // private methods
        private SslStream CreateSslStream(Stream stream)
        {
            return new SslStream(
                stream,
                leaveInnerStreamOpen: false,
                userCertificateValidationCallback: _settings.ServerCertificateValidationCallback,
                userCertificateSelectionCallback: _settings.ClientCertificateSelectionCallback);
        }

        private void DisposeStreamIgnoringExceptions(Stream stream)
        {
            try
            {
                stream.Dispose();
            }
            catch
            {
                // ignore exception
            }
        }

        private string GetTargetHost(EndPoint endPoint)
        {
            DnsEndPoint dnsEndPoint;
            if ((dnsEndPoint = endPoint as DnsEndPoint) != null)
            {
                return dnsEndPoint.Host;
            }

            IPEndPoint ipEndPoint;
            if ((ipEndPoint = endPoint as IPEndPoint) != null)
            {
                return ipEndPoint.Address.ToString();
            }

            return endPoint.ToString();
        }
    }
}
