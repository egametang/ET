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
using System.Xml;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON int value.
    /// </summary>
    [Serializable]
    public class BsonInt32 : BsonValue, IComparable<BsonInt32>, IEquatable<BsonInt32>
    {
        // private fields
        private int _value;

        // constructors
        /// <summary>
        /// Creates a new instance of the BsonInt32 class.
        /// </summary>
        /// <param name="value">The value.</param>
        public BsonInt32(int value)
            : base(BsonType.Int32)
        {
            _value = value;
        }

        // public static properties
        /// <summary>
        /// Gets an instance of BsonInt32 that represents -1.
        /// </summary>
        [Obsolete("Use new BsonInt32(-1) instead.")]
        public static BsonInt32 MinusOne
        {
            get { return new BsonInt32(-1); }
        }

        /// <summary>
        /// Gets an instance of BsonInt32 that represents -0.
        /// </summary>
        [Obsolete("Use new BsonInt32(0) instead.")]
        public static BsonInt32 Zero
        {
            get { return new BsonInt32(0); }
        }

        /// <summary>
        /// Gets an instance of BsonInt32 that represents 1.
        /// </summary>
        [Obsolete("Use new BsonInt32(1) instead.")]
        public static BsonInt32 One
        {
            get { return new BsonInt32(1); }
        }

        /// <summary>
        /// Gets an instance of BsonInt32 that represents 2.
        /// </summary>
        [Obsolete("Use new BsonInt32(2) instead.")]
        public static BsonInt32 Two
        {
            get { return new BsonInt32(2); }
        }

        /// <summary>
        /// Gets an instance of BsonInt32 that represents 3.
        /// </summary>
        [Obsolete("Use new BsonInt32(3) instead.")]
        public static BsonInt32 Three
        {
            get { return new BsonInt32(3); }
        }

        // public properties
        /// <summary>
        /// Gets the BsonInt32 as an int.
        /// </summary>
        [Obsolete("Use Value instead.")]
        public override object RawValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the value of this BsonInt32.
        /// </summary>
        public int Value
        {
            get { return _value; }
        }

        // public operators
        /// <summary>
        /// Converts an int to a BsonInt32.
        /// </summary>
        /// <param name="value">An int.</param>
        /// <returns>A BsonInt32.</returns>
        public static implicit operator BsonInt32(int value)
        {
            return new BsonInt32(value);
        }

        /// <summary>
        /// Compares two BsonInt32 values.
        /// </summary>
        /// <param name="lhs">The first BsonInt32.</param>
        /// <param name="rhs">The other BsonInt32.</param>
        /// <returns>True if the two BsonInt32 values are not equal according to ==.</returns>
        public static bool operator !=(BsonInt32 lhs, BsonInt32 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonInt32 values.
        /// </summary>
        /// <param name="lhs">The first BsonInt32.</param>
        /// <param name="rhs">The other BsonInt32.</param>
        /// <returns>True if the two BsonInt32 values are equal according to ==.</returns>
        public static bool operator ==(BsonInt32 lhs, BsonInt32 rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            return lhs.OperatorEqualsImplementation(rhs);
        }

        // public static methods
        /// <summary>
        /// Creates a new instance of the BsonInt32 class.
        /// </summary>
        /// <param name="value">An int.</param>
        /// <returns>A BsonInt32.</returns>
        [Obsolete("Use new BsonInt32(int value) instead.")]
        public static BsonInt32 Create(int value)
        {
            return new BsonInt32(value);
        }

        /// <summary>
        /// Creates a new BsonInt32.
        /// </summary>
        /// <param name="value">An object to be mapped to a BsonInt32.</param>
        /// <returns>A BsonInt32 or null.</returns>
        public new static BsonInt32 Create(object value)
        {
            if (value != null)
            {
                return (BsonInt32)BsonTypeMapper.MapToBsonValue(value, BsonType.Int32);
            }
            else
            {
                return null;
            }
        }

        // public methods
        /// <summary>
        /// Compares this BsonInt32 to another BsonInt32.
        /// </summary>
        /// <param name="other">The other BsonInt32.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonInt32 is less than, equal to, or greather than the other.</returns>
        public int CompareTo(BsonInt32 other)
        {
            if (other == null) { return 1; }
            return _value.CompareTo(other._value);
        }

        /// <summary>
        /// Compares the BsonInt32 to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonInt32 is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }
            var otherInt32 = other as BsonInt32;
            if (otherInt32 != null)
            {
                return _value.CompareTo(otherInt32._value);
            }
            var otherInt64 = other as BsonInt64;
            if (otherInt64 != null)
            {
                return ((long)_value).CompareTo(otherInt64.Value);
            }
            var otherDouble = other as BsonDouble;
            if (otherDouble != null)
            {
                return ((double)_value).CompareTo(otherDouble.Value);
            }
            return CompareTypeTo(other);
        }

        /// <summary>
        /// Compares this BsonInt32 to another BsonInt32.
        /// </summary>
        /// <param name="rhs">The other BsonInt32.</param>
        /// <returns>True if the two BsonInt32 values are equal.</returns>
        public bool Equals(BsonInt32 rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _value == rhs._value;
        }

        /// <summary>
        /// Compares this BsonInt32 to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonInt32 and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonInt32); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + BsonType.GetHashCode();
            hash = 37 * hash + _value.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Converts this BsonValue to a Boolean (using the JavaScript definition of truthiness).
        /// </summary>
        /// <returns>A Boolean.</returns>
        public override bool ToBoolean()
        {
            return _value != 0;
        }

        /// <summary>
        /// Converts this BsonValue to a Double.
        /// </summary>
        /// <returns>A Double.</returns>
        public override double ToDouble()
        {
            return (double)_value;
        }

        /// <summary>
        /// Converts this BsonValue to an Int32.
        /// </summary>
        /// <returns>An Int32.</returns>
        public override int ToInt32()
        {
            return _value;
        }

        /// <summary>
        /// Converts this BsonValue to an Int64.
        /// </summary>
        /// <returns>An Int32.</returns>
        public override long ToInt64()
        {
            return (long)_value;
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            return XmlConvert.ToString(_value);
        }

        // protected methods
        /// <summary>
        /// Compares this BsonInt32 against another BsonValue.
        /// </summary>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if this BsonInt32 and the other BsonValue are equal according to ==.</returns>
        protected override bool OperatorEqualsImplementation(BsonValue rhs)
        {
            var rhsInt32 = rhs as BsonInt32;
            if (rhsInt32 != null)
            {
                return _value == rhsInt32._value;
            }

            var rhsInt64 = rhs as BsonInt64;
            if (rhsInt64 != null)
            {
                return (long)_value == rhsInt64.Value;
            }

            var rhsDouble = rhs as BsonDouble;
            if (rhsDouble != null)
            {
                return (double)_value == rhsDouble.Value; // use == instead of Equals so NaN is handled correctly
            }

            return this.Equals(rhs);
        }
    }
}
