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
using System.Linq;
using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Shared;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON array.
    /// </summary>
    [Serializable]
    public class BsonArray : BsonValue, IComparable<BsonArray>, IEquatable<BsonArray>, IList<BsonValue>
    {
        // private fields
        private List<BsonValue> _values;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        public BsonArray()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        public BsonArray(IEnumerable<bool> values)
            : this(0)
        {
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        public BsonArray(IEnumerable<BsonValue> values)
            : this(0)
        {
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        public BsonArray(IEnumerable<DateTime> values)
            : this(0)
        {
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        public BsonArray(IEnumerable<double> values)
            : this(0)
        {
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        public BsonArray(IEnumerable<int> values)
            : this(0)
        {
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        public BsonArray(IEnumerable<long> values)
            : this(0)
        {
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        public BsonArray(IEnumerable<ObjectId> values)
            : this(0)
        {
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        public BsonArray(IEnumerable<string> values)
            : this(0)
        {
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        public BsonArray(IEnumerable values)
            : this(0)
        {
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the BsonArray class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the array.</param>
        public BsonArray(int capacity)
            : base(BsonType.Array)
        {
            _values = new List<BsonValue>(capacity);
        }

        // public operators
        /// <summary>
        /// Compares two BsonArray values.
        /// </summary>
        /// <param name="lhs">The first BsonArray.</param>
        /// <param name="rhs">The other BsonArray.</param>
        /// <returns>True if the two BsonArray values are not equal according to ==.</returns>
        public static bool operator !=(BsonArray lhs, BsonArray rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonArray values.
        /// </summary>
        /// <param name="lhs">The first BsonArray.</param>
        /// <param name="rhs">The other BsonArray.</param>
        /// <returns>True if the two BsonArray values are equal according to ==.</returns>
        public static bool operator ==(BsonArray lhs, BsonArray rhs)
        {
            return object.Equals(lhs, rhs); // handles lhs == null correctly
        }

        // public properties
        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        public virtual int Capacity
        {
            get { return _values.Capacity; }
            set { _values.Capacity = value; }
        }

        /// <summary>
        /// Gets the count of array elements.
        /// </summary>
        public virtual int Count
        {
            get { return _values.Count; }
        }

        /// <summary>
        /// Gets whether the array is read-only.
        /// </summary>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the array elements as raw values (see BsonValue.RawValue).
        /// </summary>
        [Obsolete("Use ToArray to ToList instead.")]
        public virtual IEnumerable<object> RawValues
        {
            get { return _values.Select(v => v.RawValue); }
        }

        /// <summary>
        /// Gets the array elements.
        /// </summary>
        public virtual IEnumerable<BsonValue> Values
        {
            get { return _values; }
        }

        // public indexers
        /// <summary>
        /// Gets or sets a value by position.
        /// </summary>
        /// <param name="index">The position.</param>
        /// <returns>The value.</returns>
        public override BsonValue this[int index]
        {
            get { return _values[index]; }
            set {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _values[index] = value;
            }
        }

        // public static methods
        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>A BsonArray or null.</returns>
        [Obsolete("Use new BsonArray(IEnumerable<bool> values) instead.")]
        public static BsonArray Create(IEnumerable<bool> values)
        {
            if (values != null)
            {
                return new BsonArray(values);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>A BsonArray or null.</returns>
        [Obsolete("Use new BsonArray(IEnumerable<BsonValue> values) instead.")]
        public static BsonArray Create(IEnumerable<BsonValue> values)
        {
            if (values != null)
            {
                return new BsonArray(values);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>A BsonArray or null.</returns>
        [Obsolete("Use new BsonArray(IEnumerable<DateTime> values) instead.")]
        public static BsonArray Create(IEnumerable<DateTime> values)
        {
            if (values != null)
            {
                return new BsonArray(values);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>A BsonArray or null.</returns>
        [Obsolete("Use new BsonArray(IEnumerable<double> values) instead.")]
        public static BsonArray Create(IEnumerable<double> values)
        {
            if (values != null)
            {
                return new BsonArray(values);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>A BsonArray or null.</returns>
        [Obsolete("Use new BsonArray(IEnumerable<int> values) instead.")]
        public static BsonArray Create(IEnumerable<int> values)
        {
            if (values != null)
            {
                return new BsonArray(values);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>A BsonArray or null.</returns>
        [Obsolete("Use new BsonArray(IEnumerable<long> values) instead.")]
        public static BsonArray Create(IEnumerable<long> values)
        {
            if (values != null)
            {
                return new BsonArray(values);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>A BsonArray or null.</returns>
        [Obsolete("Use new BsonArray(IEnumerable<ObjectId> values) instead.")]
        public static BsonArray Create(IEnumerable<ObjectId> values)
        {
            if (values != null)
            {
                return new BsonArray(values);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>A BsonArray or null.</returns>
        [Obsolete("Use new BsonArray(IEnumerable<string> values) instead.")]
        public static BsonArray Create(IEnumerable<string> values)
        {
            if (values != null)
            {
                return new BsonArray(values);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>A BsonArray or null.</returns>
        [Obsolete("Use new BsonArray(IEnumerable values) instead.")]
        public static BsonArray Create(IEnumerable values)
        {
            if (values != null)
            {
                return new BsonArray(values);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new BsonArray.
        /// </summary>
        /// <param name="value">A value to be mapped to a BsonArray.</param>
        /// <returns>A BsonArray or null.</returns>
        public new static BsonArray Create(object value)
        {
            if (value != null)
            {
                return (BsonArray)BsonTypeMapper.MapToBsonValue(value, BsonType.Array);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a BsonArray from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The reader.</param>
        /// <returns>A BsonArray.</returns>
        [Obsolete("Use BsonSerializer.Deserialize<BsonArray> instead.")]
        public static new BsonArray ReadFrom(BsonReader bsonReader)
        {
            return BsonSerializer.Deserialize<BsonArray>(bsonReader);
        }

        // public methods
        /// <summary>
        /// Adds an element to the array.
        /// </summary>
        /// <param name="value">The value to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray Add(BsonValue value)
        {
            if (value != null)
            {
                _values.Add(value);
            }
            return this;
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray AddRange(IEnumerable<bool> values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    Add((BsonBoolean)value);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray AddRange(IEnumerable<BsonValue> values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    if (value != null)
                    {
                        _values.Add(value);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray AddRange(IEnumerable<DateTime> values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                Add(new BsonDateTime(value));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray AddRange(IEnumerable<double> values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    Add(new BsonDouble(value));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray AddRange(IEnumerable<int> values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    Add(new BsonInt32(value));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray AddRange(IEnumerable<long> values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    Add(new BsonInt64(value));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray AddRange(IEnumerable<ObjectId> values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    Add(new BsonObjectId(value));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray AddRange(IEnumerable<string> values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    _values.Add((value == null) ? (BsonValue)BsonNull.Value : new BsonString(value));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds multiple elements to the array.
        /// </summary>
        /// <param name="values">A list of values to add to the array.</param>
        /// <returns>The array (so method calls can be chained).</returns>
        public virtual BsonArray AddRange(IEnumerable values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    Add(BsonTypeMapper.MapToBsonValue(value));
                }
            }
            return this;
        }

        /// <summary>
        /// Creates a shallow clone of the array (see also DeepClone).
        /// </summary>
        /// <returns>A shallow clone of the array.</returns>
        public override BsonValue Clone()
        {
            var clone = new BsonArray(_values.Capacity);
            foreach (var value in _values)
            {
                clone.Add(value);
            }
            return clone;
        }

        /// <summary>
        /// Clears the array.
        /// </summary>
        public virtual void Clear()
        {
            _values.Clear();
        }

        /// <summary>
        /// Compares the array to another array.
        /// </summary>
        /// <param name="rhs">The other array.</param>
        /// <returns>A 32-bit signed integer that indicates whether this array is less than, equal to, or greather than the other.</returns>
        public virtual int CompareTo(BsonArray rhs)
        {
            if (rhs == null) { return 1; }

            // lhs and rhs might be subclasses of BsonArray
            using (var lhsEnumerator = GetEnumerator())
            using (var rhsEnumerator = rhs.GetEnumerator())
            {
                while (true)
                {
                    var lhsHasNext = lhsEnumerator.MoveNext();
                    var rhsHasNext = rhsEnumerator.MoveNext();
                    if (!lhsHasNext && !rhsHasNext) { return 0; }
                    if (!lhsHasNext) { return -1; }
                    if (!rhsHasNext) { return 1; }

                    var lhsValue = lhsEnumerator.Current;
                    var rhsValue = rhsEnumerator.Current;
                    var result = lhsValue.CompareTo(rhsValue);
                    if (result != 0) { return result; }
                }
            }
        }

        /// <summary>
        /// Compares the array to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this array is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }
            var otherArray = other as BsonArray;
            if (otherArray != null)
            {
                return CompareTo(otherArray);
            }
            return CompareTypeTo(other);
        }

        /// <summary>
        /// Tests whether the array contains a value.
        /// </summary>
        /// <param name="value">The value to test for.</param>
        /// <returns>True if the array contains the value.</returns>
        public virtual bool Contains(BsonValue value)
        {
            // don't throw ArgumentNullException if value is null
            // just let _values.Contains return false
            return _values.Contains(value);
        }

        /// <summary>
        /// Copies elements from this array to another array.
        /// </summary>
        /// <param name="array">The other array.</param>
        /// <param name="arrayIndex">The zero based index of the other array at which to start copying.</param>
        public virtual void CopyTo(BsonValue[] array, int arrayIndex)
        {
            for (int i = 0, j = arrayIndex; i < _values.Count; i++, j++)
            {
                array[j] = _values[i];
            }
        }

        /// <summary>
        /// Copies elements from this array to another array as raw values (see BsonValue.RawValue).
        /// </summary>
        /// <param name="array">The other array.</param>
        /// <param name="arrayIndex">The zero based index of the other array at which to start copying.</param>
        [Obsolete("Use ToArray or ToList instead.")]
        public virtual void CopyTo(object[] array, int arrayIndex)
        {
            for (int i = 0, j = arrayIndex; i < _values.Count; i++, j++)
            {
                array[j] = _values[i].RawValue;
            }
        }

        /// <summary>
        /// Creates a deep clone of the array (see also Clone).
        /// </summary>
        /// <returns>A deep clone of the array.</returns>
        public override BsonValue DeepClone()
        {
            var clone = new BsonArray(_values.Capacity);
            foreach (var value in _values)
            {
                clone.Add(value.DeepClone());
            }
            return clone;
        }

        /// <summary>
        /// Compares this array to another array.
        /// </summary>
        /// <param name="obj">The other array.</param>
        /// <returns>True if the two arrays are equal.</returns>
        public bool Equals(BsonArray obj)
        {
            return Equals((object)obj); // handles obj == null correctly
        }

        /// <summary>
        /// Compares this BsonArray to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonArray and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null) || !(obj is BsonArray)) { return false; }

            // lhs and rhs might be subclasses of BsonArray
            var rhs = (BsonArray)obj;
            return Values.SequenceEqual(rhs.Values);
        }

        /// <summary>
        /// Gets an enumerator that can enumerate the elements of the array.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public virtual IEnumerator<BsonValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(BsonType)
                .HashElements(Values)
                .GetHashCode();
        }

        /// <summary>
        /// Gets the index of a value in the array.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>The zero based index of the value (or -1 if not found).</returns>
        public virtual int IndexOf(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return _values.IndexOf(value);
        }

        /// <summary>
        /// Gets the index of a value in the array.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <param name="index">The zero based index at which to start the search.</param>
        /// <returns>The zero based index of the value (or -1 if not found).</returns>
        public virtual int IndexOf(BsonValue value, int index)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return _values.IndexOf(value, index);
        }

        /// <summary>
        /// Gets the index of a value in the array.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <param name="index">The zero based index at which to start the search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <returns>The zero based index of the value (or -1 if not found).</returns>
        public virtual int IndexOf(BsonValue value, int index, int count)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return _values.IndexOf(value, index, count);
        }

        /// <summary>
        /// Inserts a new value into the array.
        /// </summary>
        /// <param name="index">The zero based index at which to insert the new value.</param>
        /// <param name="value">The new value.</param>
        public virtual void Insert(int index, BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            _values.Insert(index, value);
        }

        /// <summary>
        /// Removes the first occurrence of a value from the array.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>True if the value was removed.</returns>
        public virtual bool Remove(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return _values.Remove(value);
        }

        /// <summary>
        /// Removes an element from the array.
        /// </summary>
        /// <param name="index">The zero based index of the element to remove.</param>
        public virtual void RemoveAt(int index)
        {
            _values.RemoveAt(index);
        }

        /// <summary>
        /// Converts the BsonArray to an array of BsonValues.
        /// </summary>
        /// <returns>An array of BsonValues.</returns>
        public virtual BsonValue[] ToArray()
        {
            return _values.ToArray();
        }

        /// <summary>
        /// Converts the BsonArray to a list of BsonValues.
        /// </summary>
        /// <returns>A list of BsonValues.</returns>
        public virtual List<BsonValue> ToList()
        {
            return _values.ToList();
        }

        /// <summary>
        /// Returns a string representation of the array.
        /// </summary>
        /// <returns>A string representation of the array.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < _values.Count; i++)
            {
                if (i > 0) { sb.Append(", "); }
                sb.Append(_values[i].ToString());
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Writes the array to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The writer.</param>
        [Obsolete("Use BsonSerializer.Serialize<BsonArray> instead.")]
        public new void WriteTo(BsonWriter bsonWriter)
        {
            BsonSerializer.Serialize(bsonWriter, this);
        }

        // explicit interface implementations
        // our version of Add returns BsonArray
        void ICollection<BsonValue>.Add(BsonValue value)
        {
            Add(value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
