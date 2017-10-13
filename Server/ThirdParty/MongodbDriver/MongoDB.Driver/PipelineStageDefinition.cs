/* Copyright 2010-2015 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// A rendered pipeline stage.
    /// </summary>
    public interface IRenderedPipelineStageDefinition
    {
        /// <summary>
        /// Gets the name of the pipeline operator.
        /// </summary>
        /// <value>
        /// The name of the pipeline operator.
        /// </value>
        string OperatorName { get; }

        /// <summary>
        /// Gets the document.
        /// </summary>
        BsonDocument Document { get; }

        /// <summary>
        /// Gets the output serializer.
        /// </summary>
        IBsonSerializer OutputSerializer { get; }
    }

    /// <summary>
    /// A rendered pipeline stage.
    /// </summary>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    public class RenderedPipelineStageDefinition<TOutput> : IRenderedPipelineStageDefinition
    {
        private string _operatorName;
        private BsonDocument _document;
        private IBsonSerializer<TOutput> _outputSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderedPipelineStageDefinition{TOutput}"/> class.
        /// </summary>
        /// <param name="operatorName">Name of the pipeline operator.</param>
        /// <param name="document">The document.</param>
        /// <param name="outputSerializer">The output serializer.</param>
        public RenderedPipelineStageDefinition(string operatorName, BsonDocument document, IBsonSerializer<TOutput> outputSerializer)
        {
            _operatorName = Ensure.IsNotNull(operatorName, nameof(operatorName));
            _document = Ensure.IsNotNull(document, nameof(document));
            _outputSerializer = Ensure.IsNotNull(outputSerializer, nameof(outputSerializer));
        }

        /// <inheritdoc />
        public BsonDocument Document
        {
            get { return _document; }
        }

        /// <summary>
        /// Gets the output serializer.
        /// </summary>
        public IBsonSerializer<TOutput> OutputSerializer
        {
            get { return _outputSerializer; }
        }

        /// <inheritdoc />
        public string OperatorName
        {
            get { return _operatorName; }
        }

        /// <inheritdoc />
        IBsonSerializer IRenderedPipelineStageDefinition.OutputSerializer
        {
            get { return _outputSerializer; }
        }
    }

    /// <summary>
    /// A pipeline stage.
    /// </summary>
    public interface IPipelineStageDefinition
    {
        /// <summary>
        /// Gets the type of the input.
        /// </summary>
        Type InputType { get; }

        /// <summary>
        /// Gets the name of the pipeline operator.
        /// </summary>
        string OperatorName { get; }

        /// <summary>
        /// Gets the type of the output.
        /// </summary>
        Type OutputType { get; }

        /// <summary>
        /// Renders the specified document serializer.
        /// </summary>
        /// <param name="inputSerializer">The input serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>An <see cref="IRenderedPipelineStageDefinition" /></returns>
        IRenderedPipelineStageDefinition Render(IBsonSerializer inputSerializer, IBsonSerializerRegistry serializerRegistry);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="inputSerializer">The input serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        string ToString(IBsonSerializer inputSerializer, IBsonSerializerRegistry serializerRegistry);
    }

    /// <summary>
    /// Base class for pipeline stages.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    public abstract class PipelineStageDefinition<TInput, TOutput> : IPipelineStageDefinition
    {
        /// <summary>
        /// Gets the type of the input.
        /// </summary>
        public Type InputType
        {
            get { return typeof(TInput); }
        }

        /// <inheritdoc />
        public abstract string OperatorName { get; }

        /// <summary>
        /// Gets the type of the output.
        /// </summary>
        public Type OutputType
        {
            get { return typeof(TOutput); }
        }

        /// <summary>
        /// Renders the specified document serializer.
        /// </summary>
        /// <param name="inputSerializer">The input serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>A <see cref="RenderedPipelineStageDefinition{TOutput}" /></returns>
        public abstract RenderedPipelineStageDefinition<TOutput> Render(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry);

        /// <inheritdoc/>
        public override string ToString()
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var inputSerializer = serializerRegistry.GetSerializer<TInput>();
            return ToString(inputSerializer, serializerRegistry);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="inputSerializer">The input serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedStage = Render(inputSerializer, serializerRegistry);
            return renderedStage.Document.ToJson();
        }

        string IPipelineStageDefinition.ToString(IBsonSerializer inputSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return ToString((IBsonSerializer<TInput>)inputSerializer, serializerRegistry);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonDocument"/> to <see cref="PipelineStageDefinition{TInput, TOutput}"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator PipelineStageDefinition<TInput, TOutput>(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new BsonDocumentPipelineStageDefinition<TInput, TOutput>(document);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String" /> to <see cref="PipelineStageDefinition{TInput, TOutput}" />.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator PipelineStageDefinition<TInput, TOutput>(string json)
        {
            if (json == null)
            {
                return null;
            }

            return new JsonPipelineStageDefinition<TInput, TOutput>(json);
        }

        /// <inheritdoc />
        IRenderedPipelineStageDefinition IPipelineStageDefinition.Render(IBsonSerializer inputSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return Render((IBsonSerializer<TInput>)inputSerializer, serializerRegistry);
        }
    }

    /// <summary>
    /// A <see cref="BsonDocument"/> based stage.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    public sealed class BsonDocumentPipelineStageDefinition<TInput, TOutput> : PipelineStageDefinition<TInput, TOutput>
    {
        private readonly BsonDocument _document;
        private readonly IBsonSerializer<TOutput> _outputSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentPipelineStageDefinition{TInput, TOutput}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="outputSerializer">The output serializer.</param>
        public BsonDocumentPipelineStageDefinition(BsonDocument document, IBsonSerializer<TOutput> outputSerializer = null)
        {
            _document = Ensure.IsNotNull(document, nameof(document));
            _outputSerializer = outputSerializer;
        }

        /// <inheritdoc />
        public override string OperatorName
        {
            get { return _document.GetElement(0).Name; }
        }

        /// <inheritdoc />
        public override RenderedPipelineStageDefinition<TOutput> Render(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedPipelineStageDefinition<TOutput>(
                OperatorName,
                _document,
                _outputSerializer ?? (inputSerializer as IBsonSerializer<TOutput>) ?? serializerRegistry.GetSerializer<TOutput>());
        }
    }

    /// <summary>
    /// A JSON <see cref="String"/> based pipeline stage.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    public sealed class JsonPipelineStageDefinition<TInput, TOutput> : PipelineStageDefinition<TInput, TOutput>
    {
        private readonly BsonDocument _document;
        private readonly string _json;
        private readonly IBsonSerializer<TOutput> _outputSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPipelineStageDefinition{TInput, TOutput}" /> class.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="outputSerializer">The output serializer.</param>
        public JsonPipelineStageDefinition(string json, IBsonSerializer<TOutput> outputSerializer = null)
        {
            _json = Ensure.IsNotNullOrEmpty(json, nameof(json));
            _outputSerializer = outputSerializer;

            _document = BsonDocument.Parse(json);
        }

        /// <summary>
        /// Gets the json.
        /// </summary>
        public string Json
        {
            get { return _json; }
        }

        /// <inheritdoc />
        public override string OperatorName
        {
            get { return _document.GetElement(0).Name; }
        }

        /// <summary>
        /// Gets the output serializer.
        /// </summary>
        public IBsonSerializer<TOutput> OutputSerializer
        {
            get { return _outputSerializer; }
        }

        /// <inheritdoc />
        public override RenderedPipelineStageDefinition<TOutput> Render(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedPipelineStageDefinition<TOutput>(
                OperatorName,
                BsonDocument.Parse(_json),
                _outputSerializer ?? (inputSerializer as IBsonSerializer<TOutput>) ?? serializerRegistry.GetSerializer<TOutput>());
        }
    }

    internal sealed class DelegatedPipelineStageDefinition<TInput, TOutput> : PipelineStageDefinition<TInput, TOutput>
    {
        private readonly string _operatorName;
        private readonly Func<IBsonSerializer<TInput>, IBsonSerializerRegistry, RenderedPipelineStageDefinition<TOutput>> _renderer;

        public DelegatedPipelineStageDefinition(string operatorName, Func<IBsonSerializer<TInput>, IBsonSerializerRegistry, RenderedPipelineStageDefinition<TOutput>> renderer)
        {
            _operatorName = operatorName;
            _renderer = renderer;
        }

        public override string OperatorName
        {
            get { return _operatorName; }
        }

        public override RenderedPipelineStageDefinition<TOutput> Render(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return _renderer(inputSerializer, serializerRegistry);
        }
    }
}