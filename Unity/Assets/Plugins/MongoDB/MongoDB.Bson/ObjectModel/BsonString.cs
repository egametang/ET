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
    /// Represents a BSON string value.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class BsonString : BsonValue, IComparable<BsonString>, IEquatable<BsonString>
    {
        // private static fields
        private static BsonString __emptyInstance = new BsonString("");

        // private fields
        private readonly string _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonString class.
        /// </summary>
        /// <param name="value">The value.</param>
        public BsonString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            _value = value;
        }

        // public static properties
        /// <summary>
        /// Gets an instance of BsonString that represents an empty string.
        /// </summary>
        public static BsonString Empty
        {
            get { return __emptyInstance; }
        }

        // public properties
        /// <summary>
        /// Gets the BsonType of this BsonValue.
        /// </summary>
        public override BsonType BsonType
        {
            get { return BsonType.String; }
        }

        /// <summary>
        /// Gets the BsonString as a string.
        /// </summary>
        [Obsolete("Use Value instead.")]
        public override object RawValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the value of this BsonString.
        /// </summary>
        public string Value
        {
            get { return _value; }
        }

        // public operators
        /// <summary>
        /// Converts a string to a BsonString.
        /// </summary>
        /// <param name="value">A string.</param>
        /// <returns>A BsonString.</returns>
        public static implicit operator BsonString(string value)
        {
            if (value != null && value.Length == 0)
            {
                return __emptyInstance;
            }
            return new BsonString(value);
        }

        /// <summary>
        /// Compares two BsonString values.
        /// </summary>
        /// <param name="lhs">The first BsonString.</param>
        /// <param name="rhs">The other BsonString.</param>
        /// <returns>True if the two BsonString values are not equal according to ==.</returns>
        public static bool operator !=(BsonString lhs, BsonString rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonString values.
        /// </summary>
        /// <param name="lhs">The first BsonString.</param>
        /// <param name="rhs">The other BsonString.</param>
        /// <returns>True if the two BsonString values are equal according to ==.</returns>
        public static bool operator ==(BsonString lhs, BsonString rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            return lhs.Equals(rhs);
        }

        // public static methods
        /// <summary>
        /// Creates a new BsonString.
        /// </summary>
        /// <param name="value">An object to be mapped to a BsonString.</param>
        /// <returns>A BsonString or null.</returns>
        public new static BsonString Create(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return (BsonString)BsonTypeMapper.MapToBsonValue(value, BsonType.String);
        }

        // public methods
        /// <summary>
        /// Compares this BsonString to another BsonString.
        /// </summary>
        /// <param name="other">The other BsonString.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonString is less than, equal to, or greather than the other.</returns>
        public int CompareTo(BsonString other)
        {
            if (other == null) { return 1; }
            return _value.CompareTo(other.Value);
        }

        /// <summary>
        /// Compares the BsonString to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonString is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }
            var otherString = other as BsonString;
            if (otherString != null)
            {
                return _value.CompareTo(otherString.Value);
            }
            var otherSymbol = other as BsonSymbol;
            if (otherSymbol != null)
            {
                return _value.CompareTo(otherSymbol.Name);
            }
            return CompareTypeTo(other);
        }

        /// <summary>
        /// Compares this BsonString to another BsonString.
        /// </summary>
        /// <param name="rhs">The other BsonString.</param>
        /// <returns>True if the two BsonString values are equal.</returns>
        public bool Equals(BsonString rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _value == rhs._value;
        }

        /// <summary>
        /// Compares this BsonString to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonString and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonString); // works even if obj is null or of a different type
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
            return _value != "";
        }

        /// <inheritdoc/>
        public override decimal ToDecimal()
        {
            return JsonConvert.ToDecimal(_value);
        }

        /// <inheritdoc/>
        public override Decimal128 ToDecimal128()
        {
            return JsonConvert.ToDecimal128(_value);
        }

        /// <summary>
        /// Converts this BsonValue to a Double.
        /// </summary>
        /// <returns>A Double.</returns>
        public override double ToDouble()
        {
            return JsonConvert.ToDouble(_value);
        }

        /// <summary>
        /// Converts this BsonValue to an Int32.
        /// </summary>
        /// <returns>An Int32.</returns>
        public override int ToInt32()
        {
            return JsonConvert.ToInt32(_value);
        }

        /// <summary>
        /// Converts this BsonValue to an Int64.
        /// </summary>
        /// <returns>An Int32.</returns>
        public override long ToInt64()
        {
            return JsonConvert.ToInt64(_value);
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            return _value;
        }

        // protected methods
        /// <inheritdoc/>
        protected override TypeCode IConvertibleGetTypeCodeImplementation()
        {
            return TypeCode.String;
        }

        /// <inheritdoc/>
        protected override byte IConvertibleToByteImplementation(IFormatProvider provider)
        {
            return Convert.ToByte(_value, provider);
        }

        /// <inheritdoc/>
        protected override bool IConvertibleToBooleanImplementation(IFormatProvider provider)
        {
            return Convert.ToBoolean(_value, provider);
        }

        /// <inheritdoc/>
        protected override char IConvertibleToCharImplementation(IFormatProvider provider)
        {
            return Convert.ToChar(_value, provider);
        }

        /// <inheritdoc/>
        protected override DateTime IConvertibleToDateTimeImplementation(IFormatProvider provider)
        {
            return Convert.ToDateTime(_value, provider);
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
            return _value;
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
    }
}
