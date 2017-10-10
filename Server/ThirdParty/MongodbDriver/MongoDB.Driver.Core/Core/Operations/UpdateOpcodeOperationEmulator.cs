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
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    internal class UpdateOpcodeOperationEmulator
    {
        // fields
        private bool? _bypassDocumentValidation;
        private readonly CollectionNamespace _collectionNamespace;
        private int? _maxDocumentSize;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly UpdateRequest _request;
        private WriteConcern _writeConcern = WriteConcern.Acknowledged;

        // constructors
        public UpdateOpcodeOperationEmulator(
            CollectionNamespace collectionNamespace,
            UpdateRequest request,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _request = Ensure.IsNotNull(request, nameof(request));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        // properties
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
        }

        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets or sets the maximum size of a document.
        /// </summary>
        /// <value>
        /// The maximum size of a document.
        /// </value>
        public int? MaxDocumentSize
        {
            get { return _maxDocumentSize; }
            set { _maxDocumentSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        public UpdateRequest Request
        {
            get { return _request; }
        }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        public WriteConcernResult Execute(IChannelHandle channel, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(channel, nameof(channel));

            var operation = CreateOperation();
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                result = operation.Execute(channel, cancellationToken);
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return CreateResultOrThrow(channel, result, exception);
        }

        public async Task<WriteConcernResult> ExecuteAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(channel, nameof(channel));

            var operation = CreateOperation();
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                result = await operation.ExecuteAsync(channel, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return CreateResultOrThrow(channel, result, exception);
        }

        // private methods
        private BulkUpdateOperation CreateOperation()
        {
            var requests = new[] { _request };
            return new BulkUpdateOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                BypassDocumentValidation = _bypassDocumentValidation,
                IsOrdered = true,
                WriteConcern = _writeConcern
            };
        }

        private WriteConcernResult CreateResultOrThrow(IChannelHandle channel, BulkWriteOperationResult result, MongoBulkWriteOperationException exception)
        {
            var converter = new BulkWriteOperationResultConverter();
            if (exception != null)
            {
                throw converter.ToWriteConcernException(channel.ConnectionDescription.ConnectionId, exception);
            }
            else
            {
                if (_writeConcern.IsAcknowledged)
                {
                    return converter.ToWriteConcernResult(result);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
