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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a delete operation using the delete opcode.
    /// </summary>
    public class DeleteOpcodeOperation : IWriteOperation<WriteConcernResult>
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly DeleteRequest _request;
        private WriteConcern _writeConcern = WriteConcern.Acknowledged;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteOpcodeOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="request">The request.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public DeleteOpcodeOperation(
            CollectionNamespace collectionNamespace,
            DeleteRequest request,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _request = Ensure.IsNotNull(request, nameof(request));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        public DeleteRequest Request
        {
            get { return _request; }
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
        /// Gets or sets the write concern.
        /// </summary>
        /// <value>
        /// The write concern.
        /// </value>
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        /// <inheritdoc/>
        public WriteConcernResult Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = binding.GetWriteChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            {
                if (Feature.WriteCommands.IsSupported(channel.ConnectionDescription.ServerVersion) && _writeConcern.IsAcknowledged)
                {
                    var emulator = CreateEmulator();
                    return emulator.Execute(channel, cancellationToken);
                }
                else
                {
                    return ExecuteProtocol(channel, cancellationToken);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<WriteConcernResult> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = await binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            {
                if (Feature.WriteCommands.IsSupported(channel.ConnectionDescription.ServerVersion) && _writeConcern.IsAcknowledged)
                {
                    var emulator = CreateEmulator();
                    return await emulator.ExecuteAsync(channel, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return await ExecuteProtocolAsync(channel, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        // private methods
        private DeleteOpcodeOperationEmulator CreateEmulator()
        {
            return new DeleteOpcodeOperationEmulator(_collectionNamespace, _request, _messageEncoderSettings)
            {
                WriteConcern = _writeConcern
            };
        }

        private WriteConcernResult ExecuteProtocol(IChannelHandle channel, CancellationToken cancellationToken)
        {
            if (_request.Collation != null)
            {
                throw new NotSupportedException("OP_DELETE does not support collations.");
            }

            return channel.Delete(
                _collectionNamespace,
                _request.Filter,
                _request.Limit != 1,
                _messageEncoderSettings,
                _writeConcern,
                cancellationToken);
        }

        private Task<WriteConcernResult> ExecuteProtocolAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            if (_request.Collation != null)
            {
                throw new NotSupportedException("OP_DELETE does not support collations.");
            }

            return channel.DeleteAsync(
                _collectionNamespace,
                _request.Filter,
                _request.Limit != 1,
                _messageEncoderSettings,
                _writeConcern,
                cancellationToken);
        }
    }
}
