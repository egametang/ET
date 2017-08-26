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
using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents an immutable BSON array that is represented using only the raw bytes.
    /// </summary>
    [BsonSerializer(typeof(RawBsonArraySerializer))]
    public class RawBsonArray : BsonArray, IDisposable
    {
        // private fields
        private bool _disposed;
        private IByteBuffer _slice;
        private List<IDisposable> _disposableItems = new List<IDisposable>();
        private BsonBinaryReaderSettings _readerSettings = BsonBinaryReaderSettings.Defaults;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RawBsonArray"/> class.
        /// </summary>
        /// <param name="slice">The slice.</param>
        /// <exception cref="System.ArgumentNullException">slice</exception>
        /// <exception cref="System.ArgumentException">RawBsonArray cannot be used with an IByteBuffer that needs disposing.</exception>
        public RawBsonArray(IByteBuffer slice)
        {
            if (slice == null)
            {
                throw new ArgumentNullException("slice");
            }

            _slice = slice;
        }

        // public properties
        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        public override int Capacity
        {
            get
            {
                ThrowIfDisposed();
                return _slice.Capacity;
            }
            set
            {
                throw new NotSupportedException("RawBsonArray instances are immutable.");
            }
        }

        /// <summary>
        /// Gets the count of array elements.
        /// </summary>
        public override int Count
        {
            get
            {
                ThrowIfDisposed();
                using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
                using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
                {
                    var count = 0;

                    bsonReader.ReadStartDocument();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        bsonReader.SkipName();
                        bsonReader.SkipValue();
                        count++;
                    }
                    bsonReader.ReadEndDocument();

                    return count;
                }
            }
        }

        /// <summary>
        /// Gets whether the array is read-only.
        /// </summary>
        public override bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the array elements as raw values (see BsonValue.RawValue).
        /// </summary>
        [Obsolete("Use ToArray to ToList instead.")]
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
        /// Gets the array elements.
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
            get
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                ThrowIfDisposed();

                using (var stream = new ByteBufferStream(_slice, ownsBuffer: false))
                using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
                {
                    bsonReader.ReadStartDocument();
                    var i = 0;
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        bsonReader.SkipName();
                        if (i == index)
                        {
                            var context = BsonDeserializationContext.CreateRoot(bsonReader);
                            return DeserializeBsonValue(context);
                        }

                        bsonReader.SkipValue();
                        i++;
                    }
                    bsonReader.ReadEndDocument();

                    throw new ArgumentOutOfRangeException("index");
                }
            }
            set
            {
                throw new NotSupportedException("RawBsonArray instances are immutable.");
            }
        }

        // public methods
        /// <summary>
        /// Adds an element to the array.
        /// </summary>
        /// <param name="value">The value to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray Add(BsonValue value)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<bool> values)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<BsonValue> values)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<DateTime> values)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<double> values)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<int> values)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<long> values)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<ObjectId> values)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<string> values)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable values)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Creates a shallow clone of the array (see also DeepClone).
        /// </summary>
        /// <returns>A shallow clone of the array.</returns>
        public override BsonValue Clone()
        {
            return new RawBsonArray(CloneSlice());
        }

        /// <summary>
        /// Clears the array.
        /// </summary>
        public override void Clear()
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Tests whether the array contains a value.
        /// </summary>
        /// <param name="value">The value to test for.</param>
        /// <returns>True if the array contains the value.</returns>
        public override bool Contains(BsonValue value)
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
        /// Copies elements from this array to another array.
        /// </summary>
        /// <param name="array">The other array.</param>
        /// <param name="arrayIndex">The zero based index of the other array at which to start copying.</param>
        public override void CopyTo(BsonValue[] array, int arrayIndex)
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
                    array[arrayIndex++] = DeserializeBsonValue(context);
                }
                bsonReader.ReadEndDocument();
            }
        }

        /// <summary>
        /// Copies elements from this array to another array as raw values (see BsonValue.RawValue).
        /// </summary>
        /// <param name="array">The other array.</param>
        /// <param name="arrayIndex">The zero based index of the other array at which to start copying.</param>
        [Obsolete("Use ToArray or ToList instead.")]
        public override void CopyTo(object[] array, int arrayIndex)
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
                    array[arrayIndex++] = DeserializeBsonValue(context).RawValue;
                }
                bsonReader.ReadEndDocument();
            }
        }

        /// <summary>
        /// Creates a deep clone of the array (see also Clone).
        /// </summary>
        /// <returns>A deep clone of the array.</returns>
        public override BsonValue DeepClone()
        {
            ThrowIfDisposed();
            return new RawBsonArray(CloneSlice());
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
        /// Gets an enumerator that can enumerate the elements of the array.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public override IEnumerator<BsonValue> GetEnumerator()
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

        /// <summary>
        /// Gets the index of a value in the array.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>The zero based index of the value (or -1 if not found).</returns>
        public override int IndexOf(BsonValue value)
        {
            return IndexOf(value, 0, int.MaxValue);
        }

        /// <summary>
        /// Gets the index of a value in the array.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <param name="index">The zero based index at which to start the search.</param>
        /// <returns>The zero based index of the value (or -1 if not found).</returns>
        public override int IndexOf(BsonValue value, int index)
        {
            return IndexOf(value, index, int.MaxValue);
        }

        /// <summary>
        /// Gets the index of a value in the array.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <param name="index">The zero based index at which to start the search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <returns>The zero based index of the value (or -1 if not found).</returns>
        public override int IndexOf(BsonValue value, int index, int count)
        {
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
                    if (i >= index)
                    {
                        if (count == 0)
                        {
                            return -1;
                        }

                        if (DeserializeBsonValue(context).Equals(value))
                        {
                            return i;
                        }

                        count--;
                    }
                    else
                    {
                        bsonReader.SkipValue();
                    }

                    i++;
                }
                bsonReader.ReadEndDocument();

                return -1;
            }
        }

        /// <summary>
        /// Inserts a new value into the array.
        /// </summary>
        /// <param name="index">The zero based index at which to insert the new value.</param>
        /// <param name="value">The new value.</param>
        public override void Insert(int index, BsonValue value)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Removes the first occurrence of a value from the array.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>True if the value was removed.</returns>
        public override bool Remove(BsonValue value)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Removes an element from the array.
        /// </summary>
        /// <param name="index">The zero based index of the element to remove.</param>
        public override void RemoveAt(int index)
        {
            throw new NotSupportedException("RawBsonArray instances are immutable.");
        }

        /// <summary>
        /// Converts the BsonArray to an array of BsonValues.
        /// </summary>
        /// <returns>An array of BsonValues.</returns>
        public override BsonValue[] ToArray()
        {
            ThrowIfDisposed();
            return Values.ToArray();
        }

        /// <summary>
        /// Converts the BsonArray to a list of BsonValues.
        /// </summary>
        /// <returns>A list of BsonValues.</returns>
        public override List<BsonValue> ToList()
        {
            ThrowIfDisposed();
            return Values.ToList();
        }

        /// <summary>
        /// Returns a string representation of the array.
        /// </summary>
        /// <returns>A string representation of the array.</returns>
        public override string ToString()
        {
            ThrowIfDisposed();
            var parts = Values.Select(v => v.ToString()).ToArray();
            return string.Format("[{0}]", string.Join(", ", parts));
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
        /// <exception cref="System.ObjectDisposedException"></exception>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
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
