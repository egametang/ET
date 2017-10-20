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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents an immutable BSON document that is represented using only the raw bytes.
    /// </summary>
    [BsonSerializer(typeof(RawBsonDocumentSerializer))]
    public class RawBsonDocument : BsonDocument, IDisposable
    {
        // private fields
        private bool _disposed;
        private IByteBuffer _slice;
        private List<IDisposable> _disposableItems = new List<IDisposable>();
        private BsonBinaryReaderSettings _readerSettings = BsonBinaryReaderSettings.Defaults;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RawBsonDocument"/> class.
        /// </summary>
        /// <param name="slice">The slice.</param>
        /// <exception cref="System.ArgumentNullException">slice</exception>
        /// <exception cref="System.ArgumentException">RawBsonDocument cannot be used with an IByteBuffer that needs disposing.</exception>
        public RawBsonDocument(IByteBuffer slice)
        {
            if (slice == null)
            {
                throw new ArgumentNullException("slice");
            }

            _slice = slice;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawBsonDocument"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public RawBsonDocument(byte[] bytes)
            : this(new ByteArrayBuffer(bytes, isReadOnly: true))
        {
        }

        // public properties
        /// <summary>
        /// Gets the number of elements.
        /// </summary>
        public override int ElementCount
        {
            get
            {
                ThrowIfDisposed();
                using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
                using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
                {
                    var elementCount = 0;

                    bsonReader.ReadStartDocument();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        bsonReader.SkipName();
                        bsonReader.SkipValue();
                        elementCount++;
                    }
                    bsonReader.ReadEndDocument();

                    return elementCount;
                }
            }
        }

        /// <summary>
        /// Gets the elements.
        /// </summary>
        public override IEnumerable<BsonElement> Elements
        {
            get
            {
                ThrowIfDisposed();
                using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
                using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
                {
                    var context = BsonDeserializationContext.CreateRoot(bsonReader); 

                    bsonReader.ReadStartDocument();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var name = bsonReader.ReadName();
                        var value = DeserializeBsonValue(context);
                        yield return new BsonElement(name, value);
                    }
                    bsonReader.ReadEndDocument();
                }
            }
        }

        /// <summary>
        /// Gets the element names.
        /// </summary>
        public override IEnumerable<string> Names
        {
            get
            {
                ThrowIfDisposed();
                using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
                using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
                {
                    bsonReader.ReadStartDocument();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        yield return bsonReader.ReadName();
                        bsonReader.SkipValue();
                    }
                    bsonReader.ReadEndDocument();
                }
            }
        }

        /// <summary>
        /// Gets the raw values (see BsonValue.RawValue).
        /// </summary>
        [Obsolete("Use Values instead.")]
        public override IEnumerable<object> RawValues
        {
            get
            {
                ThrowIfDisposed();
                using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
                using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
                {
                    var context = BsonDeserializationContext.CreateRoot(bsonReader);

                    bsonReader.ReadStartDocument();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        bsonReader.SkipName();
                        yield return DeserializeBsonValue(context).RawValue;
                    }
                    bsonReader.ReadEndDocument();
                }
            }
        }

        /// <summary>
        /// Gets the slice.
        /// </summary>
        /// <value>
        /// The slice.
        /// </value>
        public IByteBuffer Slice
        {
            get { return _slice; }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        public override IEnumerable<BsonValue> Values
        {
            get
            {
                ThrowIfDisposed();
                using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
                using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
                {
                    var context = BsonDeserializationContext.CreateRoot(bsonReader);

                    bsonReader.ReadStartDocument();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        bsonReader.SkipName();
                        yield return DeserializeBsonValue(context);
                    }
                    bsonReader.ReadEndDocument();
                }
            }
        }

        // public indexers
        /// <summary>
        /// Gets or sets a value by position.
        /// </summary>
        /// <param name="index">The position.</param>
        /// <returns>The value.</returns>
        public override BsonValue this[int index]
        {
            get { return GetValue(index); }
            set { Set(index, value); }
        }

        /// <summary>
        /// Gets the value of an element or a default value if the element is not found.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="defaultValue">The default value to return if the element is not found.</param>
        /// <returns>Teh value of the element or a default value if the element is not found.</returns>
        [Obsolete("Use GetValue(string name, BsonValue defaultValue) instead.")]
        public override BsonValue this[string name, BsonValue defaultValue]
        {
            get { return GetValue(name, defaultValue); }
        }

        /// <summary>
        /// Gets or sets a value by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The value.</returns>
        public override BsonValue this[string name]
        {
            get { return GetValue(name); }
            set { Set(name, value); }
        }

        // public methods
        /// <summary>
        /// Adds an element to the document.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Add(BsonElement element)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public override BsonDocument Add(Dictionary<string, object> dictionary)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keys">Which keys of the hash table to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public override BsonDocument Add(Dictionary<string, object> dictionary, IEnumerable<string> keys)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public override BsonDocument Add(IDictionary<string, object> dictionary)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keys">Which keys of the hash table to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public override BsonDocument Add(IDictionary<string, object> dictionary, IEnumerable<string> keys)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public override BsonDocument Add(IDictionary dictionary)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keys">Which keys of the hash table to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public override BsonDocument Add(IDictionary dictionary, IEnumerable keys)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds a list of elements to the document.
        /// </summary>
        /// <param name="elements">The list of elements.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public override BsonDocument Add(IEnumerable<BsonElement> elements)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds a list of elements to the document.
        /// </summary>
        /// <param name="elements">The list of elements.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public override BsonDocument Add(params BsonElement[] elements)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The value of the element.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Add(string name, BsonValue value)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Creates and adds an element to the document, but only if the condition is true.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The value of the element.</param>
        /// <param name="condition">Whether to add the element to the document.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public override BsonDocument Add(string name, BsonValue value, bool condition)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument AddRange(Dictionary<string, object> dictionary)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument AddRange(IDictionary dictionary)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds a list of elements to the document.
        /// </summary>
        /// <param name="elements">The list of elements.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument AddRange(IEnumerable<BsonElement> elements)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument AddRange(IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Clears the document (removes all elements).
        /// </summary>
        public override void Clear()
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Creates a shallow clone of the document (see also DeepClone).
        /// </summary>
        /// <returns>
        /// A shallow clone of the document.
        /// </returns>
        public override BsonValue Clone()
        {
            ThrowIfDisposed();
            return new RawBsonDocument(CloneSlice());
        }

        /// <summary>
        /// Tests whether the document contains an element with the specified name.
        /// </summary>
        /// <param name="name">The name of the element to look for.</param>
        /// <returns>
        /// True if the document contains an element with the specified name.
        /// </returns>
        public override bool Contains(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            ThrowIfDisposed();

            using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
            using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
            {
                bsonReader.ReadStartDocument();
                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    if (bsonReader.ReadName() == name)
                    {
                        return true;
                    }
                    bsonReader.SkipValue();
                }
                bsonReader.ReadEndDocument();

                return false;
            }
        }

        /// <summary>
        /// Tests whether the document contains an element with the specified value.
        /// </summary>
        /// <param name="value">The value of the element to look for.</param>
        /// <returns>
        /// True if the document contains an element with the specified value.
        /// </returns>
        public override bool ContainsValue(BsonValue value)
        {
            ThrowIfDisposed();
            using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
            using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
            {
                var context = BsonDeserializationContext.CreateRoot(bsonReader);

                bsonReader.ReadStartDocument();
                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    bsonReader.SkipName();
                    if (DeserializeBsonValue(context).Equals(value))
                    {
                        return true;
                    }
                }
                bsonReader.ReadEndDocument();

                return false;
            }
        }

        /// <summary>
        /// Creates a deep clone of the document (see also Clone).
        /// </summary>
        /// <returns>
        /// A deep clone of the document.
        /// </returns>
        public override BsonValue DeepClone()
        {
            ThrowIfDisposed();
            return new RawBsonDocument(CloneSlice());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets an element of this document.
        /// </summary>
        /// <param name="index">The zero based index of the element.</param>
        /// <returns>
        /// The element.
        /// </returns>
        public override BsonElement GetElement(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ThrowIfDisposed();

            using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
            using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
            {
                var context = BsonDeserializationContext.CreateRoot(bsonReader);
                
                bsonReader.ReadStartDocument();
                var i = 0;
                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    if (i == index)
                    {
                        var name = bsonReader.ReadName();
                        var value = DeserializeBsonValue(context);
                        return new BsonElement(name, value);
                    }

                    bsonReader.SkipName();
                    bsonReader.SkipValue();
                    i++;
                }
                bsonReader.ReadEndDocument();

                throw new ArgumentOutOfRangeException("index");
            }
        }

        /// <summary>
        /// Gets an element of this document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <returns>
        /// A BsonElement.
        /// </returns>
        public override BsonElement GetElement(string name)
        {
            ThrowIfDisposed();
            BsonElement element;
            if (TryGetElement(name, out element))
            {
                return element;
            }

            string message = string.Format("Element '{0}' not found.", name);
            throw new KeyNotFoundException(message);
        }

        /// <summary>
        /// Gets an enumerator that can be used to enumerate the elements of this document.
        /// </summary>
        /// <returns>
        /// An enumerator.
        /// </returns>
        public override IEnumerator<BsonElement> GetEnumerator()
        {
            ThrowIfDisposed();
            using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
            using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
            {
                var context = BsonDeserializationContext.CreateRoot(bsonReader);

                bsonReader.ReadStartDocument();
                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    var name = bsonReader.ReadName();
                    var value = DeserializeBsonValue(context);
                    yield return new BsonElement(name, value);
                }
                bsonReader.ReadEndDocument();
            }
        }

        /// <summary>
        /// Gets the value of an element.
        /// </summary>
        /// <param name="index">The zero based index of the element.</param>
        /// <returns>
        /// The value of the element.
        /// </returns>
        public override BsonValue GetValue(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ThrowIfDisposed();

            using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
            using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
            {
                var context = BsonDeserializationContext.CreateRoot(bsonReader);
                
                bsonReader.ReadStartDocument();
                var i = 0;
                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    bsonReader.SkipName();
                    if (i == index)
                    {
                        return DeserializeBsonValue(context);
                    }

                    bsonReader.SkipValue();
                    i++;
                }
                bsonReader.ReadEndDocument();

                throw new ArgumentOutOfRangeException("index");
            }
        }

        /// <summary>
        /// Gets the value of an element.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <returns>
        /// The value of the element.
        /// </returns>
        public override BsonValue GetValue(string name)
        {
            ThrowIfDisposed();
            BsonValue value;
            if (TryGetValue(name, out value))
            {
                return value;
            }

            string message = string.Format("Element '{0}' not found.", name);
            throw new KeyNotFoundException(message);
        }

        /// <summary>
        /// Gets the value of an element or a default value if the element is not found.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="defaultValue">The default value returned if the element is not found.</param>
        /// <returns>
        /// The value of the element or the default value if the element is not found.
        /// </returns>
        public override BsonValue GetValue(string name, BsonValue defaultValue)
        {
            ThrowIfDisposed();
            BsonValue value;
            if (TryGetValue(name, out value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Inserts a new element at a specified position.
        /// </summary>
        /// <param name="index">The position of the new element.</param>
        /// <param name="element">The element.</param>
        public override void InsertAt(int index, BsonElement element)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Materializes the RawBsonDocument into a regular BsonDocument.
        /// </summary>
        /// <param name="binaryReaderSettings">The binary reader settings.</param>
        /// <returns>A BsonDocument.</returns>
        public BsonDocument Materialize(BsonBinaryReaderSettings binaryReaderSettings)
        {
            ThrowIfDisposed();
            using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
            using (var reader = new BsonBinaryReader(stream, binaryReaderSettings))
            {
                var context = BsonDeserializationContext.CreateRoot(reader);
                return BsonDocumentSerializer.Instance.Deserialize(context);
            }
        }

        /// <summary>
        /// Merges another document into this one. Existing elements are not overwritten.
        /// </summary>
        /// <param name="document">The other document.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Merge(BsonDocument document)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Merges another document into this one, specifying whether existing elements are overwritten.
        /// </summary>
        /// <param name="document">The other document.</param>
        /// <param name="overwriteExistingElements">Whether to overwrite existing elements.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Merge(BsonDocument document, bool overwriteExistingElements)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Removes an element from this document (if duplicate element names are allowed
        /// then all elements with this name will be removed).
        /// </summary>
        /// <param name="name">The name of the element to remove.</param>
        public override void Remove(string name)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Removes an element from this document.
        /// </summary>
        /// <param name="index">The zero based index of the element to remove.</param>
        public override void RemoveAt(int index)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Removes an element from this document.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public override void RemoveElement(BsonElement element)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Sets the value of an element.
        /// </summary>
        /// <param name="index">The zero based index of the element whose value is to be set.</param>
        /// <param name="value">The new value.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Set(int index, BsonValue value)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Sets the value of an element (an element will be added if no element with this name is found).
        /// </summary>
        /// <param name="name">The name of the element whose value is to be set.</param>
        /// <param name="value">The new value.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Set(string name, BsonValue value)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Sets an element of the document (replaces any existing element with the same name or adds a new element if an element with the same name is not found).
        /// </summary>
        /// <param name="element">The new element.</param>
        /// <returns>
        /// The document.
        /// </returns>
        public override BsonDocument SetElement(BsonElement element)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Sets an element of the document (replacing the existing element at that position).
        /// </summary>
        /// <param name="index">The zero based index of the element to replace.</param>
        /// <param name="element">The new element.</param>
        /// <returns>
        /// The document.
        /// </returns>
        public override BsonDocument SetElement(int index, BsonElement element)
        {
            throw new NotSupportedException("RawBsonDocument instances are immutable.");
        }

        /// <summary>
        /// Tries to get an element of this document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="element">The element.</param>
        /// <returns>
        /// True if an element with that name was found.
        /// </returns>
        public override bool TryGetElement(string name, out BsonElement element)
        {
            ThrowIfDisposed();
            using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
            using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
            {
                var context = BsonDeserializationContext.CreateRoot(bsonReader);
                
                bsonReader.ReadStartDocument();
                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    if (bsonReader.ReadName() == name)
                    {
                        var value = DeserializeBsonValue(context);
                        element = new BsonElement(name, value);
                        return true;
                    }

                    bsonReader.SkipValue();
                }
                bsonReader.ReadEndDocument();

                element = default(BsonElement);
                return false;
            }
        }

        /// <summary>
        /// Tries to get the value of an element of this document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The value of the element.</param>
        /// <returns>
        /// True if an element with that name was found.
        /// </returns>
        public override bool TryGetValue(string name, out BsonValue value)
        {
            ThrowIfDisposed();
            using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
            using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
            {
                var context = BsonDeserializationContext.CreateRoot(bsonReader);
                
                bsonReader.ReadStartDocument();
                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    if (bsonReader.ReadName() == name)
                    {
                        value = DeserializeBsonValue(context);
                        return true;
                    }

                    bsonReader.SkipValue();
                }
                bsonReader.ReadEndDocument();

                value = null;
                return false;
            }
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_slice != null)
                    {
                        _slice.Dispose();
                        _slice = null;
                    }
                    if (_disposableItems != null)
                    {
                        _disposableItems.ForEach(x => x.Dispose());
                        _disposableItems = null;
                    }
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Throws if disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">RawBsonDocument</exception>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("RawBsonDocument");
            }
        }
        
        // private methods
        private IByteBuffer CloneSlice()
        {
            return _slice.GetSlice(0, _slice.Length);
        }

        private RawBsonArray DeserializeRawBsonArray(IBsonReader bsonReader)
        {
            var slice = bsonReader.ReadRawBsonArray();
            var nestedArray = new RawBsonArray(slice);
            _disposableItems.Add(nestedArray);
            return nestedArray;
        }

        private RawBsonDocument DeserializeRawBsonDocument(IBsonReader bsonReader)
        {
            var slice = bsonReader.ReadRawBsonDocument();
            var nestedDocument = new RawBsonDocument(slice);
            _disposableItems.Add(nestedDocument);
            return nestedDocument;
        }

        private BsonValue DeserializeBsonValue(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            switch (bsonReader.GetCurrentBsonType())
            {
                case BsonType.Array: return DeserializeRawBsonArray(bsonReader);
                case BsonType.Document: return DeserializeRawBsonDocument(bsonReader);
                default: return BsonValueSerializer.Instance.Deserialize(context);
            }
        }
    }
}
