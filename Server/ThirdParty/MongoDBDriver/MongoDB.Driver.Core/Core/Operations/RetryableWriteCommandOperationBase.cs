/* Copyright 2017-present MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a base class for a delete, insert or update command operation.
    /// </summary>
    public abstract class RetryableWriteCommandOperationBase : IWriteOperation<BsonDocument>, IRetryableWriteOperation<BsonDocument>
    {
        // private fields
        private readonly DatabaseNamespace _databaseNamespace;
        private bool _isOrdered = true;
        private int? _maxBatchCount;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private bool _retryRequested;
        private WriteConcern _writeConcern = WriteConcern.Acknowledged;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryableWriteCommandOperationBase" /> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public RetryableWriteCommandOperationBase(
            DatabaseNamespace databaseNamespace,
            MessageEncoderSettings messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        // public properties
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
        /// Gets or sets a value indicating whether the server should process the requests in order.
        /// </summary>
        /// <value>A value indicating whether the server should process the requests in order.</value>
        public bool IsOrdered
        {
            get { return _isOrdered; }
            set { _isOrdered = value; }
        }

        /// <summary>
        /// Gets or sets the maximum batch count.
        /// </summary>
        /// <value>
        /// The maximum batch count.
        /// </value>
        public int? MaxBatchCount
        {
            get { return _maxBatchCount; }
            set { _maxBatchCount = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
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
        /// Gets or sets a value indicating whether retry is enabled for the operation.
        /// </summary>
        /// <value>A value indicating whether retry is enabled.</value>
        public bool RetryRequested
        {
            get { return _retryRequested; }
            set { _retryRequested = value; }
        }

        /// <summary>
        /// Gets or sets the write concern.
        /// </summary>
        /// <value>
        /// The write concern.
        /// </value>
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = value; }
        }

        // public methods
        /// <inheritdoc />
        public virtual BsonDocument Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            using (var context = RetryableWriteContext.Create(binding, _retryRequested, cancellationToken))
            {
                return Execute(context, cancellationToken);
            }
        }

        /// <inheritdoc />
        public virtual BsonDocument Execute(RetryableWriteContext context, CancellationToken cancellationToken)
        {
            return RetryableWriteOperationExecutor.Execute(this, context, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<BsonDocument> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            using (var context = await RetryableWriteContext.CreateAsync(binding, _retryRequested, cancellationToken).ConfigureAwait(false))
            {
                return Execute(context, cancellationToken);
            }
        }

        /// <inheritdoc />
        public virtual Task<BsonDocument> ExecuteAsync(RetryableWriteContext context, CancellationToken cancellationToken)
        {
            return RetryableWriteOperationExecutor.ExecuteAsync(this, context, cancellationToken);
        }

        /// <inheritdoc />
        public BsonDocument ExecuteAttempt(RetryableWriteContext context, int attempt, long? transactionNumber, CancellationToken cancellationToken)
        {
            var args = GetCommandArgs(context, attempt, transactionNumber);

            return context.Channel.Command<BsonDocument>(
                context.ChannelSource.Session,
                ReadPreference.Primary,
                _databaseNamespace,
                args.Command,
                args.CommandPayloads,
                NoOpElementNameValidator.Instance,
                null, // additionalOptions,
                args.PostWriteAction,
                args.ResponseHandling,
                BsonDocumentSerializer.Instance,
                args.MessageEncoderSettings,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<BsonDocument> ExecuteAttemptAsync(RetryableWriteContext context, int attempt, long? transactionNumber, CancellationToken cancellationToken)
        {
            var args = GetCommandArgs(context, attempt, transactionNumber);

            return context.Channel.CommandAsync<BsonDocument>(
                context.ChannelSource.Session,
                ReadPreference.Primary,
                _databaseNamespace,
                args.Command,
                args.CommandPayloads,
                NoOpElementNameValidator.Instance,
                null, // additionalOptions,
                args.PostWriteAction,
                args.ResponseHandling,
                BsonDocumentSerializer.Instance,
                args.MessageEncoderSettings,
                cancellationToken);
        }

        // protected methods
        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="connectionDescription">The connection description.</param>
        /// <param name="attempt">The attempt.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <returns>
        /// A command.
        /// </returns>
        protected abstract BsonDocument CreateCommand(ICoreSessionHandle session, ConnectionDescription connectionDescription, int attempt, long? transactionNumber);

        /// <summary>
        /// Creates the command payloads.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="attempt">The attempt.</param>
        /// <returns>
        /// The command payloads.
        /// </returns>
        protected abstract IEnumerable<Type1CommandMessageSection> CreateCommandPayloads(IChannelHandle channel, int attempt);

        // private methods
        private MessageEncoderSettings CreateMessageEncoderSettings(IChannelHandle channel)
        {
            var clone = _messageEncoderSettings.Clone();
            clone.Add(MessageEncoderSettingsName.MaxDocumentSize, channel.ConnectionDescription.MaxDocumentSize);
            clone.Add(MessageEncoderSettingsName.MaxMessageSize, channel.ConnectionDescription.MaxMessageSize);
            clone.Add(MessageEncoderSettingsName.MaxWireDocumentSize, channel.ConnectionDescription.MaxWireDocumentSize);
            return clone;
        }

        private CommandArgs GetCommandArgs(RetryableWriteContext context, int attempt, long? transactionNumber)
        {
            var args = new CommandArgs();
            args.Command = CreateCommand(context.Binding.Session, context.Channel.ConnectionDescription, attempt, transactionNumber);
            args.CommandPayloads = CreateCommandPayloads(context.Channel, attempt).ToList();
            args.PostWriteAction = GetPostWriteAction(args.CommandPayloads);
            args.ResponseHandling = GetResponseHandling();
            args.MessageEncoderSettings = CreateMessageEncoderSettings(context.Channel);
            return args;
        }

        private Action<IMessageEncoderPostProcessor> GetPostWriteAction(List<Type1CommandMessageSection> commandPayloads)
        {
            if (!_writeConcern.IsAcknowledged && _isOrdered)
            {
                return encoder =>
                {
                    var requestsPayload = commandPayloads.Single();
                    if (!requestsPayload.Documents.AllItemsWereProcessed)
                    {
                        encoder.ChangeWriteConcernFromW0ToW1();
                    }
                };
            }
            else
            {
                return null;
            }
        }

        private CommandResponseHandling GetResponseHandling()
        {
            return _writeConcern.IsAcknowledged ? CommandResponseHandling.Return : CommandResponseHandling.NoResponseExpected;
        }

        // nested types
        private class CommandArgs
        {
            public BsonDocument Command { get; set; }
            public List<Type1CommandMessageSection> CommandPayloads { get; set; }
            public Action<IMessageEncoderPostProcessor> PostWriteAction { get; set; }
            public CommandResponseHandling ResponseHandling { get; set; }
            public MessageEncoderSettings MessageEncoderSettings { get; set; }
        }
    }
}
