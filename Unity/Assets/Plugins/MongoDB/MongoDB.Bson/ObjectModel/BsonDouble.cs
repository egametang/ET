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
using System.Globalization;
using MongoDB.Bson.IO;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON double value.
    /// </summary>
    /// <seealso cref="MongoDB.Bson.BsonValue" />
#if NET45
    [Serializable]
#endif
    public class BsonDouble : BsonValue, IComparable<BsonDouble>, IEquatable<BsonDouble>
    {
         #region static
        const int __minPrecreatedValue = -100;
        const int __maxPrecreatedValue = 100;
        private static readonly BsonDouble[] __precreatedInstances = new BsonDouble[__maxPrecreatedValue - __minPrecreatedValue + 1];

        static BsonDouble()
        {
            for (var i = __minPrecreatedValue; i <= __maxPrecreatedValue; i++)
            {
                var precreatedInstance = new BsonDouble(i);
                var index = i - __minPrecreatedValue;
                __precreatedInstances[index] = precreatedInstance;
            }
        }
        #endregion

        // private fields
        private readonly double _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDouble class.
        /// </summary>
        /// <param name="value">The value.</param>
        public BsonDouble(double value)
        {
            _value = value;
        }

        // public properties
        /// <inheritdoc />
        public override BsonType BsonType
        {
            get { return BsonType.Double; }
        }

        /// <inheritdoc />
        [Obsolete("Use Value instead.")]
        public override object RawValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the value of this BsonDouble.
        /// </summary>
        public double Value
        {
            get { return _value; }
        }

        // public operators
        /// <summary>
        /// Converts a double to a BsonDouble.
        /// </summary>
        /// <param name="value">A double.</param>
        /// <returns>A BsonDouble.</returns>
        public static implicit operator BsonDouble(double value)
        {
            var intValue = (int)value;
            if (intValue == value && intValue >= __minPrecreatedValue && intValue <= __maxPrecreatedValue)
            {
                var index = intValue - __minPrecreatedValue;
                return __precreatedInstances[index];
            }
            return new BsonDouble(value);
        }

        /// <summary>
        /// Compares two BsonDouble values.
        /// </summary>
        /// <param name="lhs">The first BsonDouble.</param>
        /// <param name="rhs">The other BsonDouble.</param>
        /// <returns>True if the two BsonDouble values are not equal according to ==.</returns>
        public static bool operator !=(BsonDouble lhs, BsonDouble rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonDouble values.
        /// </summary>
        /// <param name="lhs">The first BsonDouble.</param>
        /// <param name="rhs">The other BsonDouble.</param>
        /// <returns>True if the two BsonDouble values are equal according to ==.</returns>
        public static bool operator ==(BsonDouble lhs, BsonDouble rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            return lhs.OperatorEqualsImplementation(rhs);
        }

        // public static methods
        /// <summary>
        /// Creates a new instance of the BsonDouble class.
        /// </summary>
        /// <param name="value">An object to be mapped to a BsonDouble.</param>
        /// <returns>A BsonDouble.</returns>
        public new static BsonDouble Create(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return (BsonDouble)BsonTypeMapper.MapToBsonValue(value, BsonType.Double);
        }

        // public methods
        /// <summary>
        /// Compares this BsonDouble to another BsonDouble.
        /// </summary>
        /// <param name="other">The other BsonDouble.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonDouble is less than, equal to, or greather than the other.</returns>
        public int CompareTo(BsonDouble other)
        {
            if (other == null) { return 1; }
            return _value.CompareTo(other._value);
        }

        /// <inheritdoc />
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }

            var otherDouble = other as BsonDouble;
            if (otherDouble != null)
            {
                return _value.CompareTo(otherDouble._value);
            }

            var otherInt32 = other as BsonInt32;
            if (otherInt32 != null)
            {
                return _value.CompareTo((double)otherInt32.Value);
            }

            var otherInt64 = other as BsonInt64;
            if (otherInt64 != null)
            {
                return _value.CompareTo((double)otherInt64.Value);
            }

            var otherDecimal128 = other as BsonDecimal128;
            if (otherDecimal128 != null)
            {
                return ((Decimal128)_value).CompareTo(otherDecimal128.Value);
            }

            return CompareTypeTo(other);
        }

        /// <summary>
        /// Compares this BsonDouble to another BsonDouble.
        /// </summary>
        /// <param name="rhs">The other BsonDouble.</param>
        /// <returns>True if the two BsonDouble values are equal.</returns>
        public bool Equals(BsonDouble rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _value.Equals(rhs._value); // use Equals instead of == so NaN is handled correctly
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonDouble); // works even if obj is null or of a different type
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
            return !(double.IsNaN(_value) || _value == 0.0);
        }

        /// <inheritdoc />
        public override decimal ToDecimal()
        {
            return (decimal)_value;
        }

        /// <inheritdoc />
        public override Decimal128 ToDecimal128()
        {
            return (Decimal128)_value;
        }

        /// <inheritdoc />
        public override double ToDouble()
        {
            return _value;
        }

        /// <inheritdoc />
        public override int ToInt32()
        {
            return (int)_value;
        }

        /// <inheritdoc />
        public override long ToInt64()
        {
            return (long)_value;
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
            return TypeCode.Double;
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
        protected override decimal IConvertibleToDecimalImplementation(IFormatProvider provider)
        {
            return Convert.ToDecimal(_value, provider);
        }

        /// <inheritdoc/>
        protected override double IConvertibleToDoubleImplementation(IFormatProvider provider)
        {
            return _value;
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
            return Convert.ToInt64(_value, provider);
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

        /// <inheritdoc/>
        protected override bool OperatorEqualsImplementation(BsonValue rhs)
        {
            var rhsDouble = rhs as BsonDouble;
            if (rhsDouble != null)
            {
                return _value == rhsDouble._value; // use == instead of Equals so NaN is handled correctly
            }

            var rhsInt32 = rhs as BsonInt32;
            if (rhsInt32 != null)
            {
                return _value == (double)rhsInt32.Value;
            }

            var rhsInt64 = rhs as BsonInt64;
            if (rhsInt64 != null)
            {
                return _value == (double)rhsInt64.Value;
            }

            var rhsDecimal128 = rhs as BsonDecimal128;
            if (rhsDecimal128 != null)
            {
                return _value == (double)rhsDecimal128.Value; // use == instead of Equals so NaN is handled correctly
            }

            return this.Equals(rhs);
        }
    }
}
