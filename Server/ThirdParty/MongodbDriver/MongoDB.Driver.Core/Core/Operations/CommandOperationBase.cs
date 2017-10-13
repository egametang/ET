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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents the base class for a command operation.
    /// </summary>
    /// <typeparam name="TCommandResult">The type of the command result.</typeparam>
    public abstract class CommandOperationBase<TCommandResult>
    {
        // fields
        private BsonDocument _additionalOptions;
        private BsonDocument _command;
        private IElementNameValidator _commandValidator = NoOpElementNameValidator.Instance;
        private string _comment;
        private DatabaseNamespace _databaseNamespace;
        private MessageEncoderSettings _messageEncoderSettings;
        private IBsonSerializer<TCommandResult> _resultSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandOperationBase{TCommandResult}"/> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="command">The command.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        protected CommandOperationBase(
            DatabaseNamespace databaseNamespace,
            BsonDocument command,
            IBsonSerializer<TCommandResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _command = Ensure.IsNotNull(command, nameof(command));
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        /// <summary>
        /// Gets or sets the additional options.
        /// </summary>
        /// <value>
        /// The additional options.
        /// </value>
        public BsonDocument AdditionalOptions
        {
            get { return _additionalOptions; }
            set { _additionalOptions = value; }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public BsonDocument Command
        {
            get { return _command; }
        }

        /// <summary>
        /// Gets or sets the command validator.
        /// </summary>
        /// <value>
        /// The command validator.
        /// </value>
        public IElementNameValidator CommandValidator
        {
            get { return _commandValidator; }
            set { _commandValidator = Ensure.IsNotNull(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        public DatabaseNamespace DatabaseNamespace
        {
            get { return _databaseNamespace; }
        }

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        /// <value>
        /// The result serializer.
        /// </value>
        public IBsonSerializer<TCommandResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        // methods
        private BsonDocument CreateWrappedCommand(ServerDescription serverDescription, ReadPreference readPreference)
        {
            BsonDocument readPreferenceDocument = null;
            if (serverDescription.Type == ServerType.ShardRouter)
            {
                readPreferenceDocument = QueryHelper.CreateReadPreferenceDocument(serverDescription.Type, readPreference);
            }

            var wrappedCommand = new BsonDocument
            {
                { "$query", _command },
                { "$readPreference", readPreferenceDocument, readPreferenceDocument != null },
                { "$comment", () => _comment, _comment != null }
            };
            if (_additionalOptions != null)
            {
                wrappedCommand.Merge(_additionalOptions, overwriteExistingElements: false);
            }

            if (wrappedCommand.ElementCount == 1)
            {
                return _command;
            }
            else
            {
                return wrappedCommand;
            }
        }

        private TCommandResult ExecuteProtocol(IChannelHandle channel, ServerDescription serverDescription, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            var wrappedCommand = CreateWrappedCommand(serverDescription, readPreference);
            var slaveOk = readPreference != null && readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary;

            return channel.Command<TCommandResult>(
                _databaseNamespace,
                wrappedCommand,
                _commandValidator,
                () => CommandResponseHandling.Return,
                slaveOk,
                _resultSerializer,
                _messageEncoderSettings,
                cancellationToken);
        }

        /// <summary>
        /// Executes the protocol.
        /// </summary>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the command result.</returns>
        protected TCommandResult ExecuteProtocol(
            IChannelSource channelSource,
            ReadPreference readPreference,
            CancellationToken cancellationToken)
        {
            using (var channel = channelSource.GetChannel(cancellationToken))
            {
                return ExecuteProtocol(channel, channelSource.ServerDescription, readPreference, cancellationToken);
            }
        }

        private Task<TCommandResult> ExecuteProtocolAsync(IChannelHandle channel, ServerDescription serverDescription, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            var wrappedCommand = CreateWrappedCommand(serverDescription, readPreference);
            var slaveOk = readPreference != null && readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary;

            return channel.CommandAsync<TCommandResult>(
                _databaseNamespace,
                wrappedCommand,
                _commandValidator,
                () => CommandResponseHandling.Return,
                slaveOk,
                _resultSerializer,
                _messageEncoderSettings,
                cancellationToken);
        }

        /// <summary>
        /// Executes the protocol.
        /// </summary>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the command result.</returns>
        protected async Task<TCommandResult> ExecuteProtocolAsync(
            IChannelSource channelSource,
            ReadPreference readPreference,
            CancellationToken cancellationToken)
        {
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            {
                return await ExecuteProtocolAsync(channel, channelSource.ServerDescription, readPreference, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
