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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents the payload type.
    /// </summary>
    public enum PayloadType
    {
        /// <summary>
        /// Payload type 0.
        /// </summary>
        Type0 = 0,
        /// <summary>
        /// Payload type 1.
        /// </summary>
        Type1 = 1
    }

    /// <summary>
    /// Represents a CommandMessage section.
    /// </summary>
    public abstract class CommandMessageSection
    {
        /// <summary>
        /// Gets the type of the payload.
        /// </summary>
        /// <value>
        /// The type of the payload.
        /// </value>
        public abstract PayloadType PayloadType { get; }
    }

    /// <summary>
    /// Represents a Type 0 CommandMessage section.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.WireProtocol.Messages.CommandMessageSection" />
    public abstract class Type0CommandMessageSection : CommandMessageSection
    {
        // private fields
        private readonly object _document;
        private readonly IBsonSerializer _documentSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Type0CommandMessageSection{TDocument}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="documentSerializer">The document serializer.</param>
        public Type0CommandMessageSection(object document, IBsonSerializer documentSerializer)
        {
            Ensure.IsNotNull((object)document, nameof(document));
            _document = document;
            _documentSerializer = Ensure.IsNotNull(documentSerializer, nameof(documentSerializer));
        }

        // public properties
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public object Document => _document;

        /// <summary>
        /// Gets the document serializer.
        /// </summary>
        /// <value>
        /// The document serializer.
        /// </value>
        public IBsonSerializer DocumentSerializer => _documentSerializer;

        /// <inheritdoc />
        public override PayloadType PayloadType => PayloadType.Type0;
    }

    /// <summary>
    /// Represents a Type 0 CommandMessage section.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <seealso cref="MongoDB.Driver.Core.WireProtocol.Messages.Type0CommandMessageSection" />
    public sealed class Type0CommandMessageSection<TDocument> : Type0CommandMessageSection
    {
        // private fields
        private readonly TDocument _document;
        private readonly IBsonSerializer<TDocument> _documentSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Type0CommandMessageSection{TDocument}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="documentSerializer">The document serializer.</param>
        public Type0CommandMessageSection(TDocument document, IBsonSerializer<TDocument> documentSerializer)
            : base(document, documentSerializer)
        {
            Ensure.IsNotNull((object)document, nameof(document));
            _document = document;
            _documentSerializer = Ensure.IsNotNull(documentSerializer, nameof(documentSerializer));
        }

        // public properties
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public new TDocument Document => _document;

        /// <summary>
        /// Gets the document serializer.
        /// </summary>
        /// <value>
        /// The document serializer.
        /// </value>
        public new IBsonSerializer<TDocument> DocumentSerializer => _documentSerializer;
    }

    /// <summary>
    /// Represents a Type 1 CommandMessage section.
    /// </summary>
    public abstract class Type1CommandMessageSection : CommandMessageSection
    {
        // private fields
        private readonly IBatchableSource<object> _documents;
        private readonly IBsonSerializer _documentSerializer;
        private readonly IElementNameValidator _elementNameValidator;
        private readonly string _identifier;
        private readonly int? _maxBatchCount;
        private readonly int? _maxDocumentSize;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Type1CommandMessageSection{TDocument}" /> class.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="elementNameValidator">The element name validator.</param>
        /// <param name="maxBatchCount">The maximum batch count.</param>
        /// <param name="maxDocumentSize">Maximum size of the document.</param>
        public Type1CommandMessageSection(
            string identifier,
            IBatchableSource<object> documents,
            IBsonSerializer documentSerializer,
            IElementNameValidator elementNameValidator,
            int? maxBatchCount,
            int? maxDocumentSize)
        {
            _identifier = Ensure.IsNotNull(identifier, nameof(identifier));
            _documents = Ensure.IsNotNull(documents, nameof(documents));
            _documentSerializer = Ensure.IsNotNull(documentSerializer, nameof(documentSerializer));
            _elementNameValidator = Ensure.IsNotNull(elementNameValidator, nameof(elementNameValidator));
            _maxBatchCount = Ensure.IsNullOrGreaterThanZero(maxBatchCount, nameof(maxBatchCount));
            _maxDocumentSize = Ensure.IsNullOrGreaterThanZero(maxDocumentSize, nameof(maxDocumentSize));
        }

        // public properties
        /// <summary>
        /// Gets the documents.
        /// </summary>
        /// <value>
        /// The documents.
        /// </value>
        public IBatchableSource<object> Documents => _documents;

        /// <summary>
        /// Gets the document serializer.
        /// </summary>
        /// <value>
        /// The document serializer.
        /// </value>
        public IBsonSerializer DocumentSerializer => _documentSerializer;

        /// <summary>
        /// Gets the type of the document.
        /// </summary>
        /// <value>
        /// The type of the document.
        /// </value>
        public abstract Type DocumentType { get; }

        /// <summary>
        /// Gets the element name validator.
        /// </summary>
        /// <value>
        /// The element name validator.
        /// </value>
        public IElementNameValidator ElementNameValidator => _elementNameValidator;

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Identifier => _identifier;

        /// <summary>
        /// Gets the maximum batch count.
        /// </summary>
        /// <value>
        /// The maximum batch count.
        /// </value>
        public int? MaxBatchCount => _maxBatchCount;

        /// <summary>
        /// Gets the maximum size of the document.
        /// </summary>
        /// <value>
        /// The maximum size of the document.
        /// </value>
        public int? MaxDocumentSize => _maxDocumentSize;

        /// <inheritdoc />
        public override PayloadType PayloadType => PayloadType.Type1;
    }

    /// <summary>
    /// Represents a Type 1 CommandMessage section.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <seealso cref="MongoDB.Driver.Core.WireProtocol.Messages.CommandMessageSection" />
    public class Type1CommandMessageSection<TDocument> : Type1CommandMessageSection where TDocument : class
    {
        // private fields
        private readonly IBatchableSource<TDocument> _documents;
        private readonly IBsonSerializer<TDocument> _documentSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Type1CommandMessageSection{TDocument}" /> class.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="elementNameValidator">The element name validator.</param>
        /// <param name="maxBatchCount">The maximum batch count.</param>
        /// <param name="maxDocumentSize">Maximum size of the document.</param>
        public Type1CommandMessageSection(
            string identifier,
            IBatchableSource<TDocument> documents,
            IBsonSerializer<TDocument> documentSerializer,
            IElementNameValidator elementNameValidator,
            int? maxBatchCount,
            int? maxDocumentSize)
            : base(identifier, documents, documentSerializer, elementNameValidator, maxBatchCount, maxDocumentSize)
        {
            _documents = Ensure.IsNotNull(documents, nameof(documents));
            _documentSerializer = Ensure.IsNotNull(documentSerializer, nameof(documentSerializer));
        }

        // public properties
        /// <summary>
        /// Gets the documents.
        /// </summary>
        /// <value>
        /// The documents.
        /// </value>
        public new IBatchableSource<TDocument> Documents => _documents;

        /// <summary>
        /// Gets the document serializer.
        /// </summary>
        /// <value>
        /// The document serializer.
        /// </value>
        public new IBsonSerializer<TDocument> DocumentSerializer => _documentSerializer;

        /// <inheritdoc />
        public override Type DocumentType => typeof(TDocument);
    }
}
