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

using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// A PLAIN SASL authenticator.
    /// </summary>
    public sealed class PlainAuthenticator : SaslAuthenticator
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
            get { return "PLAIN"; }
        }

        // fields
        private readonly string _databaseName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PlainAuthenticator"/> class.
        /// </summary>
        /// <param name="credential">The credential.</param>
        public PlainAuthenticator(UsernamePasswordCredential credential)
            : base(new PlainMechanism(credential))
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
        private class PlainMechanism : ISaslMechanism
        {
            // fields
            private readonly UsernamePasswordCredential _credential;

            // constructors
            public PlainMechanism(UsernamePasswordCredential credential)
            {
                _credential = Ensure.IsNotNull(credential, nameof(credential));
            }

            // properties
            public string Name
            {
                get { return MechanismName; }
            }

            // methods
            public ISaslStep Initialize(IConnection connection, ConnectionDescription description)
            {
                Ensure.IsNotNull(connection, nameof(connection));
                Ensure.IsNotNull(description, nameof(description));

                var dataString = string.Format("\0{0}\0{1}",
                    _credential.Username,
                    _credential.GetInsecurePassword());

                var bytes = Utf8Encodings.Strict.GetBytes(dataString);
                return new CompletedStep(bytes);
            }
        }
    }
}