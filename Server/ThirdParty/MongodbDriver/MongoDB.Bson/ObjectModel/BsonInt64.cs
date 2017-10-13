/* Copyright 2010-2016 MongoDB Inc.
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

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON long value.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class BsonInt64 : BsonValue, IComparable<BsonInt64>, IEquatable<BsonInt64>
    {
         #region static
        const long __minPrecreatedValue = -100L;
        const long __maxPrecreatedValue = 100L;
        private static readonly BsonInt64[] __precreatedInstances = new BsonInt64[__maxPrecreatedValue - __minPrecreatedValue + 1];

        static BsonInt64()
        {
            for (var i = __minPrecreatedValue; i <= __maxPrecreatedValue; i++)
            {
                var precreatedInstance = new BsonInt64(i);
                var index = i - __minPrecreatedValue;
                __precreatedInstances[index] = precreatedInstance;
            }
        }
        #endregion

       // private fields
        private readonly long _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonInt64 class.
        /// </summary>
        /// <param name="value">The value.</param>
        public BsonInt64(long value)
        {
            _value = value;
        }

        // public properties
        /// <summary>
        /// Gets the BsonType of this BsonValue.
        /// </summary>
        public override BsonType BsonType
        {
            get { return BsonType.Int64; }
        }

        /// <summary>
        /// Gets the BsonInt64 as a long.
        /// </summary>
        [Obsolete("Use Value instead.")]
        public override object RawValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the value of this BsonInt64.
        /// </summary>
        public long Value
        {
            get { return _value; }
        }

        // public operators
        /// <summary>
        /// Converts a long to a BsonInt64.
        /// </summary>
        /// <param name="value">A long.</param>
        /// <returns>A BsonInt64.</returns>
        public static implicit operator BsonInt64(long value)
        {
            if (value >= __minPrecreatedValue && value <= __maxPrecreatedValue)
            {
                var index = value - __minPrecreatedValue;
                return __precreatedInstances[index];
            }
            return new BsonInt64(value);
        }

        /// <summary>
        /// Compares two BsonInt64 values.
        /// </summary>
        /// <param name="lhs">The first BsonInt64.</param>
        /// <param name="rhs">The other BsonInt64.</param>
        /// <returns>True if the two BsonInt64 values are not equal according to ==.</returns>
        public static bool operator !=(BsonInt64 lhs, BsonInt64 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonInt64 values.
        /// </summary>
        /// <param name="lhs">The first BsonInt64.</param>
        /// <param name="rhs">The other BsonInt64.</param>
        /// <returns>True if the two BsonInt64 values are equal according to ==.</returns>
        public static bool operator ==(BsonInt64 lhs, BsonInt64 rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            return lhs.OperatorEqualsImplementation(rhs);
        }

        // public static methods
        /// <summary>
        /// Creates a new BsonInt64.
        /// </summary>
        /// <param name="value">An object to be mapped to a BsonInt64.</param>
        /// <returns>A BsonInt64 or null.</returns>
        public new static BsonInt64 Create(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return (BsonInt64)BsonTypeMapper.MapToBsonValue(value, BsonType.Int64);
        }

        // public methods
        /// <summary>
        /// Compares this BsonInt64 to another BsonInt64.
        /// </summary>
        /// <param name="other">The other BsonInt64.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonInt64 is less than, equal to, or greather than the other.</returns>
        public int CompareTo(BsonInt64 other)
        {
            if (other == null) { return 1; }
            return _value.CompareTo(other._value);
        }

        /// <summary>
        /// Compares the BsonInt64 to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonInt64 is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }

            var otherInt64 = other as BsonInt64;
            if (otherInt64 != null)
            {
                return _value.CompareTo(otherInt64._value);
            }

            var otherInt32 = other as BsonInt32;
            if (otherInt32 != null)
            {
                return _value.CompareTo((long)otherInt32.Value);
            }

            var otherDouble = other as BsonDouble;
            if (otherDouble != null)
            {
                return ((double)_value).CompareTo(otherDouble.Value);
            }

            var otherDecimal128 = other as BsonDecimal128;
            if (otherDecimal128 != null)
            {
                return new Decimal128(_value).CompareTo(otherDecimal128.Value);
            }

            return CompareTypeTo(other);
        }

        /// <summary>
        /// Compares this BsonInt64 to another BsonInt64.
        /// </summary>
        /// <param name="rhs">The other BsonInt64.</param>
        /// <returns>True if the two BsonInt64 values are equal.</returns>
        public bool Equals(BsonInt64 rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _value == rhs._value;
        }

        /// <summary>
        /// Compares this BsonInt64 to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonInt64 and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonInt64); // works even if obj is null or of a different type
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

        /// <inheritdoc/>
        public override decimal ToDecimal()
        {
            return (decimal)_value;
        }

        /// <inheritdoc/>
        public override Decimal128 ToDecimal128()
        {
            return (Decimal128)_value;
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
            return (int)_value;
        }

        /// <summary>
        /// Converts this BsonValue to an Int64.
        /// </summary>
        /// <returns>An Int32.</returns>
        public override long ToInt64()
        {
            return _value;
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            return JsonConvert.ToString(_value);
        }

        // protected methods
        /// <inheritdoc/>
        protected override TypeCode IConvertibleGetTypeCodeImplementation()
        {
            return TypeCode.Int64;
        }

        /// <inheritdoc/>
        protected override bool IConvertibleToBooleanImplementation(IFormatProvider provider)
        {
            return Convert.ToBoolean(_value, provider);
        }

        /// <inheritdoc/>
        protected override byte IConvertibleToByteImplementation(IFormatProvider provider)
        {
            return Convert.ToByte(_value, provider);
        }

        /// <inheritdoc/>
        protected override char IConvertibleToCharImplementation(IFormatProvider provider)
        {
            return Convert.ToChar(_value, provider);
        }

        /// <inheritdoc/>
        protected override decimal IConvertibleToDecimalImplementation(IFormatProvider provider)
        {
            return Convert.ToDecimal(_value, provider);
        }

        /// <inheritdoc/>
        protected override double IConvertibleToDoubleImplementation(IFormatProvider provider)
        {
            return Convert.ToDouble(_value, provider);
        }

        /// <inheritdoc/>
        protected override short IConvertibleToInt16Implementation(IFormatProvider provider)
        {
            return Convert.ToInt16(_value, provider);
        }

        /// <inheritdoc/>
        protected override int IConvertibleToInt32Implementation(IFormatProvider provider)
        {
            return Convert.ToInt32(_value, provider);
        }

        /// <inheritdoc/>
        protected override long IConvertibleToInt64Implementation(IFormatProvider provider)
        {
            return _value;
        }

        /// <inheritdoc/>
#pragma warning disable 3002
        protected override sbyte IConvertibleToSByteImplementation(IFormatProvider provider)
        {
            return Convert.ToSByte(_value, provider);
        }
#pragma warning restore

        /// <inheritdoc/>
        protected override float IConvertibleToSingleImplementation(IFormatProvider provider)
        {
            return Convert.ToSingle(_value, provider);
        }

        /// <inheritdoc/>
        protected override string IConvertibleToStringImplementation(IFormatProvider provider)
        {
            return Convert.ToString(_value, provider);
        }

        /// <inheritdoc/>
#pragma warning disable 3002
        protected override ushort IConvertibleToUInt16Implementation(IFormatProvider provider)
        {
            return Convert.ToUInt16(_value, provider);
        }
#pragma warning restore

        /// <inheritdoc/>
#pragma warning disable 3002
        protected override uint IConvertibleToUInt32Implementation(IFormatProvider provider)
        {
            return Convert.ToUInt32(_value, provider);
        }
#pragma warning restore

        /// <inheritdoc/>
#pragma warning disable 3002
        protected override ulong IConvertibleToUInt64Implementation(IFormatProvider provider)
        {
            return Convert.ToUInt64(_value, provider);
        }
#pragma warning restore
       
        /// <summary>
        /// Compares this BsonInt32 against another BsonValue.
        /// </summary>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if this BsonInt64 and the other BsonValue are equal according to ==.</returns>
        protected override bool OperatorEqualsImplementation(BsonValue rhs)
        {
            var rhsInt64 = rhs as BsonInt64;
            if (rhsInt64 != null)
            {
                return _value == rhsInt64._value;
            }

            var rhsInt32 = rhs as BsonInt32;
            if (rhsInt32 != null)
            {
                return _value == (long)rhsInt32.Value;
            }

            var rhsDouble = rhs as BsonDouble;
            if (rhsDouble != null)
            {
                return (double)_value == rhsDouble.Value; // use == instead of Equals so NaN is handled correctly
            }

            var rhsDecimal128 = rhs as BsonDecimal128;
            if (rhsDecimal128 != null)
            {
                return _value == (long)rhsDecimal128.Value;
            }

            return this.Equals(rhs);
        }
    }
}
