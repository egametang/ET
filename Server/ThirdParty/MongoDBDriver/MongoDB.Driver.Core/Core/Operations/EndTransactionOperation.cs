/* Copyright 2018-present MongoDB Inc.
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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Abstract base class for AbortTransactionOperation and CommitTransactionOperation.
    /// </summary>
    public abstract class EndTransactionOperation : IReadOperation<BsonDocument>
    {
        // private fields
        private MessageEncoderSettings _messageEncoderSettings;
        private readonly WriteConcern _writeConcern;

        // protected constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EndTransactionOperation"/> class.
        /// </summary>
        /// <param name="writeConcern">The write concern.</param>
        protected EndTransactionOperation(WriteConcern writeConcern)
        {
            _writeConcern = Ensure.IsNotNull(writeConcern, nameof(writeConcern));
        }

        // public properties
        /// <summary>
        /// Gets or sets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
            set { _messageEncoderSettings = value; }
        }

        /// <summary>
        /// Gets the write concern.
        /// </summary>
        /// <value>
        /// The write concern.
        /// </value>
        public WriteConcern WriteConcern => _writeConcern;

        // protected properties
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        protected abstract string CommandName { get; }

        // public methods
        /// <inheritdoc />
        public virtual BsonDocument Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadWriteBinding(channelSource.Server, channel, binding.Session.Fork()))
            {
                var operation = CreateOperation();
                return operation.Execute(channelBinding, cancellationToken);
            }
        }

        /// <inheritdoc />
        public virtual async Task<BsonDocument> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadWriteBinding(channelSource.Server, channel, binding.Session.Fork()))
            {
                var operation = CreateOperation();
                return await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
            }
        }

        // private methods
        private BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { CommandName, 1 },
                { "writeConcern", () => _writeConcern.ToBsonDocument(), !_writeConcern.IsServerDefault }
            };
        }

        private IReadOperation<BsonDocument> CreateOperation()
        {
            var command = CreateCommand();
            return new ReadCommandOperation<BsonDocument>(DatabaseNamespace.Admin, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
        }
    }

    /// <summary>
    /// The abort transaction operation.
    /// </summary>
    public sealed class AbortTransactionOperation : EndTransactionOperation
    {
        // public constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AbortTransactionOperation"/> class.
        /// </summary>
        /// <param name="writeConcern">The write concern.</param>
        public AbortTransactionOperation(WriteConcern writeConcern)
            : base(writeConcern)
        {
        }

        // protected properties
        /// <inheritdoc />
        protected override string CommandName => "abortTransaction";
    }

    /// <summary>
    /// The commit transaction operation.
    /// </summary>
    public sealed class CommitTransactionOperation : EndTransactionOperation
    {
        // public constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AbortTransactionOperation"/> class.
        /// </summary>
        /// <param name="writeConcern">The write concern.</param>
        public CommitTransactionOperation(WriteConcern writeConcern)
            : base(writeConcern)
        {
        }

        // protected properties
        /// <inheritdoc />
        protected override string CommandName => "commitTransaction";

        // public methods
        /// <inheritdoc />
        public override BsonDocument Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            try
            {
                return base.Execute(binding, cancellationToken);
            }
            catch (MongoException exception) when (ShouldReplaceTransientTransactionErrorWithUnknownTransactionCommitResult(exception))
            {
                ReplaceTransientTransactionErrorWithUnknownTransactionCommitResult(exception);
                throw;
            }
        }

        /// <inheritdoc />
        public override async Task<BsonDocument> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            try
            {
                return await base.ExecuteAsync(binding, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoException exception) when (ShouldReplaceTransientTransactionErrorWithUnknownTransactionCommitResult(exception))
            {
                ReplaceTransientTransactionErrorWithUnknownTransactionCommitResult(exception);
                throw;
            }
        }

        // private methods
        private void ReplaceTransientTransactionErrorWithUnknownTransactionCommitResult(MongoException exception)
        {
            exception.RemoveErrorLabel("TransientTransactionError");
            exception.AddErrorLabel("UnknownTransactionCommitResult");
        }

        private bool ShouldReplaceTransientTransactionErrorWithUnknownTransactionCommitResult(MongoException exception)
        {
            if (exception is MongoConnectionException)
            {
                return true;
            }

            if (exception is MongoNotPrimaryException || exception is MongoNodeIsRecoveringException)
            {
                return true;
            }

            var writeConcernException = exception as MongoWriteConcernException;
            if (writeConcernException != null)
            {
                var writeConcernError = writeConcernException.WriteConcernResult.Response?.GetValue("writeConcernError", null)?.AsBsonDocument;
                if (writeConcernError != null)
                {
                    var code = (ServerErrorCode)writeConcernError.GetValue("code", -1).ToInt32();
                    switch (code)
                    {
                        case ServerErrorCode.UnsatisfiableWriteConcern:
                        case ServerErrorCode.UnknownReplWriteConcern:
                            return false;

                        default:
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
