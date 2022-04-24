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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// A BsonWriter that appends elements to the end of a document.
    /// </summary>
    /// <seealso cref="MongoDB.Bson.IO.IBsonWriter" />
    internal sealed class ElementAppendingBsonWriter : WrappingBsonWriter
    {
        // private fields
        private int _depth;
        private readonly List<BsonElement> _elements;
        private readonly Action<BsonWriterSettings> _settingsConfigurator;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementAppendingBsonWriter" /> class.
        /// </summary>
        /// <param name="wrapped">The wrapped writer.</param>
        /// <param name="elements">The elements to append.</param>
        /// <param name="settingsConfigurator">The settings configurator.</param>
        public ElementAppendingBsonWriter(
            IBsonWriter wrapped,
            IEnumerable<BsonElement> elements,
            Action<BsonWriterSettings> settingsConfigurator)
            : base(wrapped)
        {
            if (elements == null) { throw new ArgumentNullException(nameof(elements)); }
            _elements = elements.ToList();
            _settingsConfigurator = settingsConfigurator ?? (s => { });
        }

        // public methods
        /// <inheritdoc />
        public override void WriteEndDocument()
        {
            if (--_depth == 0)
            {
                Wrapped.PushSettings(_settingsConfigurator);
                try
                {
                    var context = BsonSerializationContext.CreateRoot(Wrapped);
                    foreach (var element in _elements)
                    {
                        Wrapped.WriteName(element.Name);
                        BsonValueSerializer.Instance.Serialize(context, element.Value);
                    }
                }
                finally
                {
                    Wrapped.PopSettings();
                }
            }
            base.WriteEndDocument();
        }

        public override void WriteRawBsonDocument(IByteBuffer slice)
        {
            WriteStartDocument();

            if (Wrapped is BsonBinaryWriter binaryWriter)
            {
                // just copy the bytes (without the length and terminating null)
                var lengthBytes = new byte[4];
                slice.GetBytes(0, lengthBytes, 0, 4);
                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var elements = slice.GetSlice(4, length - 5))
                {
                    var stream = binaryWriter.BsonStream;
                    stream.WriteSlice(elements);
                }
            }
            else
            {
                throw new NotSupportedException("WriteRawBsonDocument supports only BsonBinaryWriter.");
            }

            WriteEndDocument();
        }

        /// <inheritdoc />
        public override void WriteStartDocument()
        {
            _depth++;
            base.WriteStartDocument();
        }
    }
}
