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
using System.Text.RegularExpressions;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON regular expression value.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class BsonRegularExpression : BsonValue, IComparable<BsonRegularExpression>, IEquatable<BsonRegularExpression>
    {
        // private fields
        private readonly string _pattern;
        private readonly string _options;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonRegularExpression class.
        /// </summary>
        /// <param name="pattern">A regular expression pattern.</param>
        public BsonRegularExpression(string pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }
            if (pattern.Length > 0 && pattern[0] == '/')
            {
                var index = pattern.LastIndexOf('/');
                var escaped = pattern.Substring(1, index - 1);
                var unescaped = (escaped == "(?:)") ? "" : escaped.Replace("\\/", "/");
                _pattern = unescaped;
                _options = pattern.Substring(index + 1);
            }
            else
            {
                _pattern = pattern;
                _options = "";
            }
        }

        /// <summary>
        /// Initializes a new instance of the BsonRegularExpression class.
        /// </summary>
        /// <param name="pattern">A regular expression pattern.</param>
        /// <param name="options">Regular expression options.</param>
        public BsonRegularExpression(string pattern, string options)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }
            _pattern = pattern;
            _options = options ?? "";
        }

        /// <summary>
        /// Initializes a new instance of the BsonRegularExpression class.
        /// </summary>
        /// <param name="regex">A Regex.</param>
        public BsonRegularExpression(Regex regex)
        {
            if (regex == null)
            {
                throw new ArgumentNullException("regex");
            }
            _pattern = regex.ToString();
            _options = "";
            if ((regex.Options & RegexOptions.IgnoreCase) != 0)
            {
                _options += "i";
            }
            if ((regex.Options & RegexOptions.Multiline) != 0)
            {
                _options += "m";
            }
            if ((regex.Options & RegexOptions.IgnorePatternWhitespace) != 0)
            {
                _options += "x";
            }
            if ((regex.Options & RegexOptions.Singleline) != 0)
            {
                _options += "s";
            }
        }

        // public properties
        /// <summary>
        /// Gets the BsonType of this BsonValue.
        /// </summary>
        public override BsonType BsonType
        {
            get { return BsonType.RegularExpression; }
        }

        /// <summary>
        /// Gets the regular expression pattern.
        /// </summary>
        public string Pattern
        {
            get { return _pattern; }
        }

        /// <summary>
        /// Gets the regular expression options.
        /// </summary>
        public string Options
        {
            get { return _options; }
        }

        // public operators
        /// <summary>
        /// Converts a Regex to a BsonRegularExpression.
        /// </summary>
        /// <param name="value">A Regex.</param>
        /// <returns>A BsonRegularExpression.</returns>
        public static implicit operator BsonRegularExpression(Regex value)
        {
            return new BsonRegularExpression(value);
        }

        /// <summary>
        /// Converts a string to a BsonRegularExpression.
        /// </summary>
        /// <param name="value">A string.</param>
        /// <returns>A BsonRegularExpression.</returns>
        public static implicit operator BsonRegularExpression(string value)
        {
            return new BsonRegularExpression(value);
        }

        /// <summary>
        /// Compares two BsonRegularExpression values.
        /// </summary>
        /// <param name="lhs">The first BsonRegularExpression.</param>
        /// <param name="rhs">The other BsonRegularExpression.</param>
        /// <returns>True if the two BsonRegularExpression values are not equal according to ==.</returns>
        public static bool operator !=(BsonRegularExpression lhs, BsonRegularExpression rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonRegularExpression values.
        /// </summary>
        /// <param name="lhs">The first BsonRegularExpression.</param>
        /// <param name="rhs">The other BsonRegularExpression.</param>
        /// <returns>True if the two BsonRegularExpression values are equal according to ==.</returns>
        public static bool operator ==(BsonRegularExpression lhs, BsonRegularExpression rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            return lhs.Equals(rhs);
        }

        // public methods
        /// <summary>
        /// Creates a new BsonRegularExpression.
        /// </summary>
        /// <param name="value">An object to be mapped to a BsonRegularExpression.</param>
        /// <returns>A BsonRegularExpression or null.</returns>
        public new static BsonRegularExpression Create(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return (BsonRegularExpression)BsonTypeMapper.MapToBsonValue(value, BsonType.RegularExpression);
        }

        // public methods
        /// <summary>
        /// Compares this BsonRegularExpression to another BsonRegularExpression.
        /// </summary>
        /// <param name="other">The other BsonRegularExpression.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonRegularExpression is less than, equal to, or greather than the other.</returns>
        public int CompareTo(BsonRegularExpression other)
        {
            if (other == null) { return 1; }
            int r = _pattern.CompareTo(other._pattern);
            if (r != 0) { return r; }
            return _options.CompareTo(other._options);
        }

        /// <summary>
        /// Compares the BsonRegularExpression to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonRegularExpression is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }
            var otherRegularExpression = other as BsonRegularExpression;
            if (otherRegularExpression != null)
            {
                return CompareTo(otherRegularExpression);
            }
            return CompareTypeTo(other);
        }

        /// <summary>
        /// Compares this BsonRegularExpression to another BsonRegularExpression.
        /// </summary>
        /// <param name="rhs">The other BsonRegularExpression.</param>
        /// <returns>True if the two BsonRegularExpression values are equal.</returns>
        public bool Equals(BsonRegularExpression rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _pattern == rhs._pattern && _options == rhs._options;
        }

        /// <summary>
        /// Compares this BsonRegularExpression to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonRegularExpression and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonRegularExpression); // works even if obj is null or of a different type
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
            hash = 37 * hash + _pattern.GetHashCode();
            hash = 37 * hash + _options.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Converts the BsonRegularExpression to a Regex.
        /// </summary>
        /// <returns>A Regex.</returns>
        public Regex ToRegex()
        {
            var options = RegexOptions.None;
            if (_options.IndexOf('i') != -1)
            {
                options |= RegexOptions.IgnoreCase;
            }
            if (_options.IndexOf('m') != -1)
            {
                options |= RegexOptions.Multiline;
            }
            if (_options.IndexOf('x') != -1)
            {
                options |= RegexOptions.IgnorePatternWhitespace;
            }
            if (_options.IndexOf('s') != -1)
            {
                options |= RegexOptions.Singleline;
            }
            return new Regex(_pattern, options);
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            var escaped = (_pattern == "") ? "(?:)" :_pattern.Replace("/", @"\/");
            return string.Format("/{0}/{1}", escaped, _options);
        }
    }
}
