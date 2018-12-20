/* Original work:
 *   Copyright 2016 .NET Foundation and Contributors
 * 
 *   The MIT License (MIT)
 *   
 *   Copyright (c) .NET Foundation and Contributors
 *   
 *   All rights reserved.
 *   
 *   Permission is hereby granted, free of charge, to any person obtaining a copy
 *   of this software and associated documentation files (the "Software"), to deal
 *   in the Software without restriction, including without limitation the rights
 *   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *   copies of the Software, and to permit persons to whom the Software is
 *   furnished to do so, subject to the following conditions:
 *   
 *   The above copyright notice and this permission notice shall be included in all
 *   copies or substantial portions of the Software.
 *   
 *   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *   SOFTWARE.
 * 
 * Modified work: 
 *   Copyright 2018–present MongoDB Inc.
 *
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *   
 *   http://www.apache.org/licenses/LICENSE-2.0
 *   
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */

using System;

namespace MongoDB.Driver.Core.Authentication.Vendored
{
    // .NET Standard 1.5 includes this class, as does .NET Framework >= 4.6
#if NET452
   // Strongly typed string representing the name of a hash algorithm.
    // Open ended to allow extensibility while giving the discoverable feel of an enum for common values.

    /// <summary>
    /// Specifies the name of a cryptographic hash algorithm.
    /// </summary>
    /// <remarks>
    /// Asymmetric Algorithms implemented using Microsoft's CNG (Cryptography Next Generation) API
    /// will interpret the underlying string value as a CNG algorithm identifier: 
    ///   * https://msdn.microsoft.com/en-us/library/windows/desktop/aa375534(v=vs.85).aspx
    ///
    /// As with CNG, the names are case-sensitive. 
    /// 
    /// Asymmetric Algorithms implemented using other technologies:
    ///    * Must recognize at least "MD5", "SHA1", "SHA256", "SHA384", and "SHA512".
    ///    * Should recognize additional CNG IDs for any other hash algorithms that they also support.
    /// </remarks>
    public struct HashAlgorithmName : IEquatable<HashAlgorithmName>
    {
        // Returning a new instance every time is free here since HashAlgorithmName is a struct with
        // a single string field. The optimized codegen should be equivalent to return "MD5".

        /// <summary>
        /// Gets a <see cref="HashAlgorithmName" /> representing "MD5"
        /// </summary>
        public static HashAlgorithmName MD5 { get { return new HashAlgorithmName("MD5"); } }

        /// <summary>
        /// Gets a <see cref="HashAlgorithmName" /> representing "SHA1"
        /// </summary>
        public static HashAlgorithmName SHA1 { get { return new HashAlgorithmName("SHA1"); } }

        /// <summary>
        /// Gets a <see cref="HashAlgorithmName" /> representing "SHA256"
        /// </summary>
        public static HashAlgorithmName SHA256 { get { return new HashAlgorithmName("SHA256"); } }

        /// <summary>
        /// Gets a <see cref="HashAlgorithmName" /> representing "SHA384"
        /// </summary>
        public static HashAlgorithmName SHA384 { get { return new HashAlgorithmName("SHA384"); } }

        /// <summary>
        /// Gets a <see cref="HashAlgorithmName" /> representing "SHA512"
        /// </summary>
        public static HashAlgorithmName SHA512 { get { return new HashAlgorithmName("SHA512"); } }

        private readonly string _name;

        /// <summary>
        /// Gets a <see cref="HashAlgorithmName" /> representing a custom name.
        /// </summary>
        /// <param name="name">The custom hash algorithm name.</param>
        public HashAlgorithmName(string name)
        {
            // Note: No validation because we have to deal with default(HashAlgorithmName) regardless.
            _name = name;
        }

        /// <summary>
        /// Gets the underlying string representation of the algorithm name. 
        /// </summary>
        /// <remarks>
        /// May be null or empty to indicate that no hash algorithm is applicable.
        /// </remarks>
        public string Name
        {
            get { return _name; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _name ?? String.Empty;
        }
        
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is HashAlgorithmName && Equals((HashAlgorithmName)obj);
        }

        /// <inheritdoc/>
        public bool Equals(HashAlgorithmName other)
        {
            // NOTE: intentionally ordinal and case sensitive, matches CNG.
            return _name == other._name;
        }
        
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _name == null ? 0 : _name.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of the == operator.
        /// </returns>
        public static bool operator ==(HashAlgorithmName left, HashAlgorithmName right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of the != operator.
        /// </returns>
        public static bool operator !=(HashAlgorithmName left, HashAlgorithmName right)
        {
            return !(left == right);
        }
    }
#endif    
}