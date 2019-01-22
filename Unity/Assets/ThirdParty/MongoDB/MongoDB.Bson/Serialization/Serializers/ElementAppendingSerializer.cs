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
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// A serializer that serializes a document and appends elements to the end of it.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <seealso cref="MongoDB.Bson.Serialization.IBsonSerializer{TDocument}" />
    public class ElementAppendingSerializer<TDocument> : IBsonSerializer<TDocument>
    {
        // private fields
        private readonly IBsonSerializer<TDocument> _documentSerializer;
        private readonly List<BsonElement> _elements;
        private readonly Action<BsonWriterSettings> _writerSettingsConfigurator;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementAppendingSerializer{TDocument}" /> class.
        /// </summary>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="elements">The elements to append.</param>
        /// <param name="writerSettingsConfigurator">The writer settings configurator.</param>
        public ElementAppendingSerializer(
            IBsonSerializer<TDocument> documentSerializer, 
            IEnumerable<BsonElement> elements, 
            Action<BsonWriterSettings> writerSettingsConfigurator = null)
        {
            if (documentSerializer == null) { throw new ArgumentNullException(nameof(documentSerializer)); }
            if (elements == null) { throw new ArgumentNullException(nameof(elements)); }
            _documentSerializer = documentSerializer;
            _elements = elements.ToList();
            _writerSettingsConfigurator = writerSettingsConfigurator; // can be null
        }

        // public properties
        /// <inheritdoc />
        public Type ValueType => typeof(TDocument);

        // public methods
        /// <inheritdoc />
        public TDocument Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw new NotSupportedException();
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TDocument value)
        {
            var writer = context.Writer;
            var elementAppendingWriter = new ElementAppendingBsonWriter(writer, _elements, _writerSettingsConfigurator);
            var elementAppendingContext = BsonSerializationContext.CreateRoot(elementAppendingWriter, builder => ConfigureElementAppendingContext(builder, context));
            _documentSerializer.Serialize(elementAppendingContext, args, value);
        }

        void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            Serialize(context, args, (TDocument)value);
        }

        // private methods
        private void ConfigureElementAppendingContext(BsonSerializationContext.Builder builder, BsonSerializationContext originalContext)
        {
            builder.IsDynamicType = originalContext.IsDynamicType;
        }
    }
}
