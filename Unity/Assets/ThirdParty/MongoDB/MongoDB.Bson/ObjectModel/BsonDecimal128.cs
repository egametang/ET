/* Copyright 2016 MongoDB Inc.
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
    /// Represents a BSON Decimal128 value.
    /// </summary>
    /// <seealso cref="MongoDB.Bson.BsonValue" />
#if NET45
    [Serializable]
#endif
    public class BsonDecimal128 : BsonValue, IComparable<BsonDecimal128>, IEquatable<BsonDecimal128>
    {
        // private fields
        private readonly Decimal128 _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDecimal128" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public BsonDecimal128(Decimal128 value)
        {
            _value = value;
        }

        // public properties
        /// <inheritdoc />
        public override BsonType BsonType
        {
            get { return BsonType.Decimal128; }
        }

        /// <inheritdoc />
        [Obsolete("Use Value instead.")]
        public override object RawValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public Decimal128 Value
        {
            get { return _value; }
        }

        // public operators
        /// <summary>
        /// Converts a Decimal128 to a BsonDecimal128.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>A BsonDecimal128.</returns>
        public static implicit operator BsonDecimal128(Decimal128 value)
        {
            return new BsonDecimal128(value);
        }

        /// <summary>
        /// Compares two BsonDecimal128 values.
        /// </summary>
        /// <param name="lhs">The first BsonDecimal128.</param>
        /// <param name="rhs">The other BsonDecimal128.</param>
        /// <returns>True if the two BsonDecimal128 values are not equal according to ==.</returns>
        public static bool operator !=(BsonDecimal128 lhs, BsonDecimal128 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonDecimal128 values.
        /// </summary>
        /// <param name="lhs">The first BsonDecimal128.</param>
        /// <param name="rhs">The other BsonDecimal128.</param>
        /// <returns>True if the two BsonDecimal128 values are equal according to ==.</returns>
        public static bool operator ==(BsonDecimal128 lhs, BsonDecimal128 rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            return lhs.OperatorEqualsImplementation(rhs);
        }

        // public static methods
        /// <summary>
        /// Creates a new instance of the BsonDecimal128 class.
        /// </summary>
        /// <param name="value">An object to be mapped to a BsonDecimal128.</param>
        /// <returns>A BsonDecimal128.</returns>
        public new static BsonDecimal128 Create(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return (BsonDecimal128)BsonTypeMapper.MapToBsonValue(value, BsonType.Decimal128);
        }

        // public methods
        /// <summary>
        /// Compares this BsonDecimal128 to another BsonDecimal128.
        /// </summary>
        /// <param name="other">The other BsonDecimal128.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonDecimal128 is less than, equal to, or greather than the other.</returns>
        public int CompareTo(BsonDecimal128 other)
        {
            if (other == null) { return 1; }
            return _value.CompareTo(other._value);
        }

        /// <inheritdoc />
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }

            var otherDecimal128 = other as BsonDecimal128;
            if (otherDecimal128 != null)
            {
                return _value.CompareTo(otherDecimal128.Value);
            }

            var otherInt32 = other as BsonInt32;
            if (otherInt32 != null)
            {
                return _value.CompareTo((Decimal128)otherInt32.Value);
            }

            var otherInt64 = other as BsonInt64;
            if (otherInt64 != null)
            {
                return _value.CompareTo((Decimal128)otherInt64.Value);
            }

            var otherDouble = other as BsonDouble;
            if (otherDouble != null)
            {
                return _value.CompareTo((Decimal128)otherDouble.Value);
            }

            return CompareTypeTo(other);
        }

        /// <summary>
        /// Compares this BsonDecimal128 to another BsonDecimal128.
        /// </summary>
        /// <param name="rhs">The other BsonDecimal128.</param>
        /// <returns>True if the two BsonDecimal128 values are equal.</returns>
        public bool Equals(BsonDecimal128 rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _value.Equals(rhs._value); // use Equals instead of == so NaN is handled correctly
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonDecimal128); // works even if obj is null or of a different type
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + BsonType.GetHashCode();
            hash = 37 * hash + _value.GetHashCode();
            return hash;
        }

        /// <inheritdoc />
        public override bool ToBoolean()
        {
            return !(Decimal128.IsNaN(_value) || _value.Equals(Decimal128.Zero));
        }

        /// <inheritdoc />
        public override decimal ToDecimal()
        {
            return Decimal128.ToDecimal(_value);
        }

        /// <inheritdoc />
        public override Decimal128 ToDecimal128()
        {
            return _value;
        }

        /// <inheritdoc />
        public override double ToDouble()
        {
            return Decimal128.ToDouble(_value);
        }

        /// <inheritdoc />
        public override int ToInt32()
        {
            return Decimal128.ToInt32(_value);
        }

        /// <inheritdoc />
        public override long ToInt64()
        {
            return Decimal128.ToInt64(_value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonConvert.ToString(_value);
        }

        // protected methods
        /// <inheritdoc/>
        protected override TypeCode IConvertibleGetTypeCodeImplementation()
        {
            return TypeCode.Object;
        }

        /// <inheritdoc/>
        protected override bool IConvertibleToBooleanImplementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToBoolean(provider);
        }

        /// <inheritdoc/>
        protected override byte IConvertibleToByteImplementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToByte(provider);
        }

        /// <inheritdoc/>
        protected override decimal IConvertibleToDecimalImplementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToDecimal(provider);
        }

        /// <inheritdoc/>
        protected override double IConvertibleToDoubleImplementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToDouble(provider);
        }

        /// <inheritdoc/>
        protected override short IConvertibleToInt16Implementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToInt16(provider);
        }

        /// <inheritdoc/>
        protected override int IConvertibleToInt32Implementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToInt32(provider);
        }

        /// <inheritdoc/>
        protected override long IConvertibleToInt64Implementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToInt64(provider);
        }

        /// <inheritdoc/>
#pragma warning disable 3002
        protected override sbyte IConvertibleToSByteImplementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToSByte(provider);
        }
#pragma warning restore

        /// <inheritdoc/>
        protected override float IConvertibleToSingleImplementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToSingle(provider);
        }

        /// <inheritdoc/>
        protected override string IConvertibleToStringImplementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToString(provider);
        }

        /// <inheritdoc/>
#pragma warning disable 3002
        protected override ushort IConvertibleToUInt16Implementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToUInt16(provider);
        }
#pragma warning restore

        /// <inheritdoc/>
#pragma warning disable 3002
        protected override uint IConvertibleToUInt32Implementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToUInt32(provider);
        }
#pragma warning restore

        /// <inheritdoc/>
#pragma warning disable 3002
        protected override ulong IConvertibleToUInt64Implementation(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToUInt64(provider);
        }
#pragma warning restore

        /// <inheritdoc/>
        protected override bool OperatorEqualsImplementation(BsonValue rhs)
        {
            var rhsDecimal128 = rhs as BsonDecimal128;
            if (rhsDecimal128 != null)
            {
                return _value == rhsDecimal128._value; // use == instead of Equals so NaN is handled correctly
            }

            var rhsInt32 = rhs as BsonInt32;
            if (rhsInt32 != null)
            {
                return _value == (Decimal128)rhsInt32.Value;
            }

            var rhsInt64 = rhs as BsonInt64;
            if (rhsInt64 != null)
            {
                return _value == (Decimal128)rhsInt64.Value;
            }

            var rhsDouble = rhs as BsonDouble;
            if (rhsDouble != null)
            {
                return _value == (Decimal128)rhsDouble.Value; // use == instead of Equals so NaN is handled correctly
            }

            return this.Equals(rhs);
        }
    }
}