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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol
{
    internal class InsertWireProtocol<TDocument> : WriteWireProtocolBase
    {
        // fields
        private readonly bool _continueOnError;
        private readonly BatchableSource<TDocument> _documentSource;
        private readonly int? _maxBatchCount;
        private readonly int? _maxMessageSize;
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        public InsertWireProtocol(
            CollectionNamespace collectionNamespace,
            WriteConcern writeConcern,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings,
            BatchableSource<TDocument> documentSource,
            int? maxBatchCount,
            int? maxMessageSize,
            bool continueOnError,
            Func<bool> shouldSendGetLastError = null)
            : base(collectionNamespace, messageEncoderSettings, writeConcern, shouldSendGetLastError)
        {
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _documentSource = Ensure.IsNotNull(documentSource, nameof(documentSource));
            _maxBatchCount = Ensure.IsNullOrGreaterThanZero(maxBatchCount, nameof(maxBatchCount));
            _maxMessageSize = Ensure.IsNullOrGreaterThanZero(maxMessageSize, nameof(maxMessageSize));
            _continueOnError = continueOnError;
        }

        // methods
        protected override RequestMessage CreateWriteMessage(IConnection connection)
        {
            return new InsertMessage<TDocument>(
                RequestMessage.GetNextRequestId(),
                CollectionNamespace,
                _serializer,
                _documentSource,
                _maxBatchCount ?? connection.Description.MaxBatchCount,
                _maxMessageSize ?? connection.Description.MaxMessageSize,
                _continueOnError);
        }
    }
}
