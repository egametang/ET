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
    /// Represents a BSON string value.
    /// </summary>
    [Serializable]
    public class BsonString : BsonValue, IComparable<BsonString>, IEquatable<BsonString>
    {
        // private static fields
        private static BsonString __emptyInstance = new BsonString("");

        // private fields
        private string _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonString class.
        /// </summary>
        /// <param name="value">The value.</param>
        public BsonString(string value)
            : base(BsonType.String)
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
            if (value != null)
            {
                return (BsonString)BsonTypeMapper.MapToBsonValue(value, BsonType.String);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new instance of the BsonString class.
        /// </summary>
        /// <param name="value">A string.</param>
        /// <returns>A BsonString.</returns>
        [Obsolete("Use new BsonString(string value) instead.")]
        public static BsonString Create(string value)
        {
            if (value != null)
            {
                // TODO: are there any other commonly used strings worth checking for?
                return value == "" ? __emptyInstance : new BsonString(value);
            }
            else
            {
                return null;
            }
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

        /// <summary>
        /// Converts this BsonValue to a Double.
        /// </summary>
        /// <returns>A Double.</returns>
        public override double ToDouble()
        {
            return XmlConvert.ToDouble(_value);
        }

        /// <summary>
        /// Converts this BsonValue to an Int32.
        /// </summary>
        /// <returns>An Int32.</returns>
        public override int ToInt32()
        {
            return XmlConvert.ToInt32(_value);
        }

        /// <summary>
        /// Converts this BsonValue to an Int64.
        /// </summary>
        /// <returns>An Int32.</returns>
        public override long ToInt64()
        {
            return XmlConvert.ToInt64(_value);
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            return _value;
        }
    }
}
