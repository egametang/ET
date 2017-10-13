/* Copyright 2010-2014 MongoDB Inc.
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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON array that is not materialized until you start using it.
    /// </summary>
    [BsonSerializer(typeof(MaterializedOnDemandBsonArraySerializer))]
    public abstract class MaterializedOnDemandBsonArray : BsonArray, IDisposable
    {
        // private fields
        private bool _disposed;
        private bool _isMaterialized;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterializedOnDemandBsonArray"/> class.
        /// </summary>
        protected MaterializedOnDemandBsonArray()
        {
        }

        // public properties
        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        public override int Capacity
        {
            get
            {
                EnsureIsMaterialized();
                return base.Capacity;
            }
            set
            {
                EnsureIsMaterialized();
                base.Capacity = value;
            }
        }

        /// <summary>
        /// Gets the count of array elements.
        /// </summary>
        public override int Count
        {
            get
            {
                EnsureIsMaterialized();
                return base.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is materialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is materialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsMaterialized
        {
            get { return _isMaterialized; }
        }

        /// <summary>
        /// Gets the array elements as raw values (see BsonValue.RawValue).
        /// </summary>
        [Obsolete("Use ToArray to ToList instead.")]
        public override IEnumerable<object> RawValues
        {
            get
            {
                EnsureIsMaterialized();
                return base.RawValues;
            }
        }

        /// <summary>
        /// Gets the array elements.
        /// </summary>
        public override IEnumerable<BsonValue> Values
        {
            get
            {
                EnsureIsMaterialized();
                return base.Values;
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
                EnsureIsMaterialized();
                return base[index];
            }
            set
            {
                EnsureIsMaterialized();
                base[index] = value;
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
            EnsureIsMaterialized();
            return base.Add(value);
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<bool> values)
        {
            EnsureIsMaterialized();
            return base.AddRange(values);
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<BsonValue> values)
        {
            EnsureIsMaterialized();
            return base.AddRange(values);
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<DateTime> values)
        {
            EnsureIsMaterialized();
            return base.AddRange(values);
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<double> values)
        {
            EnsureIsMaterialized();
            return base.AddRange(values);
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<int> values)
        {
            EnsureIsMaterialized();
            return base.AddRange(values);
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<long> values)
        {
            EnsureIsMaterialized();
            return base.AddRange(values);
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<ObjectId> values)
        {
            EnsureIsMaterialized();
            return base.AddRange(values);
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable<string> values)
        {
            EnsureIsMaterialized();
            return base.AddRange(values);
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public override BsonArray AddRange(IEnumerable values)
        {
            EnsureIsMaterialized();
            return base.AddRange(values);
        }

        /// <summary>
        /// Clears the array.
        /// </summary>
        public override void Clear()
        {
            EnsureIsMaterialized();
            base.Clear();
        }

        /// <summary>
        /// Creates a shallow clone of the array (see also DeepClone).
        /// </summary>
        /// <returns>
        /// A shallow clone of the array.
        /// </returns>
        public override BsonValue Clone()
        {
            EnsureIsMaterialized();
            return base.Clone();
        }

        /// <summary>
        /// Compares the array to another array.
        /// </summary>
        /// <param name="other">The other array.</param>
        /// <returns>A 32-bit signed integer that indicates whether this array is less than, equal to, or greather than the other.</returns>
        public override int CompareTo(BsonArray other)
        {
            EnsureIsMaterialized();
            return base.CompareTo(other);
        }

        /// <summary>
        /// Compares the array to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this array is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            EnsureIsMaterialized();
            return base.CompareTo(other);
        }

        /// <summary>
        /// Tests whether the array contains a value.
        /// </summary>
        /// <param name="value">The value to test for.</param>
        /// <returns>True if the array contains the value.</returns>
        public override bool Contains(BsonValue value)
        {
            EnsureIsMaterialized();
            return base.Contains(value);
        }

        /// <summary>
        /// Copies elements from this array to another array.
        /// </summary>
        /// <param name="array">The other array.</param>
        /// <param name="arrayIndex">The zero based index of the other array at which to start copying.</param>
        public override void CopyTo(BsonValue[] array, int arrayIndex)
        {
            EnsureIsMaterialized();
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies elements from this array to another array as raw values (see BsonValue.RawValue).
        /// </summary>
        /// <param name="array">The other array.</param>
        /// <param name="arrayIndex">The zero based index of the other array at which to start copying.</param>
        [Obsolete("Use ToArray or ToList instead.")]
        public override void CopyTo(object[] array, int arrayIndex)
        {
            EnsureIsMaterialized();
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Creates a deep clone of the array (see also Clone).
        /// </summary>
        /// <returns>
        /// A deep clone of the array.
        /// </returns>
        public override BsonValue DeepClone()
        {
            EnsureIsMaterialized();
            return base.DeepClone();
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
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            EnsureIsMaterialized();
            return base.Equals(obj);
        }

        /// <summary>
        /// Gets an enumerator that can enumerate the elements of the array.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public override IEnumerator<BsonValue> GetEnumerator()
        {
            EnsureIsMaterialized();
            return base.GetEnumerator();
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            EnsureIsMaterialized();
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the index of a value in the array.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>The zero based index of the value (or -1 if not found).</returns>
        public override int IndexOf(BsonValue value)
        {
            EnsureIsMaterialized();
            return base.IndexOf(value);
        }

        /// <summary>
        /// Gets the index of a value in the array.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <param name="index">The zero based index at which to start the search.</param>
        /// <returns>The zero based index of the value (or -1 if not found).</returns>
        public override int IndexOf(BsonValue value, int index)
        {
            EnsureIsMaterialized();
            return base.IndexOf(value, index);
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
            EnsureIsMaterialized();
            return base.IndexOf(value, index, count);
        }

        /// <summary>
        /// Inserts a new value into the array.
        /// </summary>
        /// <param name="index">The zero based index at which to insert the new value.</param>
        /// <param name="value">The new value.</param>
        public override void Insert(int index, BsonValue value)
        {
            EnsureIsMaterialized();
            base.Insert(index, value);
        }

        /// <summary>
        /// Removes the first occurrence of a value from the array.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>True if the value was removed.</returns>
        public override bool Remove(BsonValue value)
        {
            EnsureIsMaterialized();
            return base.Remove(value);
        }

        /// <summary>
        /// Removes an element from the array.
        /// </summary>
        /// <param name="index">The zero based index of the element to remove.</param>
        public override void RemoveAt(int index)
        {
            EnsureIsMaterialized();
            base.RemoveAt(index);
        }

        /// <summary>
        /// Converts the BsonArray to an array of BsonValues.
        /// </summary>
        /// <returns>An array of BsonValues.</returns>
        public override BsonValue[] ToArray()
        {
            EnsureIsMaterialized();
            return base.ToArray();
        }

        /// <summary>
        /// Converts the BsonArray to a list of BsonValues.
        /// </summary>
        /// <returns>A list of BsonValues.</returns>
        public override List<BsonValue> ToList()
        {
            EnsureIsMaterialized();
            return base.ToList();
        }

        /// <summary>
        /// Returns a string representation of the array.
        /// </summary>
        /// <returns>A string representation of the array.</returns>
        public override string ToString()
        {
            EnsureIsMaterialized();
            return base.ToString();
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        /// <summary>
        /// Materializes the BsonArray.
        /// </summary>
        /// <returns>The materialized elements.</returns>
        protected abstract IEnumerable<BsonValue> Materialize();

        /// <summary>
        /// Informs subclasses that the Materialize process completed so they can free any resources related to the unmaterialized state.
        /// </summary>
        protected abstract void MaterializeCompleted();

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
        private void EnsureIsMaterialized()
        {
            ThrowIfDisposed();
            if (!_isMaterialized)
            {
                var values = Materialize();
                try
                {
                    _isMaterialized = true;
                    base.AddRange(values);
                    MaterializeCompleted();
                }
                catch
                {
                    base.Clear();
                    _isMaterialized = false;
                    throw;
                }
            }
        }

        internal class MaterializedOnDemandBsonArraySerializer : AbstractClassSerializer<MaterializedOnDemandBsonArray>
        {
        }
    }
}
