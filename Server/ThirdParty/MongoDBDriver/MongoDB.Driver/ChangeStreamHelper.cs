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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver
{
    internal static class ChangeStreamHelper
    {
        // public static methods
        public static ChangeStreamOperation<TResult> CreateChangeStreamOperation<TResult>(
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options,
            ReadConcern readConcern,
            MessageEncoderSettings messageEncoderSettings)
        {
            var renderedPipeline = RenderPipeline(pipeline, BsonDocumentSerializer.Instance);

            var operation = new ChangeStreamOperation<TResult>(
                renderedPipeline.Documents,
                renderedPipeline.OutputSerializer,
                messageEncoderSettings);
            SetOperationOptions(operation, options, readConcern);

            return operation;
        }

        public static ChangeStreamOperation<TResult> CreateChangeStreamOperation<TResult>(
            IMongoDatabase database,
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options,
            ReadConcern readConcern,
            MessageEncoderSettings messageEncoderSettings)
        {
            var renderedPipeline = RenderPipeline(pipeline, BsonDocumentSerializer.Instance);

            var operation = new ChangeStreamOperation<TResult>(
                database.DatabaseNamespace,
                renderedPipeline.Documents,
                renderedPipeline.OutputSerializer,
                messageEncoderSettings);
            SetOperationOptions(operation, options, readConcern);

            return operation;
        }

        public static ChangeStreamOperation<TResult> CreateChangeStreamOperation<TResult, TDocument>(
            IMongoCollection<TDocument> collection,
            PipelineDefinition<ChangeStreamDocument<TDocument>, TResult> pipeline,
            IBsonSerializer<TDocument> documentSerializer,
            ChangeStreamOptions options,
            ReadConcern readConcern,
            MessageEncoderSettings messageEncoderSettings)
        {
            var renderedPipeline = RenderPipeline(pipeline, documentSerializer);

            var operation = new ChangeStreamOperation<TResult>(
                collection.CollectionNamespace,
                renderedPipeline.Documents,
                renderedPipeline.OutputSerializer,
                messageEncoderSettings);
            SetOperationOptions(operation, options, readConcern);

            return operation;
        }

        // private static methods
        private static RenderedPipelineDefinition<TResult> RenderPipeline<TResult, TDocument>(
            PipelineDefinition<ChangeStreamDocument<TDocument>, TResult> pipeline,
            IBsonSerializer<TDocument> documentSerializer)
        {
            var changeStreamDocumentSerializer = new ChangeStreamDocumentSerializer<TDocument>(documentSerializer);
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            return pipeline.Render(changeStreamDocumentSerializer, serializerRegistry);
        }

        private static void SetOperationOptions<TResult>(
            ChangeStreamOperation<TResult> operation,
            ChangeStreamOptions options,
            ReadConcern readConcern)
        {
            options = options ?? new ChangeStreamOptions();

            operation.BatchSize = options.BatchSize;
            operation.Collation = options.Collation;
            operation.FullDocument = options.FullDocument;
            operation.MaxAwaitTime = options.MaxAwaitTime;
            operation.ReadConcern = readConcern;
            operation.ResumeAfter = options.ResumeAfter;
            operation.StartAtOperationTime = options.StartAtOperationTime;
        }
    }
}
