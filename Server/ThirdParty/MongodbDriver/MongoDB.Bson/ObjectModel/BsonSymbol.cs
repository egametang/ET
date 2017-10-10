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

namespace MongoDB.Bson
{
    // TODO: [Serializable] // must have custom deserialization to do SymbolTable lookup
    /// <summary>
    /// Represents a BSON symbol value.
    /// </summary>
    public class BsonSymbol : BsonValue, IComparable<BsonSymbol>, IEquatable<BsonSymbol>
    {
        // private fields
        private readonly string _name;

        // constructors
        // internal because only BsonSymbolTable should call this constructor
        internal BsonSymbol(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            _name = name;
        }

        // public properties
        /// <summary>
        /// Gets the BsonType of this BsonValue.
        /// </summary>
        public override BsonType BsonType
        {
            get { return BsonType.Symbol; }
        }

        /// <summary>
        /// Gets the name of the symbol.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        // public operators
        /// <summary>
        /// Converts a string to a BsonSymbol.
        /// </summary>
        /// <param name="name">A string.</param>
        /// <returns>A BsonSymbol.</returns>
        public static implicit operator BsonSymbol(string name)
        {
            return BsonSymbolTable.Lookup(name);
        }

        /// <summary>
        /// Compares two BsonSymbol values.
        /// </summary>
        /// <param name="lhs">The first BsonSymbol.</param>
        /// <param name="rhs">The other BsonSymbol.</param>
        /// <returns>True if the two BsonSymbol values are not equal according to ==.</returns>
        public static bool operator !=(BsonSymbol lhs, BsonSymbol rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonSymbol values.
        /// </summary>
        /// <param name="lhs">The first BsonSymbol.</param>
        /// <param name="rhs">The other BsonSymbol.</param>
        /// <returns>True if the two BsonSymbol values are equal according to ==.</returns>
        public static bool operator ==(BsonSymbol lhs, BsonSymbol rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            return lhs.Equals(rhs);
        }

        // public static methods
        /// <summary>
        /// Creates a new BsonSymbol.
        /// </summary>
        /// <param name="value">An object to be mapped to a BsonSymbol.</param>
        /// <returns>A BsonSymbol or null.</returns>
        public new static BsonSymbol Create(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return (BsonSymbol)BsonTypeMapper.MapToBsonValue(value, BsonType.Symbol);
        }

        // public methods
        // note: a BsonSymbol is guaranteed to be unique because it must be looked up in BsonSymbolTable
        // therefore the implementations of Equals and GetHashCode are considerably more efficient

        /// <summary>
        /// Compares this BsonSymbol to another BsonSymbol.
        /// </summary>
        /// <param name="other">The other BsonSymbol.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonSymbol is less than, equal to, or greather than the other.</returns>
        public int CompareTo(BsonSymbol other)
        {
            if (other == null) { return 1; }
            return _name.CompareTo(other._name);
        }

        /// <summary>
        /// Compares the BsonSymbol to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonSymbol is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }
            var otherSymbol = other as BsonSymbol;
            if (otherSymbol != null)
            {
                return _name.CompareTo(otherSymbol.Name);
            }
            var otherString = other as BsonString;
            if (otherString != null)
            {
                return _name.CompareTo(otherString.Value);
            }
            return CompareTypeTo(other);
        }

        /// <summary>
        /// Compares this BsonSymbol to another BsonSymbol.
        /// </summary>
        /// <param name="rhs">The other BsonSymbol.</param>
        /// <returns>True if the two BsonSymbol values are equal.</returns>
        public bool Equals(BsonSymbol rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return object.ReferenceEquals(this, rhs); // symbols are guaranteed to be unique
        }

        /// <summary>
        /// Compares this BsonSymbol to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonSymbol and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonSymbol); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            return _name;
        }
    }
}
