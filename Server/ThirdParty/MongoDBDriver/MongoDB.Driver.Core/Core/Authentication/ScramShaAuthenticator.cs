/* Copyright 2018–present MongoDB Inc.
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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Authentication.Vendored;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// A SCRAM-SHA SASL authenticator.
    /// </summary>
    public abstract class ScramShaAuthenticator : SaslAuthenticator
    {
        /// <summary>
        /// An H function as defined in RFC5802.
        /// </summary>
        /// <param name="data">The data to hash. Also called "str" in RFC5802.</param>
        protected internal delegate byte[] H(byte[] data);
        
        /// <summary>
        /// A Hi function used to compute the SaltedPassword as defined in RFC5802, except with "str" parameter replaced
        /// with a UsernamePassword credential so that the password can be optionally digested/prepped in a secure fashion
        /// before being consumed as the "str" parameter would be in RFC5802's Hi.
        /// </summary>
        /// <param name="credentials">The credential to be digested/prepped before being consumed as the "str"
        /// parameter would be in RFC5802's Hi</param>
        /// <param name="salt">The salt.</param>
        /// <param name="iterations">The iteration count.</param>
        protected internal delegate byte[] Hi(UsernamePasswordCredential credentials, byte[] salt, int iterations);
        
        /// <summary>
        /// An HMAC function as defined in RFC5802, plus the encoding of the data.
        /// </summary>
        /// <param name="encoding">The encoding of the data.</param>
        /// <param name="data">The data. Also called "str" in RFC5802.</param>
        /// <param name="key">The key.</param>
        protected internal delegate byte[] Hmac(UTF8Encoding encoding, byte[] data, string key);
        
        // fields
        private readonly string _databaseName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ScramShaAuthenticator"/> class.
        /// </summary>
        /// <param name="credential">The credential.</param>
        /// <param name="hashAlgorithmName">The hash algorithm name.</param>
        /// <param name="h">The H function to use.</param>
        /// <param name="hi">The Hi function to use.</param>
        /// <param name="hmac">The Hmac function to use.</param>
        protected ScramShaAuthenticator(UsernamePasswordCredential credential, 
            HashAlgorithmName hashAlgorithmName,
            H h,
            Hi hi,
            Hmac hmac)
            : this(credential, hashAlgorithmName, new DefaultRandomStringGenerator(), h, hi, hmac) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ScramShaAuthenticator"/> class.
        /// </summary>
        /// <param name="credential">The credential.</param>
        /// <param name="hashAlgorithName">The hash algorithm name.</param>
        /// <param name="randomStringGenerator">The random string generator.</param>
        /// <param name="h">The H function to use.</param>
        /// <param name="hi">The Hi function to use.</param>
        /// <param name="hmac">The Hmac function to use.</param>
        internal ScramShaAuthenticator(
            UsernamePasswordCredential credential, 
            HashAlgorithmName hashAlgorithName,
            IRandomStringGenerator randomStringGenerator,
            H h,
            Hi hi,
            Hmac hmac)
            : base(new ScramShaMechanism(credential, hashAlgorithName, randomStringGenerator, h, hi, hmac))
        {
            _databaseName = credential.Source;
        }

        // properties
        /// <inheritdoc/>
        public override string DatabaseName => _databaseName;

        // nested classes
        private class ScramShaMechanism : ISaslMechanism
        {
            private readonly UsernamePasswordCredential _credential;
            private readonly IRandomStringGenerator _randomStringGenerator;
            private readonly H _h;
            private readonly Hi _hi;
            private readonly Hmac _hmac;
            private readonly string _name;

            public ScramShaMechanism(
                UsernamePasswordCredential credential, 
                HashAlgorithmName hashAlgorithmName, 
                IRandomStringGenerator randomStringGenerator,
                H h,
                Hi hi,
                Hmac hmac)
            {
                _credential = Ensure.IsNotNull(credential, nameof(credential));
                _h = h;
                _hi = hi;
                _hmac = hmac;
                if (!hashAlgorithmName.ToString().StartsWith("SHA"))
                {
                    throw new ArgumentException("Must specify a SHA algorithm.");
                }
                _name = $"SCRAM-SHA-{hashAlgorithmName.ToString().Substring(3)}";
                _randomStringGenerator = Ensure.IsNotNull(randomStringGenerator, nameof(randomStringGenerator));
            }

            public string Name => _name;

            public ISaslStep Initialize(IConnection connection, SaslConversation conversation, ConnectionDescription description)
            {
                Ensure.IsNotNull(connection, nameof(connection));
                Ensure.IsNotNull(description, nameof(description));

                const string gs2Header = "n,,";
                var username = "n=" + PrepUsername(_credential.Username);
                var r = GenerateRandomString();
                var nonce = "r=" + r;

                var clientFirstMessageBare = username + "," + nonce;
                var clientFirstMessage = gs2Header + clientFirstMessageBare;
                var clientFirstMessageBytes = Utf8Encodings.Strict.GetBytes(clientFirstMessage); 

                return new ClientFirst(clientFirstMessageBytes, clientFirstMessageBare, _credential, r, _h, _hi, _hmac);
            }

            private string GenerateRandomString()
            {
                const string legalCharacters = "!\"#$%&'()*+-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

                return _randomStringGenerator.Generate(20, legalCharacters);
            }

            private string PrepUsername(string username)
            {
                return username.Replace("=", "=3D").Replace(",", "=2C");
            }
        }
        
        private class ClientFirst : ISaslStep
        {
            
            private readonly byte[] _bytesToSendToServer;
            private readonly string _clientFirstMessageBare;
            private readonly UsernamePasswordCredential _credential;
           
            private readonly string _rPrefix;
            private readonly H _h;
            private readonly Hi _hi;
            private readonly Hmac _hmac;

            public ClientFirst(
                byte[] bytesToSendToServer, 
                string clientFirstMessageBare, 
                UsernamePasswordCredential credential, 
                string rPrefix,
                H h,
                Hi hi,
                Hmac hmac)
            {
                _bytesToSendToServer = bytesToSendToServer;
                _clientFirstMessageBare = clientFirstMessageBare;
                _credential = credential;
                _h = h;
                _hi = hi;
                _hmac = hmac;
                _rPrefix = rPrefix;
            }

            public byte[] BytesToSendToServer => _bytesToSendToServer;

            public bool IsComplete => false;

            public ISaslStep Transition(SaslConversation conversation, byte[] bytesReceivedFromServer)
            {
                var encoding = Utf8Encodings.Strict;
                var serverFirstMessage = encoding.GetString(bytesReceivedFromServer);
                var map = SaslMapParser.Parse(serverFirstMessage);

                var r = map['r'];
                if (!r.StartsWith(_rPrefix))
                {
                    throw new MongoAuthenticationException(conversation.ConnectionId, message: "Server sent an invalid nonce.");
                }
                var s = map['s'];
                var i = map['i'];

                const string gs2Header = "n,,";
                var channelBinding = "c=" + Convert.ToBase64String(encoding.GetBytes(gs2Header));
                var nonce = "r=" + r;
                var clientFinalMessageWithoutProof = channelBinding + "," + nonce;

                var saltedPassword = _hi(
                    _credential,
                    Convert.FromBase64String(s),
                    int.Parse(i));

                var clientKey = _hmac(encoding, saltedPassword, "Client Key");
                var storedKey = _h(clientKey);
                var authMessage = _clientFirstMessageBare + "," + serverFirstMessage + "," + clientFinalMessageWithoutProof;
                var clientSignature = _hmac(encoding, storedKey, authMessage);
                var clientProof = XOR(clientKey, clientSignature);
                var serverKey = _hmac(encoding, saltedPassword, "Server Key");
                var serverSignature = _hmac(encoding, serverKey, authMessage);

                var proof = "p=" + Convert.ToBase64String(clientProof);
                var clientFinalMessage = clientFinalMessageWithoutProof + "," + proof;

                return new ClientLast(encoding.GetBytes(clientFinalMessage), serverSignature);
            }

            private byte[] XOR(byte[] a, byte[] b)
            {
                var result = new byte[a.Length];
                for (int i = 0; i < a.Length; i++)
                {
                    result[i] = (byte)(a[i] ^ b[i]);
                }

                return result;
            }

        }

        private class ClientLast : ISaslStep
        {            
            private readonly byte[] _bytesToSendToServer;
            private readonly byte[] _serverSignature64;

            public ClientLast(byte[] bytesToSendToServer, byte[] serverSignature64)
            {
                _bytesToSendToServer = bytesToSendToServer;
                _serverSignature64 = serverSignature64;
            }

            public byte[] BytesToSendToServer => _bytesToSendToServer;

            public bool IsComplete => false;

            public ISaslStep Transition(SaslConversation conversation, byte[] bytesReceivedFromServer)
            {
                var encoding = Utf8Encodings.Strict;
                var map = SaslMapParser.Parse(encoding.GetString(bytesReceivedFromServer));
                var serverSignature = Convert.FromBase64String(map['v']);

                if (!ConstantTimeEquals(_serverSignature64, serverSignature))
                {
                    throw new MongoAuthenticationException(conversation.ConnectionId, message: "Server signature was invalid.");
                }

                return new CompletedStep();
            }
            
            private bool ConstantTimeEquals(byte[] a, byte[] b)
            {
                var diff = a.Length ^ b.Length;
                for (var i = 0; i < a.Length && i < b.Length; i++)
                {
                    diff |= a[i] ^ b[i];
                }

                return diff == 0;
            }
        }
    }
}
