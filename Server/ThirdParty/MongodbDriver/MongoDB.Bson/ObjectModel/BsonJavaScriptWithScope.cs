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

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON JavaScript value with a scope.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class BsonJavaScriptWithScope : BsonJavaScript, IComparable<BsonJavaScriptWithScope>, IEquatable<BsonJavaScriptWithScope>
    {
        // private fields
        private readonly BsonDocument _scope;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonJavaScriptWithScope class.
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        /// <param name="scope">A scope (a set of variables with values).</param>
        public BsonJavaScriptWithScope(string code, BsonDocument scope)
            : base(code)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }
            _scope = scope;
        }

        // public operators
        /// <summary>
        /// Compares two BsonJavaScriptWithScope values.
        /// </summary>
        /// <param name="lhs">The first BsonJavaScriptWithScope.</param>
        /// <param name="rhs">The other BsonJavaScriptWithScope.</param>
        /// <returns>True if the two BsonJavaScriptWithScope values are not equal according to ==.</returns>
        public static bool operator !=(BsonJavaScriptWithScope lhs, BsonJavaScriptWithScope rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonJavaScriptWithScope values.
        /// </summary>
        /// <param name="lhs">The first BsonJavaScriptWithScope.</param>
        /// <param name="rhs">The other BsonJavaScriptWithScope.</param>
        /// <returns>True if the two BsonJavaScriptWithScope values are equal according to ==.</returns>
        public static bool operator ==(BsonJavaScriptWithScope lhs, BsonJavaScriptWithScope rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            return lhs.Equals(rhs);
        }

        // public properties
        /// <summary>
        /// Gets the BsonType of this BsonValue.
        /// </summary>
        public override BsonType BsonType
        {
            get { return BsonType.JavaScriptWithScope; }
        }

        /// <summary>
        /// Gets the scope (a set of variables with values).
        /// </summary>
        public BsonDocument Scope
        {
            get { return _scope; }
        }

        // public static methods
        /// <summary>
        /// Creates a new BsonJavaScriptWithScope.
        /// </summary>
        /// <param name="value">An object to be mapped to a BsonJavaScriptWithScope.</param>
        /// <returns>A BsonJavaScriptWithScope or null.</returns>
        public new static BsonJavaScriptWithScope Create(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return (BsonJavaScriptWithScope)BsonTypeMapper.MapToBsonValue(value, BsonType.JavaScriptWithScope);
        }

        // public methods
        /// <summary>
        /// Creates a shallow clone of the BsonJavaScriptWithScope (see also DeepClone).
        /// </summary>
        /// <returns>A shallow clone of the BsonJavaScriptWithScope.</returns>
        public override BsonValue Clone()
        {
            return new BsonJavaScriptWithScope(Code, (BsonDocument)_scope.Clone());
        }

        /// <summary>
        /// Creates a deep clone of the BsonJavaScriptWithScope (see also Clone).
        /// </summary>
        /// <returns>A deep clone of the BsonJavaScriptWithScope.</returns>
        public override BsonValue DeepClone()
        {
            BsonJavaScriptWithScope clone = new BsonJavaScriptWithScope(Code, new BsonDocument());
            foreach (BsonElement element in _scope)
            {
                clone._scope.Add(element.DeepClone());
            }
            return clone;
        }

        /// <summary>
        /// Compares this BsonJavaScriptWithScope to another BsonJavaScriptWithScope.
        /// </summary>
        /// <param name="other">The other BsonJavaScriptWithScope.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonJavaScriptWithScope is less than, equal to, or greather than the other.</returns>
        public int CompareTo(BsonJavaScriptWithScope other)
        {
            if (other == null) { return 1; }
            int r = Code.CompareTo(other.Code);
            if (r != 0) { return r; }
            return _scope.CompareTo(other._scope);
        }

        /// <summary>
        /// Compares the BsonJavaScriptWithScope to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonJavaScriptWithScope is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }
            var otherJavaScriptWithScope = other as BsonJavaScriptWithScope;
            if (otherJavaScriptWithScope != null)
            {
                return CompareTo(otherJavaScriptWithScope);
            }
            return CompareTypeTo(other);
        }

        /// <summary>
        /// Compares this BsonJavaScriptWithScope to another BsonJavaScriptWithScope.
        /// </summary>
        /// <param name="rhs">The other BsonJavaScriptWithScope.</param>
        /// <returns>True if the two BsonJavaScriptWithScope values are equal.</returns>
        public bool Equals(BsonJavaScriptWithScope rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return Code == rhs.Code && _scope == rhs._scope;
        }

        /// <summary>
        /// Compares this BsonJavaScriptWithScope to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonJavaScriptWithScope and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonJavaScriptWithScope); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + base.GetHashCode();
            hash = 37 * hash + _scope.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            return string.Format("new BsonJavaScript(\"{0}\", {1})", Code, _scope.ToJson());
        }
    }
}
