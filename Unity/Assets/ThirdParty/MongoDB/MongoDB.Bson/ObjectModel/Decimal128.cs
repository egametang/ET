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
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a Decimal128 value.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public struct Decimal128 : IConvertible, IComparable<Decimal128>, IEquatable<Decimal128>
    {
        #region static
        // private constants
        private const short __exponentMax = 6111;
        private const short __exponentMin = -6176;
        private const short __exponentBias = 6176;
        private const short __maxSignificandDigits = 34;

        // private static fields
        private static readonly UInt128 __maxSignificand = UInt128.Parse("9999999999999999999999999999999999");
        private static readonly Decimal128 __maxValue = Decimal128.Parse("9999999999999999999999999999999999E+6111");
        private static readonly Decimal128 __minValue = Decimal128.Parse("-9999999999999999999999999999999999E+6111");

        // public static properties
        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        public static Decimal128 MaxValue =>
            __maxValue;

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        public static Decimal128 MinValue =>
            __minValue;

        /// <summary>
        /// Represents negative infinity.
        /// </summary>
        public static Decimal128 NegativeInfinity =>
            new Decimal128(Flags.NegativeInfinity, 0);

        /// <summary>
        /// Represents one.
        /// </summary>
        public static Decimal128 One =>
            new Decimal128(0, 1);

        /// <summary>
        /// Represents positive infinity.
        /// </summary>
        public static Decimal128 PositiveInfinity =>
            new Decimal128(Flags.PositiveInfinity, 0);

        /// <summary>
        /// Represents a value that is not a number.
        /// </summary>
        public static Decimal128 QNaN =>
            new Decimal128(Flags.QNaN, 0);

        /// <summary>
        /// Represents a value that is not a number and raises errors when used in calculations.
        /// </summary>
        public static Decimal128 SNaN =>
            new Decimal128(Flags.SNaN, 0);

        /// <summary>
        /// Represents zero.
        /// </summary>
        public static Decimal128 Zero =>
            new Decimal128(0, 0);

        // public static operators
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Decimal128 lhs, Decimal128 rhs)
        {
            if (Decimal128.IsNaN(lhs) || Decimal128.IsNaN(rhs))
            {
                return false;
            }
            else
            {
                return lhs.Equals(rhs);
            }
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Decimal128 lhs, Decimal128 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Returns a value indicating whether a specified Decimal128 is greater than another specified Decimal128.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        /// <returns>
        /// true if x &gt; y; otherwise, false.
        /// </returns>
        public static bool operator >(Decimal128 x, Decimal128 y)
        {
            return Decimal128.Compare(x, y) > 0;
        }

        /// <summary>
        /// Returns a value indicating whether a specified Decimal128 is greater than or equal to another another specified Decimal128.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        /// <returns>
        /// true if x &gt;= y; otherwise, false.
        /// </returns>
        public static bool operator >=(Decimal128 x, Decimal128 y)
        {
            return Decimal128.Compare(x, y) >= 0;
        }

        /// <summary>
        /// Returns a value indicating whether a specified Decimal128 is less than another specified Decimal128.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        /// <returns>
        /// true if x &lt; y; otherwise, false.
        /// </returns>
        public static bool operator <(Decimal128 x, Decimal128 y)
        {
            return Decimal128.Compare(x, y) < 0;
        }

        /// <summary>
        /// Returns a value indicating whether a specified Decimal128 is less than or equal to another another specified Decimal128.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        /// <returns>
        /// true if x &lt;= y; otherwise, false.
        /// </returns>
        public static bool operator <=(Decimal128 x, Decimal128 y)
        {
            return Decimal128.Compare(x, y) <= 0;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Byte"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator byte(Decimal128 value)
        {
            return ToByte(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="char"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator char(Decimal128 value)
        {
            return (char)ToUInt16(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Decimal"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator decimal(Decimal128 value)
        {
            return ToDecimal(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Byte"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(byte value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Decimal"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(decimal value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="double"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Decimal128(double value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="float"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Decimal128(float value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(int value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int64"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(long value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.SByte"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator Decimal128(sbyte value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int16"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Decimal128(short value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt32"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator Decimal128(uint value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt16"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator Decimal128(ushort value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static implicit operator Decimal128(ulong value)
        {
            return new Decimal128(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="double"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator double(Decimal128 value)
        {
            return Decimal128.ToDouble(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator float(Decimal128 value)
        {
            return Decimal128.ToSingle(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator int(Decimal128 value)
        {
            return ToInt32(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Int64"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator long(Decimal128 value)
        {
            return ToInt64(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.SByte"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator sbyte(Decimal128 value)
        {
            return ToSByte(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.Int16"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator short(Decimal128 value)
        {
            return ToInt16(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.UInt32"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator uint(Decimal128 value)
        {
            return ToUInt32(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator ulong(Decimal128 value)
        {
            return ToUInt64(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Decimal128"/> to <see cref="System.UInt16"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [CLSCompliant(false)]
        public static explicit operator ushort(Decimal128 value)
        {
            return ToUInt16(value);
        }

        // public static methods
        /// <summary>
        /// Compares two specified Decimal128 values and returns an integer that indicates whether the first value
        /// is greater than, less than, or equal to the second value.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        /// <returns>Less than zero if x &lt; y, zero if x == y, and greater than zero if x &gt; y.</returns>
        public static int Compare(Decimal128 x, Decimal128 y)
        {
            return Decimal128Comparer.Instance.Compare(x, y);
        }

        /// <summary>
        /// Determines whether the specified Decimal128 instances are considered equal.
        /// </summary>
        /// <param name="x">The first Decimal128 object to compare.</param>
        /// <param name="y">The second Decimal128 object to compare.</param>
        /// <returns>True if the objects are considered equal; otherwise false. If both x and y are null, the method returns true.</returns>
        public static bool Equals(Decimal128 x, Decimal128 y)
        {
            return Decimal128.Compare(x, y) == 0;
        }

        /// <summary>
        /// Creates a new Decimal128 value from its components.
        /// </summary>
        /// <param name="isNegative">if set to <c>true</c> [is negative].</param>
        /// <param name="exponent">The exponent.</param>
        /// <param name="significandHighBits">The signficand high bits.</param>
        /// <param name="significandLowBits">The significand low bits.</param>
        /// <returns>A Decimal128 value.</returns>
        [CLSCompliant(false)]
        public static Decimal128 FromComponents(bool isNegative, short exponent, ulong significandHighBits, ulong significandLowBits)
        {
            return FromComponents(isNegative, exponent, new UInt128(significandHighBits, significandLowBits));
        }

        /// <summary>
        /// Creates a new Decimal128 value from the IEEE encoding bits.
        /// </summary>
        /// <param name="highBits">The high bits.</param>
        /// <param name="lowBits">The low bits.</param>
        /// <returns>A Decimal128 value.</returns>
        [CLSCompliant(false)]
        public static Decimal128 FromIEEEBits(ulong highBits, ulong lowBits)
        {
            return new Decimal128(MapIEEEHighBitsToDecimal128HighBits(highBits), lowBits);
        }

        /// <summary>
        /// Gets the exponent of a Decimal128 value.
        /// </summary>
        /// <param name="d">The Decimal128 value.</param>
        /// <returns>The exponent.</returns>
        public static short GetExponent(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                return MapDecimal128BiasedExponentToExponent((short)((d._highBits & Flags.FirstFormExponentBits) >> 49));
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return MapDecimal128BiasedExponentToExponent((short)((d._highBits & Flags.SecondFormExponentBits) >> 47));
            }
            else
            {
                throw new InvalidOperationException("GetExponent cannot be called for Infinity or NaN.");
            }
        }

        /// <summary>
        /// Gets the high bits of the significand of a Decimal128 value.
        /// </summary>
        /// <param name="d">The Decimal128 value.</param>
        /// <returns>The high bits of the significand.</returns>
        [CLSCompliant(false)]
        public static ulong GetSignificandHighBits(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                return d._highBits & Flags.FirstFormSignificandBits;
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new InvalidOperationException("GetSignificandHighBits cannot be called for Infinity or NaN.");
            }
        }

        /// <summary>
        /// Gets the high bits of the significand of a Decimal128 value.
        /// </summary>
        /// <param name="d">The Decimal128 value.</param>
        /// <returns>The high bits of the significand.</returns>
        [CLSCompliant(false)]
        public static ulong GetSignificandLowBits(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                return d._lowBits;
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new InvalidOperationException("GetSignificandLowBits cannot be called for Infinity or NaN.");
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to negative or positive infinity.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> evaluates to negative or positive infinity; otherwise, false.</returns>
        public static bool IsInfinity(Decimal128 d) => Flags.IsInfinity(d._highBits);

        /// <summary>
        /// Returns a value indicating whether the specified number is not a number.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> is not a number; otherwise, false.</returns>
        public static bool IsNaN(Decimal128 d) => Flags.IsNaN(d._highBits);

        /// <summary>
        /// Returns a value indicating whether the specified number is negative.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> is negative; otherwise, false.</returns>
        public static bool IsNegative(Decimal128 d) => Flags.IsNegative(d._highBits);

        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to negative infinity.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> evaluates to negative infinity; otherwise, false.</returns>
        public static bool IsNegativeInfinity(Decimal128 d) => Flags.IsNegativeInfinity(d._highBits);

        /// <summary>
        /// Returns a value indicating whether the specified number evaluates to positive infinity.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> evaluates to positive infinity; otherwise, false.</returns>
        public static bool IsPositiveInfinity(Decimal128 d) => Flags.IsPositiveInfinity(d._highBits);

        /// <summary>
        /// Returns a value indicating whether the specified number is a quiet not a number.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> is a quiet not a number; otherwise, false.</returns>
        public static bool IsQNaN(Decimal128 d) => Flags.IsQNaN(d._highBits);

        /// <summary>
        /// Returns a value indicating whether the specified number is a signaled not a number.
        /// </summary>
        /// <param name="d">A 128-bit decimal.</param>
        /// <returns>true if <paramref name="d" /> is a signaled not a number; otherwise, false.</returns>
        public static bool IsSNaN(Decimal128 d) => Flags.IsSNaN(d._highBits);

        /// <summary>
        /// Gets a value indicating whether this instance is zero.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is zero; otherwise, <c>false</c>.
        /// </value>
        public static bool IsZero(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits) && GetSignificand(d).Equals(UInt128.Zero))
            {
                return true;
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                // all second form values are invalid representations and are interpreted as zero
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Negates the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>The result of multiplying the value by negative one.</returns>
        public static Decimal128 Negate(Decimal128 x)
        {
            return new Decimal128(x._highBits ^ Flags.SignBit, x._lowBits);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Decimal128" /> equivalent.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <returns>
        /// The equivalent to the number contained in <paramref name="s" />.
        /// </returns>
        public static Decimal128 Parse(string s)
        {
            Decimal128 value;
            if (!TryParse(s, out value))
            {
                throw new FormatException($"{s} is not a valid Decimal128.");
            }

            return value;
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 8-bit unsigned integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 8-bit unsigned integer equivalent to <paramref name="d" />.</returns>
        public static byte ToByte(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                ulong value;
                if (Decimal128.TryTruncateToUInt64(d, 0, byte.MaxValue, out value))
                {
                    return (byte)value;
                }
                else
                {
                    throw new OverflowException("Value is too large or too small to be converted to a Byte.");
                }
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new OverflowException("Infinity or NaN cannot be converted to a Byte.");
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent <see cref="decimal"/>.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A <see cref="decimal"/> equivalent to <paramref name="d" />.</returns>
        public static decimal ToDecimal(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                var exponent = Decimal128.GetExponent(d);

                // try to get the exponent within the range of 0 to -28
                if (exponent > 0)
                {
                    d = Decimal128.DecreaseExponent(d, 0);
                    exponent = Decimal128.GetExponent(d);
                }
                else if (exponent < -28)
                {
                    d = Decimal128.IncreaseExponent(d, -28);
                    exponent = Decimal128.GetExponent(d);
                }

                // try to get the significand to have zeros for the high order 32 bits
                var significand = Decimal128.GetSignificand(d);
                while ((significand.High >> 32) != 0)
                {
                    uint remainder;
                    var significandDividedBy10 = UInt128.Divide(significand, (uint)10, out remainder);
                    if (remainder != 0)
                    {
                        break;
                    }
                    exponent += 1;
                    significand = significandDividedBy10;
                }


                if (exponent < -28 || exponent > 0 || (significand.High >> 32) != 0)
                {
                    throw new OverflowException("Value is too large or too small to be converted to a Decimal.");
                }

                var lo = (int)significand.Low;
                var mid = (int)(significand.Low >> 32);
                var hi = (int)significand.High;
                var isNegative = Decimal128.IsNegative(d);
                var scale = (byte)(-exponent);

                return new decimal(lo, mid, hi, isNegative, scale);
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return Decimal.Zero;
            }
            else
            {
                throw new OverflowException("Infinity or NaN cannot be converted to Decimal.");
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent <see cref="double"/>.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A <see cref="double"/> equivalent to <paramref name="d" />.</returns>
        public static double ToDouble(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                // TODO: implement this more efficiently
                var stringValue = d.ToString();
                return double.Parse(stringValue);
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0.0;
            }
            else if (Flags.IsPositiveInfinity(d._highBits))
            {
                return double.PositiveInfinity;
            }
            else if (Flags.IsNegativeInfinity(d._highBits))
            {
                return double.NegativeInfinity;
            }
            else
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 16-bit signed integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 16-bit signed integer equivalent to <paramref name="d" />.</returns>
        public static short ToInt16(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                var maxNegativeValue = (ulong)short.MaxValue + 1;
                ulong value;
                if (Decimal128.TryTruncateToUInt64(d, maxNegativeValue, (ulong)short.MaxValue, out value))
                {
                    return Decimal128.IsNegative(d) ? (value == maxNegativeValue ? short.MinValue : (short )(-(short)value)) : (short)value;
                }
                else
                {
                    throw new OverflowException("Value is too large or too small to be converted to an Int16.");
                }
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new OverflowException("Infinity or NaN cannot be converted to an Int16.");
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 32-bit signed integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 32-bit signed integer equivalent to <paramref name="d" />.</returns>
        public static int ToInt32(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                var maxNegativeValue = (ulong)int.MaxValue + 1;
                ulong value;
                if (Decimal128.TryTruncateToUInt64(d, maxNegativeValue, int.MaxValue, out value))
                {
                    return Decimal128.IsNegative(d) ? (value == maxNegativeValue ? int.MinValue : -(int)value) : (int)value;
                }
                else
                {
                    throw new OverflowException("Value is too large or too small to be converted to an Int32.");
                }
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new OverflowException("Infinity or NaN cannot be converted to an Int32.");
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 64-bit signed integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 64-bit signed integer equivalent to <paramref name="d" />.</returns>
        public static long ToInt64(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                ulong maxNegativeValue = (ulong)long.MaxValue + 1;
                ulong value;
                if (Decimal128.TryTruncateToUInt64(d, maxNegativeValue, long.MaxValue, out value))
                {
                    return Decimal128.IsNegative(d) ? (value == maxNegativeValue ? long.MinValue : -(long)value) : (long)value;
                }
                else
                {
                    throw new OverflowException("Value is too large or too small to be converted to an Int64.");
                }
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new OverflowException("Infinity or NaN cannot be converted to an Int64.");
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 8-bit signed integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 8-bit signed integer equivalent to <paramref name="d" />.</returns>
        [CLSCompliant(false)]
        public static sbyte ToSByte(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                ulong maxNegativeValue = (ulong)sbyte.MaxValue + 1;
                ulong value;
                if (Decimal128.TryTruncateToUInt64(d, maxNegativeValue, (ulong)sbyte.MaxValue, out value))
                {
                    return Decimal128.IsNegative(d) ? (value == maxNegativeValue ? sbyte.MinValue : (sbyte)(-(sbyte)value)) : (sbyte)value;
                }
                else
                {
                    throw new OverflowException("Value is too large or too small to be converted to an SByte.");
                }
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new OverflowException("Infinity or NaN cannot be converted to an SByte.");
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent <see cref="float"/>.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A <see cref="float"/> equivalent to <paramref name="d" />.</returns>
        public static float ToSingle(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                // TODO: implement this more efficiently
                var stringValue = d.ToString();
                return float.Parse(stringValue);
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return (float)0.0;
            }
            else if (Flags.IsPositiveInfinity(d._highBits))
            {
                return float.PositiveInfinity;
            }
            else if (Flags.IsNegativeInfinity(d._highBits))
            {
                return float.NegativeInfinity;
            }
            else
            {
                return float.NaN;
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 16-bit unsigned integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 16-bit unsigned integer equivalent to <paramref name="d" />.</returns>
        [CLSCompliant(false)]
        public static ushort ToUInt16(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                ulong value;
                if (Decimal128.TryTruncateToUInt64(d, 0, ushort.MaxValue, out value))
                {
                    return (ushort)value;
                }
                else
                {
                    throw new OverflowException("Value is too large or too small to be converted to a UInt16.");
                }
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new OverflowException("Infinity or NaN cannot be converted to a UInt16.");
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 32-bit unsigned integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 32-bit unsigned integer equivalent to <paramref name="d" />.</returns>
        [CLSCompliant(false)]
        public static uint ToUInt32(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                ulong value;
                if (Decimal128.TryTruncateToUInt64(d, 0, uint.MaxValue, out value))
                {
                    return (uint)value;
                }
                else
                {
                    throw new OverflowException("Value is too large or too small to be converted to a UInt32.");
                }
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new OverflowException("Infinity or NaN cannot be converted to a UInt32.");
            }
        }

        /// <summary>
        /// Converts the value of the specified <see cref="Decimal128"/> to the equivalent 64-bit unsigned integer.
        /// </summary>
        /// <param name="d">The number to convert.</param>
        /// <returns>A 64-bit unsigned integer equivalent to <paramref name="d" />.</returns>
        [CLSCompliant(false)]
        public static ulong ToUInt64(Decimal128 d)
        {
            if (Flags.IsFirstForm(d._highBits))
            {
                ulong value;
                if (Decimal128.TryTruncateToUInt64(d, 0, ulong.MaxValue, out value))
                {
                    return value;
                }
                else
                {
                    throw new OverflowException("Value is too large or too small to be converted to a UInt64.");
                }
            }
            else if (Flags.IsSecondForm(d._highBits))
            {
                return 0;
            }
            else
            {
                throw new OverflowException("Infinity or NaN cannot be converted to a UInt64.");
            }
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Decimal128" /> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">The string representation of the number to convert.</param>
        /// <param name="result">When this method returns, contains the <see cref="Decimal128" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or is zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is null, is not a number in a valid format, or represents a number less than the min value or greater than the max value. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if <paramref name="s" /> was converted successfully; otherwise, false.
        /// </returns>
        public static bool TryParse(string s, out Decimal128 result)
        {
            if (s == null || s.Length == 0)
            {
                result = default(Decimal128);
                return false;
            }

            const string pattern =
                @"^(?<sign>[+-])?" +
                @"(?<significand>\d+([.]\d*)?|[.]\d+)" +
                @"(?<exponent>[eE](?<exponentSign>[+-])?(?<exponentDigits>\d+))?$";

            var match = Regex.Match(s, pattern);
            if (!match.Success)
            {
                if (s.Equals("Inf", StringComparison.OrdinalIgnoreCase) || s.Equals("Infinity", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("+Inf", StringComparison.OrdinalIgnoreCase) || s.Equals("+Infinity", StringComparison.OrdinalIgnoreCase))
                {
                    result = Decimal128.PositiveInfinity;
                    return true;
                }

                if (s.Equals("-Inf", StringComparison.OrdinalIgnoreCase) || s.Equals("-Infinity", StringComparison.OrdinalIgnoreCase))
                {
                    result = Decimal128.NegativeInfinity;
                    return true;
                }

                if (s.Equals("NaN", StringComparison.OrdinalIgnoreCase) || s.Equals("-NaN", StringComparison.OrdinalIgnoreCase))
                {
                    result = Decimal128.QNaN;
                    return true;
                }

                result = default(Decimal128);
                return false;
            }

            var isNegative = match.Groups["sign"].Value == "-";

            var exponent = 0;
            if (match.Groups["exponent"].Length != 0)
            {
                if (!int.TryParse(match.Groups["exponentDigits"].Value, out exponent))
                {
                    result = default(Decimal128);
                    return false;
                }
                if (match.Groups["exponentSign"].Value == "-")
                {
                    exponent = -exponent;
                }
            }

            var significandString = match.Groups["significand"].Value;

            int decimalPointIndex;
            if ((decimalPointIndex = significandString.IndexOf('.')) != -1)
            {
                exponent -= significandString.Length - (decimalPointIndex + 1);
                significandString = significandString.Substring(0, decimalPointIndex) + significandString.Substring(decimalPointIndex + 1);
            }

            significandString = RemoveLeadingZeroes(significandString);
            significandString = ClampOrRound(ref exponent, significandString);

            if (exponent > __exponentMax || exponent < __exponentMin)
            {
                result = default(Decimal128);
                return false;
            }
            if (significandString.Length > 34)
            {
                result = default(Decimal128);
                return false;
            }

            var significand = UInt128.Parse(significandString);

            result = Decimal128.FromComponents(isNegative, (short)exponent, significand);
            return true;
        }

        // private static methods
        private static string ClampOrRound(ref int exponent, string significandString)
        {
            if (exponent > __exponentMax)
            {
                if (significandString == "0")
                {
                    // since significand is zero simply use the largest possible exponent
                    exponent = __exponentMax;
                }
                else
                {
                    // use clamping to bring the exponent into range
                    var numberOfTrailingZeroesToAdd = exponent - __exponentMax;
                    var digitsAvailable = 34 - significandString.Length;
                    if (numberOfTrailingZeroesToAdd <= digitsAvailable)
                    {
                        exponent = __exponentMax;
                        significandString = significandString + new string('0', numberOfTrailingZeroesToAdd);
                    }
                }
            }
            else if (exponent < __exponentMin)
            {
                if (significandString == "0")
                {
                    // since significand is zero simply use the smallest possible exponent
                    exponent = __exponentMin;
                }
                else
                {
                    // use exact rounding to bring the exponent into range
                    var numberOfTrailingZeroesToRemove = __exponentMin - exponent;
                    if (numberOfTrailingZeroesToRemove < significandString.Length)
                    {
                        var trailingDigits = significandString.Substring(significandString.Length - numberOfTrailingZeroesToRemove);
                        if (Regex.IsMatch(trailingDigits, "^0+$"))
                        {
                            exponent = __exponentMin;
                            significandString = significandString.Substring(0, significandString.Length - numberOfTrailingZeroesToRemove);
                        }
                    }
                }
            }
            else if (significandString.Length > 34)
            {
                // use exact rounding to reduce significand to 34 digits
                var numberOfTrailingZeroesToRemove = significandString.Length - 34;
                if (exponent + numberOfTrailingZeroesToRemove <= __exponentMax)
                {
                    var trailingDigits = significandString.Substring(significandString.Length - numberOfTrailingZeroesToRemove);
                    if (Regex.IsMatch(trailingDigits, "^0+$"))
                    {
                        exponent += numberOfTrailingZeroesToRemove;
                        significandString = significandString.Substring(0, significandString.Length - numberOfTrailingZeroesToRemove);
                    }
                }
            }

            return significandString;
        }

        private static Decimal128 DecreaseExponent(Decimal128 x, short goal)
        {
            if (Decimal128.IsZero(x))
            {
                // return a zero with the desired exponent
                return Decimal128.FromComponents(Decimal128.IsNegative(x), goal, UInt128.Zero);
            }

            var exponent = GetExponent(x);
            var significand = GetSignificand(x);
            while (exponent > goal)
            {
                var significandTimes10 = UInt128.Multiply(significand, (uint)10);
                if (significandTimes10.CompareTo(Decimal128.__maxSignificand) <= 0)
                {
                    break;
                }
                exponent -= 1;
                significand = significandTimes10;
            }

            return Decimal128.FromComponents(Decimal128.IsNegative(x), exponent, significand);
        }

        private static Decimal128 FromComponents(bool isNegative, short exponent, UInt128 significand)
        {
            if (exponent < __exponentMin || exponent > __exponentMax)
            {
                throw new ArgumentOutOfRangeException(nameof(exponent));
            }
            if (significand.CompareTo(__maxSignificand) > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(significand));
            }

            var biasedExponent = MapExponentToDecimal128BiasedExponent(exponent);
            var highBits = ((ulong)biasedExponent << 49) | significand.High;
            if (isNegative)
            {
                highBits = Flags.SignBit | highBits;
            }

            return new Decimal128(highBits, significand.Low);
        }

        private static UInt128 GetSignificand(Decimal128 d)
        {
            return new UInt128(GetSignificandHighBits(d), GetSignificandLowBits(d));
        }

        private static Decimal128 IncreaseExponent(Decimal128 x, short goal)
        {
            if (Decimal128.IsZero(x))
            {
                // return a zero with the desired exponent
                return Decimal128.FromComponents(Decimal128.IsNegative(x), goal, UInt128.Zero);
            }

            var exponent = GetExponent(x);
            var significand = GetSignificand(x);
            while (exponent < goal)
            {
                uint remainder;
                var significandDividedBy10 = UInt128.Divide(significand, (uint)10, out remainder);
                if (remainder != 0)
                {
                    break;
                }
                exponent += 1;
                significand = significandDividedBy10;
            }

            return Decimal128.FromComponents(Decimal128.IsNegative(x), exponent, significand);
        }

        private static short MapDecimal128BiasedExponentToExponent(short biasedExponent)
        {
            if (biasedExponent <= 6111)
            {
                return biasedExponent;
            }
            else
            {
                return (short)(biasedExponent - 12288);
            }
        }

        private static ulong MapDecimal128HighBitsToIEEEHighBits(ulong highBits)
        {
            // for Decimal128Bias from    0 to  6111: IEEEBias = Decimal128Bias + 6176
            // for Decimal128Bias from 6112 to 12287: IEEEBias = Decimal128Bias - 6112

            if (Flags.IsFirstForm(highBits))
            {
                var exponentBits = highBits & Flags.FirstFormExponentBits;
                if (exponentBits <= (6111L << 49))
                {
                    return highBits + (6176L << 49);
                }
                else
                {
                    return highBits - (6112L << 49);
                }
            }
            else if (Flags.IsSecondForm(highBits))
            {
                var exponentBits = highBits & Flags.SecondFormExponentBits;
                if (exponentBits <= (6111L << 47))
                {
                    return highBits + (6176L << 47);
                }
                else
                {
                    return highBits - (6112L << 47);
                }
            }
            else
            {
                return highBits;
            }
        }

        private static short MapExponentToDecimal128BiasedExponent(short exponent)
        {
            // internally we use a different bias than IEEE so that a Decimal128 struct filled with zero bytes is a true Decimal128 zero
            // Decimal128Bias is defined as:
            // exponents from     0 to 6111: biasedExponent = exponent
            // exponents from -6176 to   -1: biasedExponent = exponent + 12288

            if (exponent >= 0)
            {
                return exponent;
            }
            else
            {
                return (short)(exponent + 12288);
            }
        }

        private static ulong MapIEEEHighBitsToDecimal128HighBits(ulong highBits)
        {
            // for IEEEBias from    0 to  6175: Decimal128Bias = IEEEBias + 6112
            // for IEEEBias from 6176 to 12287: Decimal128Bias = IEEEBias - 6176

            if (Flags.IsFirstForm(highBits))
            {
                var exponentBits = highBits & Flags.FirstFormExponentBits;
                if (exponentBits <= (6175L << 49))
                {
                    return highBits + (6112L << 49);
                }
                else
                {
                    return highBits - (6176L << 49);
                }
            }
            else if (Flags.IsSecondForm(highBits))
            {
                var exponentBits = highBits & Flags.SecondFormExponentBits;
                if (exponentBits <= (6175L << 47))
                {
                    return highBits + (6112L << 47);
                }
                else
                {
                    return highBits - (6176L << 47);
                }
            }
            else
            {
                return highBits;
            }
        }

        private static string RemoveLeadingZeroes(string significandString)
        {
            if (significandString[0] == '0' && significandString.Length > 1)
            {
                significandString = Regex.Replace(significandString, "^0+", "");
                return significandString.Length == 0 ? "0" : significandString;
            }
            else
            {
                return significandString;
            }
        }
        #endregion

        // private fields
        private readonly ulong _highBits;
        private readonly ulong _lowBits;

        // constructors
        private Decimal128(ulong highBits, ulong lowBits)
        {
            _highBits = highBits;
            _lowBits = lowBits;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(decimal value)
        {
            var bits = decimal.GetBits(value);
            var isNegative = (bits[3] & 0x80000000) != 0;
            var scale = (short)((bits[3] & 0x00FF0000) >> 16);
            var exponent = (short)-scale;
            var significandHigh = (ulong)(uint)bits[2];
            var significandLow = ((ulong)(uint)bits[1] << 32) | (ulong)(uint)bits[0];

            _highBits = (isNegative ? Flags.SignBit : 0) | ((ulong)MapExponentToDecimal128BiasedExponent(exponent) << 49) | significandHigh;
            _lowBits = significandLow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(double value)
        {
            // TODO: implement this more efficiently
            var stringValue = value.ToString("G17");
            var decimal128Value = Decimal128.Parse(stringValue);
            _highBits = MapIEEEHighBitsToDecimal128HighBits(decimal128Value.GetIEEEHighBits());
            _lowBits = decimal128Value.GetIEEELowBits();

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(float value)
        {
            // TODO: implement this more efficiently
            var stringValue = value.ToString("G17");
            var decimal128Value = Decimal128.Parse(stringValue);
            _highBits = MapIEEEHighBitsToDecimal128HighBits(decimal128Value.GetIEEEHighBits());
            _lowBits = decimal128Value.GetIEEELowBits();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(int value)
        {
            if (value >= 0)
            {
                _highBits = 0;
                _lowBits = (ulong)value;
            }
            else
            {
                _highBits = Flags.SignBit;
                _lowBits = value == int.MinValue ? (ulong)int.MaxValue + 1 : (ulong)-value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Decimal128(long value)
        {
            if (value >= 0)
            {
                _highBits = 0;
                _lowBits = (ulong)value;
            }
            else
            {
                _highBits = Flags.SignBit;
                _lowBits = value == long.MinValue ? (ulong)long.MaxValue + 1 : (ulong)-value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        [CLSCompliant(false)]
        public Decimal128(uint value)
        {
            _highBits = 0;
            _lowBits = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal128"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        [CLSCompliant(false)]
        public Decimal128(ulong value)
        {
            _highBits = 0;
            _lowBits = value;
        }

        // public methods
        /// <inheritdoc />
        public int CompareTo(Decimal128 other)
        {
            return Decimal128.Compare(this, other);
        }

        /// <inheritdoc />
        public bool Equals(Decimal128 other)
        {
            return Decimal128.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Decimal128))
            {
                return false;
            }
            else
            {
                return Equals((Decimal128)obj);
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + _highBits.GetHashCode();
            hash = 37 * hash + _lowBits.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Gets the high order 64 bits of the binary representation of this instance.
        /// </summary>
        /// <returns>The high order 64 bits of the binary representation of this instance.</returns>
        [CLSCompliant(false)]
        public ulong GetIEEEHighBits()
        {
            return MapDecimal128HighBitsToIEEEHighBits(_highBits);
        }

        /// <summary>
        /// Gets the low order 64 bits of the binary representation of this instance.
        /// </summary>
        /// <returns>The low order 64 bits of the binary representation of this instance.</returns>
        [CLSCompliant(false)]
        public ulong GetIEEELowBits()
        {
            return _lowBits;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Flags.IsFirstForm(_highBits))
            {
                var exponent = GetExponent(this);
                var significand = GetSignificand(this);
                var coefficientString = significand.ToString();
                var adjustedExponent = exponent + coefficientString.Length - 1;

                string result;
                if (exponent > 0 || adjustedExponent < -6)
                {
                    result = ToStringWithExponentialNotation(coefficientString, adjustedExponent);
                }
                else
                {
                    result = ToStringWithoutExponentialNotation(coefficientString, exponent);
                }

                if (Flags.IsNegative(_highBits))
                {
                    result = "-" + result;
                }

                return result;
            }
            else if (Flags.IsSecondForm(_highBits))
            {
                // invalid representation treated as zero
                var exponent = GetExponent(this);
                if (exponent == 0)
                {
                    return Flags.IsNegative(_highBits) ? "-0" : "0";
                }
                else
                {
                    var exponentString = exponent.ToString(NumberFormatInfo.InvariantInfo);
                    if (exponent > 0)
                    {
                        exponentString = "+" + exponentString;
                    }
                    return (Flags.IsNegative(_highBits) ? "-0E" : "0E") + exponentString;
                }
            }
            else if (Flags.IsNegativeInfinity(_highBits))
            {
                return "-Infinity";
            }
            else if (Flags.IsPositiveInfinity(_highBits))
            {
                return "Infinity";
            }
            else
            {
                return "NaN";
            }
        }

        // explicit IConvertible implementation
        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return !(Decimal128.Equals(this, Decimal128.Zero) || Decimal128.IsNaN(this));
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Decimal128.ToByte(this);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException("Invalid cast from Decima128 to Char.");
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException("Invalid cast from Decima128 to DateTime.");
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Decimal128.ToDecimal(this);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Decimal128.ToDouble(this);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Decimal128.ToInt16(this);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Decimal128.ToInt32(this);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Decimal128.ToInt64(this);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Decimal128.ToSByte(this);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Decimal128.ToSingle(this);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return ToString();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            var convertible = (IConvertible)this;
            switch (Type.GetTypeCode(conversionType))
            {
                case TypeCode.Boolean: return convertible.ToBoolean(provider);
                case TypeCode.Byte: return convertible.ToByte(provider);
                case TypeCode.Char: return convertible.ToChar(provider);
                case TypeCode.DateTime: return convertible.ToDateTime(provider);
                case TypeCode.Decimal: return convertible.ToDecimal(provider);
                case TypeCode.Double: return convertible.ToDouble(provider);
                case TypeCode.Int16: return convertible.ToInt16(provider);
                case TypeCode.Int32: return convertible.ToInt32(provider);
                case TypeCode.Int64: return convertible.ToInt64(provider);
                case TypeCode.SByte: return convertible.ToSByte(provider);
                case TypeCode.Single: return convertible.ToSingle(provider);
                case TypeCode.String: return convertible.ToString(provider);
                case TypeCode.UInt16: return convertible.ToUInt16(provider);
                case TypeCode.UInt32: return convertible.ToUInt32(provider);
                case TypeCode.UInt64: return convertible.ToUInt64(provider);
                default: throw new InvalidCastException();
            }
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Decimal128.ToUInt16(this);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Decimal128.ToUInt32(this);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Decimal128.ToUInt64(this);
        }

        // private methods
        private string ToStringWithExponentialNotation(string coefficientString, int adjustedExponent)
        {
            if (coefficientString.Length > 1)
            {
                coefficientString = coefficientString.Substring(0, 1) + "." + coefficientString.Substring(1);
            }
            var exponentString = adjustedExponent.ToString(NumberFormatInfo.InvariantInfo);
            if (adjustedExponent >= 0)
            {
                exponentString = "+" + exponentString;
            }
            return coefficientString + "E" + exponentString;
        }

        private string ToStringWithoutExponentialNotation(string coefficientString, int exponent)
        {
            if (exponent == 0)
            {
                return coefficientString;
            }
            else
            {
                var exponentAbsoluteValue = Math.Abs(exponent);
                var minimumCoefficientStringLength = exponentAbsoluteValue + 1;
                if (coefficientString.Length < minimumCoefficientStringLength)
                {
                    coefficientString = coefficientString.PadLeft(minimumCoefficientStringLength, '0');
                }
                var decimalPointIndex = coefficientString.Length - exponentAbsoluteValue;
                return coefficientString.Substring(0, decimalPointIndex) + "." + coefficientString.Substring(decimalPointIndex);
            }
        }

        private static bool TryTruncateToUInt64(Decimal128 d, ulong maxNegativeValue, ulong maxPositiveValue, out ulong value)
        {
            if (Decimal128.IsZero(d))
            {
                value = 0;
                return true;

            }

            var exponent = Decimal128.GetExponent(d);
            var significand = Decimal128.GetSignificand(d);

            if (exponent < 0)
            {
                while (exponent < 0)
                {
                    uint remainder; // ignored because we are truncating
                    significand = UInt128.Divide(significand, (uint)10, out remainder);
                    if (significand.Equals(UInt128.Zero))
                    {
                        value = 0;
                        return true;
                    }
                    exponent += 1;
                }
            }
            else if (exponent > 0)
            {
                while (exponent > 0)
                {
                    significand = UInt128.Multiply(significand, (uint)10);
                    if (significand.CompareTo(__maxSignificand) > 0)
                    {
                        value = 0;
                        return false;
                    }
                    exponent -= 1;
                }
            }

            if (exponent != 0)
            {
                value = 0;
                return false;
            }

            if (significand.High != 0 || significand.Low > (Decimal128.IsNegative(d) ? maxNegativeValue : maxPositiveValue))
            {
                value = 0;
                return false;
            }

            value = significand.Low;
            return true;
        }

        // nested types
        private class Decimal128Comparer : IComparer<Decimal128>
        {
            #region static
            // private static fields
            private static readonly Decimal128Comparer __instance = new Decimal128Comparer();

            // public static properties
            public static Decimal128Comparer Instance
            {
                get { return __instance; }
            }
            #endregion

            // public methods
            public int Compare(Decimal128 x, Decimal128 y)
            {
                var xType = GetDecimal128Type(x);
                var yType = GetDecimal128Type(y);
                var result = xType.CompareTo(yType);
                if (result == 0 && xType == Decimal128Type.Number)
                {
                    return CompareNumbers(x, y);
                }
                else
                {
                    return result;
                }
            }

            // private methods
            private Decimal128Type GetDecimal128Type(Decimal128 x)
            {
                if (Decimal128.IsNaN(x)) { return Decimal128Type.NaN; }
                else if (Decimal128.IsNegativeInfinity(x)) { return Decimal128Type.NegativeInfinity; }
                else if (Decimal128.IsPositiveInfinity(x)) { return Decimal128Type.PositiveInfity; }
                else { return Decimal128Type.Number; }
            }

            private int CompareNumbers(Decimal128 x, Decimal128 y)
            {
                var xClass = GetNumberClass(x);
                var yClass = GetNumberClass(y);
                var result = xClass.CompareTo(yClass);
                if (result == 0)
                {
                    if (xClass == NumberClass.Negative)
                    {
                        return CompareNegativeNumbers(x, y);
                    }
                    else if (xClass == NumberClass.Positive)
                    {
                        return ComparePositiveNumbers(x, y);
                    }
                    else
                    {
                        return 0; // else all Zeroes compare equal
                    }
                }
                else
                {
                    return result;
                }
            }

            private NumberClass GetNumberClass(Decimal128 x)
            {
                if (Decimal128.IsZero(x)) { return NumberClass.Zero; } // must test for Zero first
                else if (Decimal128.IsNegative(x)) { return NumberClass.Negative; }
                else { return NumberClass.Positive; }
            }

            private int CompareNegativeNumbers(Decimal128 x, Decimal128 y)
            {
                return -ComparePositiveNumbers(Decimal128.Negate(x), Decimal128.Negate(y));
            }

            private int ComparePositiveNumbers(Decimal128 x, Decimal128 y)
            {
                var xExponent = GetExponent(x);
                var yExponent = GetExponent(y);
                var exponentDifference = Math.Abs(xExponent - yExponent);
                if (exponentDifference <= 66)
                {
                    // we may or may not be able to make the exponents equal but we won't know until we try
                    // but we do know we can't eliminate an exponent difference larger than 66
                    if (xExponent < yExponent)
                    {
                        x = IncreaseExponent(x, yExponent);
                        y = DecreaseExponent(y, xExponent);
                    }
                    else if (xExponent > yExponent)
                    {
                        x = DecreaseExponent(x, yExponent);
                        y = IncreaseExponent(y, xExponent);
                    }
                }

                if (xExponent == yExponent)
                {
                    return GetSignificand(x).CompareTo(GetSignificand(y));
                }
                else
                {
                    return xExponent.CompareTo(yExponent);
                }
            }


            private enum Decimal128Type { NaN, NegativeInfinity, Number, PositiveInfity }; // the order matters
            private enum NumberClass { Negative, Zero, Positive }; // the order matters
        }

        private static class Flags
        {
            public const ulong SignBit = 0x8000000000000000;
            public const ulong FirstFormLeadingBits = 0x6000000000000000;
            public const ulong FirstFormLeadingBitsMax = 0x4000000000000000;
            public const ulong FirstFormExponentBits = 0x7FFE000000000000;
            public const ulong FirstFormSignificandBits = 0x0001FFFFFFFFFFFF;
            public const ulong SecondFormLeadingBits = 0x7800000000000000;
            public const ulong SecondFormLeadingBitsMin = 0x6000000000000000;
            public const ulong SecondFormLeadingBitsMax = 0x7000000000000000;
            public const ulong SecondFormExponentBits = 0x1FFF800000000000;
            public const ulong InfinityBits = 0x7C00000000000000;
            public const ulong Infinity = 0x7800000000000000;
            public const ulong SignedInfinityBits = 0xFC00000000000000;
            public const ulong PositiveInfinity = 0x7800000000000000;
            public const ulong NegativeInfinity = 0xF800000000000000;
            public const ulong PartialNaNBits = 0x7C00000000000000;
            public const ulong PartialNaN = 0x7C00000000000000;
            public const ulong NaNBits = 0x7E00000000000000;
            public const ulong QNaN = 0x7C00000000000000;
            public const ulong SNaN = 0x7E00000000000000;

            public static bool IsFirstForm(ulong highBits)
            {
                return (highBits & Flags.FirstFormLeadingBits) <= Flags.FirstFormLeadingBitsMax;
            }

            public static bool IsInfinity(ulong highBits)
            {
                return (highBits & Flags.InfinityBits) == Flags.Infinity;
            }

            public static bool IsNaN(ulong highBits)
            {
                return (highBits & Flags.PartialNaNBits) == Flags.PartialNaN;
            }

            public static bool IsNegative(ulong highBits)
            {
                return (highBits & Flags.SignBit) != 0;
            }

            public static bool IsNegativeInfinity(ulong highBits)
            {
                return (highBits & Flags.SignedInfinityBits) == Flags.NegativeInfinity;
            }

            public static bool IsPositiveInfinity(ulong highBits)
            {
                return (highBits & Flags.SignedInfinityBits) == Flags.PositiveInfinity;
            }

            public static bool IsQNaN(ulong highBits)
            {
                return (highBits & Flags.NaNBits) == Flags.QNaN;
            }

            public static bool IsSecondForm(ulong highBits)
            {
                var secondFormLeadingBits = highBits & Flags.SecondFormLeadingBits;
                return secondFormLeadingBits >= Flags.SecondFormLeadingBitsMin & secondFormLeadingBits <= Flags.SecondFormLeadingBitsMax;
            }

            public static bool IsSNaN(ulong highBits)
            {
                return (highBits & Flags.NaNBits) == Flags.SNaN;
            }
        }
    }
}
