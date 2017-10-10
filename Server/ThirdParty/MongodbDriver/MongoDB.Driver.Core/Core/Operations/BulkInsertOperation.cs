/* Copyright 2010-2016 MongoDB Inc.
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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations.ElementNameValidators;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    internal class BulkInsertOperation : BulkUnmixedWriteOperationBase
    {
        // constructors
        public BulkInsertOperation(
            CollectionNamespace collectionNamespace,
            IEnumerable<InsertRequest> requests,
            MessageEncoderSettings messageEncoderSettings)
            : base(collectionNamespace, requests, messageEncoderSettings)
        {
        }

        // properties
        protected override string CommandName
        {
            get { return "insert"; }
        }

        public new IEnumerable<InsertRequest> Requests
        {
            get { return base.Requests.Cast<InsertRequest>(); }
        }

        protected override string RequestsElementName
        {
            get { return "documents"; }
        }

        // methods
        protected override BatchSerializer CreateBatchSerializer(ConnectionDescription connectionDescription, int maxBatchCount, int maxBatchLength)
        {
            var isSystemIndexesCollection = CollectionNamespace.Equals(CollectionNamespace.DatabaseNamespace.SystemIndexesCollection);
            var elementNameValidator = isSystemIndexesCollection ? (IElementNameValidator)NoOpElementNameValidator.Instance : CollectionElementNameValidator.Instance;
            return new InsertBatchSerializer(connectionDescription, maxBatchCount, maxBatchLength, elementNameValidator);
        }

        protected override BulkUnmixedWriteOperationEmulatorBase CreateEmulator()
        {
            return new BulkInsertOperationEmulator(CollectionNamespace, Requests, MessageEncoderSettings)
            {
                MaxBatchCount = MaxBatchCount,
                MaxBatchLength = MaxBatchLength,
                IsOrdered = IsOrdered,
                WriteConcern = WriteConcern
            };
        }

        // nested types
        private class InsertBatchSerializer : BatchSerializer
        {
            // fields
            private IBsonSerializer _cachedSerializer;
            private Type _cachedSerializerType;
            private IElementNameValidator _elementNameValidator;

            // constructors
            public InsertBatchSerializer(ConnectionDescription connectionDescription, int maxBatchCount, int maxBatchLength, IElementNameValidator elementNameValidator)
                : base(connectionDescription, maxBatchCount, maxBatchLength)
            {
                _elementNameValidator = elementNameValidator;
            }

            // methods
            protected override void SerializeRequest(BsonSerializationContext context, WriteRequest request)
            {
                var insertRequest = (InsertRequest)request;
                var document = insertRequest.Document;

                var actualType = document.GetType();
                if (_cachedSerializerType != actualType)
                {
                    _cachedSerializer = BsonSerializer.LookupSerializer(actualType);
                    _cachedSerializerType = actualType;
                }

                var serializer = _cachedSerializer;

                var bsonWriter = (BsonBinaryWriter)context.Writer;
                bsonWriter.PushMaxDocumentSize(ConnectionDescription.MaxDocumentSize);
                bsonWriter.PushElementNameValidator(_elementNameValidator);
                try
                {
                    serializer.Serialize(context, document);
                }
                finally
                {
                    bsonWriter.PopMaxDocumentSize();
                    bsonWriter.PopElementNameValidator();
                }
            }
        }
    }
}
