/* Copyright 2015 MongoDB Inc.
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
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// A SCRAM-SHA1 SASL authenticator.
    /// </summary>
    public sealed class ScramSha1Authenticator : SaslAuthenticator
    {
        // static properties
        /// <summary>
        /// Gets the name of the mechanism.
        /// </summary>
        /// <value>
        /// The name of the mechanism.
        /// </value>
        public static string MechanismName
        {
            get { return "SCRAM-SHA-1"; }
        }

        // fields
        private readonly string _databaseName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ScramSha1Authenticator"/> class.
        /// </summary>
        /// <param name="credential">The credential.</param>
        public ScramSha1Authenticator(UsernamePasswordCredential credential)
            : this(credential, new DefaultRandomStringGenerator())
        {
        }

        internal ScramSha1Authenticator(UsernamePasswordCredential credential, IRandomStringGenerator randomStringGenerator)
            : base(new ScramSha1Mechanism(credential, randomStringGenerator))
        {
            _databaseName = credential.Source;
        }

        // properties
        /// <inheritdoc/>
        public override string DatabaseName
        {
            get { return _databaseName; }
        }

        // nested classes
        private class ScramSha1Mechanism : ISaslMechanism
        {
            private readonly UsernamePasswordCredential _credential;
            private readonly IRandomStringGenerator _randomStringGenerator;

            public ScramSha1Mechanism(UsernamePasswordCredential credential, IRandomStringGenerator randomStringGenerator)
            {
                _credential = Ensure.IsNotNull(credential, nameof(credential));
                _randomStringGenerator = Ensure.IsNotNull(randomStringGenerator, nameof(randomStringGenerator));
            }

            public string Name
            {
                get { return MechanismName; }
            }

            public ISaslStep Initialize(IConnection connection, ConnectionDescription description)
            {
                Ensure.IsNotNull(connection, nameof(connection));
                Ensure.IsNotNull(description, nameof(description));

                const string gs2Header = "n,,";
                var username = "n=" + PrepUsername(_credential.Username);
                var r = GenerateRandomString();
                var nonce = "r=" + r;

                var clientFirstMessageBare = username + "," + nonce;
                var clientFirstMessage = gs2Header + clientFirstMessageBare;

                return new ClientFirst(
                    Utf8Encodings.Strict.GetBytes(clientFirstMessage),
                    clientFirstMessageBare,
                    _credential,
                    r);
            }

            private string GenerateRandomString()
            {
                const string legalCharacters = "!\"#$%&'()*+-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

                return _randomStringGenerator.Generate(20, legalCharacters);
            }

            private static string PrepUsername(string username)
            {
                return username.Replace("=", "=3D").Replace(",", "=2C");
            }
        }

        private class ClientFirst : ISaslStep
        {
            private readonly byte[] _bytesToSendToServer;
            private string _clientFirstMessageBare;
            private UsernamePasswordCredential _credential;
            private readonly string _rPrefix;

            public ClientFirst(byte[] bytesToSendToServer, string clientFirstMessageBare, UsernamePasswordCredential credential, string rPrefix)
            {
                _bytesToSendToServer = bytesToSendToServer;
                _clientFirstMessageBare = clientFirstMessageBare;
                _credential = credential;
                _rPrefix = rPrefix;
            }

            public byte[] BytesToSendToServer
            {
                get { return _bytesToSendToServer; }
            }

            public bool IsComplete
            {
                get { return false; }
            }

            public ISaslStep Transition(SaslConversation conversation, byte[] bytesReceivedFromServer)
            {
                var encoding = Utf8Encodings.Strict;
                var serverFirstMessage = encoding.GetString(bytesReceivedFromServer);
                var map = NVParser.Parse(serverFirstMessage);

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

                var saltedPassword = Hi(
                    AuthenticationHelper.MongoPasswordDigest(_credential.Username, _credential.Password),
                    Convert.FromBase64String(s),
                    int.Parse(i));

                var clientKey = HMAC(encoding, saltedPassword, "Client Key");
                var storedKey = H(clientKey);
                var authMessage = _clientFirstMessageBare + "," + serverFirstMessage + "," + clientFinalMessageWithoutProof;
                var clientSignature = HMAC(encoding, storedKey, authMessage);
                var clientProof = XOR(clientKey, clientSignature);
                var serverKey = HMAC(encoding, saltedPassword, "Server Key");
                var serverSignature = HMAC(encoding, serverKey, authMessage);

                var proof = "p=" + Convert.ToBase64String(clientProof);
                var clientFinalMessage = clientFinalMessageWithoutProof + "," + proof;

                return new ClientLast(encoding.GetBytes(clientFinalMessage), serverSignature);
            }

            private static byte[] XOR(byte[] a, byte[] b)
            {
                var result = new byte[a.Length];
                for (int i = 0; i < a.Length; i++)
                {
                    result[i] = (byte)(a[i] ^ b[i]);
                }

                return result;
            }

            private static byte[] H(byte[] data)
            {
                using (var sha1 = SHA1.Create())
                {
                    return sha1.ComputeHash(data);
                }
            }

            private static byte[] Hi(string password, byte[] salt, int iterations)
            {
                return new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iterations).GetBytes(20); // this is length of output of a sha-1 hmac
            }

            private static byte[] HMAC(UTF8Encoding encoding, byte[] data, string key)
            {
#if NET45
                using (var hmac = new HMACSHA1(data, useManagedSha1: true))
#else
                using (var hmac = new HMACSHA1(data))
#endif
                {
                    return hmac.ComputeHash(encoding.GetBytes(key));
                }
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

            public byte[] BytesToSendToServer
            {
                get { return _bytesToSendToServer; }
            }

            public bool IsComplete
            {
                get { return false; }
            }

            public ISaslStep Transition(SaslConversation conversation, byte[] bytesReceivedFromServer)
            {
                var encoding = Utf8Encodings.Strict;
                var map = NVParser.Parse(encoding.GetString(bytesReceivedFromServer));
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

        private class NVParser
        {
            private const int EOF = -1;

            public static IDictionary<char, string> Parse(string text)
            {
                IDictionary<char, string> dict = new Dictionary<char, string>();

                using (var reader = new StringReader(text))
                {
                    while (reader.Peek() != EOF)
                    {
                        dict.Add(ReadKeyValue(reader));
                        if (reader.Peek() == ',')
                        {
                            Read(reader, ',');
                        }
                    }
                }

                return dict;
            }

            private static KeyValuePair<char, string> ReadKeyValue(TextReader reader)
            {
                var key = ReadKey(reader);
                Read(reader, '=');
                var value = ReadValue(reader);
                return new KeyValuePair<char, string>(key, value);
            }

            private static char ReadKey(TextReader reader)
            {
                // keys are of length 1.
                return (char)reader.Read();
            }

            private static void Read(TextReader reader, char expected)
            {
                var ch = (char)reader.Read();
                if (ch != expected)
                {
                    throw new IOException(string.Format("Expected {0} but found {1}.", expected, ch));
                }
            }

            private static string ReadValue(TextReader reader)
            {
                var sb = new StringBuilder();
                var ch = reader.Peek();
                while (ch != ',' && ch != EOF)
                {
                    sb.Append((char)reader.Read());
                    ch = reader.Peek();
                }

                return sb.ToString();
            }
        }
    }
}