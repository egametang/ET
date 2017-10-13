/* Copyright 2013-2015 MongoDB Inc.
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
using MongoDB.Shared;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// Represents a semantic version number.
    /// </summary>
    public class SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>
    {
        // fields
        private readonly int _major;
        private readonly int _minor;
        private readonly int _patch;
        private readonly string _preRelease;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticVersion"/> class.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch version.</param>
        public SemanticVersion(int major, int minor, int patch)
            : this(major, minor, patch, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticVersion"/> class.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch version.</param>
        /// <param name="preRelease">The pre release version.</param>
        public SemanticVersion(int major, int minor, int patch, string preRelease)
        {
            _major = Ensure.IsGreaterThanOrEqualToZero(major, nameof(major));
            _minor = Ensure.IsGreaterThanOrEqualToZero(minor, nameof(minor));
            _patch = Ensure.IsGreaterThanOrEqualToZero(patch, nameof(patch));
            _preRelease = preRelease; // can be null
        }

        // properties
        /// <summary>
        /// Gets the major version.
        /// </summary>
        /// <value>
        /// The major version.
        /// </value>
        public int Major
        {
            get { return _major; }
        }

        /// <summary>
        /// Gets the minor version.
        /// </summary>
        /// <value>
        /// The minor version.
        /// </value>
        public int Minor
        {
            get { return _minor; }
        }

        /// <summary>
        /// Gets the patch version.
        /// </summary>
        /// <value>
        /// The patch version.
        /// </value>
        public int Patch
        {
            get { return _patch; }
        }

        /// <summary>
        /// Gets the pre release version.
        /// </summary>
        /// <value>
        /// The pre release version.
        /// </value>
        public string PreRelease
        {
            get { return _preRelease; }
        }

        // methods
        /// <inheritdoc/>
        public int CompareTo(SemanticVersion other)
        {
            if (other == null)
            {
                return 1;
            }

            var result = _major.CompareTo(other._major);
            if (result != 0)
            {
                return result;
            }

            result = _minor.CompareTo(other._minor);
            if (result != 0)
            {
                return result;
            }

            result = _patch.CompareTo(other._patch);
            if (result != 0)
            {
                return result;
            }

            if (_preRelease == null && other._preRelease == null)
            {
                return 0;
            }
            else if (_preRelease == null)
            {
                return 1;
            }
            else if (other._preRelease == null)
            {
                return -1;
            }

            return _preRelease.CompareTo(other._preRelease);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as SemanticVersion);
        }

        /// <inheritdoc/>
        public bool Equals(SemanticVersion other)
        {
            return CompareTo(other) == 0;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (_preRelease == null)
            {
                return string.Format("{0}.{1}.{2}", _major, _minor, _patch);
            }
            else
            {
                return string.Format("{0}.{1}.{2}-{3}", _major, _minor, _patch, _preRelease);
            }
        }

        /// <summary>
        /// Parses a string representation of a semantic version.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <returns>A semantic version.</returns>
        public static SemanticVersion Parse(string value)
        {
            SemanticVersion result;
            if (TryParse(value, out result))
            {
                return result;
            }

            throw new FormatException(string.Format(
                "Invalid SemanticVersion string: '{0}'.", value));
        }

        /// <summary>
        /// Tries to parse a string representation of a semantic version.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if the string representation was parsed successfully; otherwise false.</returns>
        public static bool TryParse(string value, out SemanticVersion result)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var pattern = @"(?<major>\d+)\.(?<minor>\d+)(\.(?<patch>\d+)(-(?<preRelease>.+))?)?";
                var match = Regex.Match((string)value, pattern);
                if (match.Success)
                {
                    var major = int.Parse(match.Groups["major"].Value);
                    var minor = int.Parse(match.Groups["minor"].Value);
                    var patch = match.Groups["patch"].Success ? int.Parse(match.Groups["patch"].Value) : 0;
                    var preRelease = match.Groups["preRelease"].Success ? match.Groups["preRelease"].Value : null;

                    result = new SemanticVersion(major, minor, patch, preRelease);
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Determines whether two specified semantic versions have the same value.
        /// </summary>
        /// <param name="a">The first semantic version to compare, or null.</param>
        /// <param name="b">The second semantic version to compare, or null.</param>
        /// <returns>
        /// True if the value of a is the same as the value of b; otherwise false.
        /// </returns>
        public static bool operator ==(SemanticVersion a, SemanticVersion b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return object.ReferenceEquals(b, null);
            }

            return a.CompareTo(b) == 0;
        }

        /// <summary>
        /// Determines whether two specified semantic versions have different values.
        /// </summary>
        /// <param name="a">The first semantic version to compare, or null.</param>
        /// <param name="b">The second semantic version to compare, or null.</param>
        /// <returns>
        /// True if the value of a is different from the value of b; otherwise false.
        /// </returns>
        public static bool operator !=(SemanticVersion a, SemanticVersion b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines whether the first specified SemanticVersion is greater than the second specified SemanticVersion.
        /// </summary>
        /// <param name="a">The first semantic version to compare, or null.</param>
        /// <param name="b">The second semantic version to compare, or null.</param>
        /// <returns>
        /// True if the value of a is greater than b; otherwise false.
        /// </returns>
        public static bool operator >(SemanticVersion a, SemanticVersion b)
        {
            if (a == null)
            {
                if (b == null)
                {
                    return true;
                }

                return false;
            }

            return a.CompareTo(b) > 0;
        }

        /// <summary>
        /// Determines whether the first specified SemanticVersion is greater than or equal to the second specified SemanticVersion.
        /// </summary>
        /// <param name="a">The first semantic version to compare, or null.</param>
        /// <param name="b">The second semantic version to compare, or null.</param>
        /// <returns>
        /// True if the value of a is greater than or equal to b; otherwise false.
        /// </returns>
        public static bool operator >=(SemanticVersion a, SemanticVersion b)
        {
            return !(a < b);
        }

        /// <summary>
        /// Determines whether the first specified SemanticVersion is less than the second specified SemanticVersion.
        /// </summary>
        /// <param name="a">The first semantic version to compare, or null.</param>
        /// <param name="b">The second semantic version to compare, or null.</param>
        /// <returns>
        /// True if the value of a is less than b; otherwise false.
        /// </returns>
        public static bool operator <(SemanticVersion a, SemanticVersion b)
        {
            return b > a;
        }

        /// <summary>
        /// Determines whether the first specified SemanticVersion is less than or equal to the second specified SemanticVersion.
        /// </summary>
        /// <param name="a">The first semantic version to compare, or null.</param>
        /// <param name="b">The second semantic version to compare, or null.</param>
        /// <returns>
        /// True if the value of a is less than or equal to b; otherwise false.
        /// </returns>
        public static bool operator <=(SemanticVersion a, SemanticVersion b)
        {
            return !(b < a);
        }
    }
}