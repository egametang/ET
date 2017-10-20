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
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Translators;

namespace MongoDB.Driver
{
    /// <summary>
    /// A rendered command.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class RenderedCommand<TResult>
    {
        private readonly BsonDocument _document;
        private readonly IBsonSerializer<TResult> _resultSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderedCommand{TResult}" /> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        public RenderedCommand(BsonDocument document, IBsonSerializer<TResult> resultSerializer)
        {
            _document = Ensure.IsNotNull(document, nameof(document));
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public BsonDocument Document
        {
            get { return _document; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }
    }

    /// <summary>
    /// Base class for commands.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class Command<TResult>
    {
        /// <summary>
        /// Renders the command to a <see cref="RenderedCommand{TResult}" />.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>A <see cref="RenderedCommand{TResult}" />.</returns>
        public abstract RenderedCommand<TResult> Render(IBsonSerializerRegistry serializerRegistry);

        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonDocument"/> to <see cref="Command{TResult}"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Command<TResult>(BsonDocument document)
        {
            return new BsonDocumentCommand<TResult>(document);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Command{TResult}"/>.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Command<TResult>(string json)
        {
            return new JsonCommand<TResult>(json);
        }
    }

    /// <summary>
    /// A <see cref="BsonDocument" /> based command.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class BsonDocumentCommand<TResult> : Command<TResult>
    {
        private readonly BsonDocument _document;
        private readonly IBsonSerializer<TResult> _resultSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentCommand{TResult}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        public BsonDocumentCommand(BsonDocument document, IBsonSerializer<TResult> resultSerializer = null)
        {
            _document = Ensure.IsNotNull(document, nameof(document));
            _resultSerializer = resultSerializer;
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public BsonDocument Document
        {
            get { return _document; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        /// <inheritdoc />
        public override RenderedCommand<TResult> Render(IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedCommand<TResult>(
                _document,
                _resultSerializer ?? serializerRegistry.GetSerializer<TResult>());
        }
    }

    /// <summary>
    /// A JSON <see cref="String" /> based command.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class JsonCommand<TResult> : Command<TResult>
    {
        private readonly string _json;
        private readonly IBsonSerializer<TResult> _resultSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonCommand{TResult}"/> class.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        public JsonCommand(string json, IBsonSerializer<TResult> resultSerializer = null)
        {
            _json = Ensure.IsNotNullOrEmpty(json, nameof(json));
            _resultSerializer = resultSerializer; // can be null
        }

        /// <summary>
        /// Gets the json.
        /// </summary>
        public string Json
        {
            get { return _json; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        /// <inheritdoc />
        public override RenderedCommand<TResult> Render(IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedCommand<TResult>(
                BsonDocument.Parse(_json),
                _resultSerializer ?? serializerRegistry.GetSerializer<TResult>());
        }
    }

    /// <summary>
    /// An <see cref="Object" /> based command.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class ObjectCommand<TResult> : Command<TResult>
    {
        private readonly object _obj;
        private readonly IBsonSerializer<TResult> _resultSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectCommand{TResult}"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        public ObjectCommand(object obj, IBsonSerializer<TResult> resultSerializer = null)
        {
            _obj = Ensure.IsNotNull(obj, nameof(obj));
            _resultSerializer = resultSerializer;
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        public object Object
        {
            get { return _obj; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        /// <inheritdoc />
        public override RenderedCommand<TResult> Render(IBsonSerializerRegistry serializerRegistry)
        {
            var serializer = serializerRegistry.GetSerializer(_obj.GetType());
            return new RenderedCommand<TResult>(
                new BsonDocumentWrapper(_obj, serializer),
                _resultSerializer ?? serializerRegistry.GetSerializer<TResult>());
        }
    }
}
