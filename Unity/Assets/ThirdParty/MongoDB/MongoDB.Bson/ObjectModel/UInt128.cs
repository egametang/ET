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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MongoDB.Bson
{
    // this is a minimal implementation of UInt128 containing only what we need for Decimal128
    internal struct UInt128 : IComparable<UInt128>, IEquatable<UInt128>
    {
        #region static
        // public static properties
        public static UInt128 Zero
        {
            get { return new UInt128(0, 0); }
        }

        // public static methods
        public static UInt128 Add(UInt128 x, UInt128 y)
        {
            var high = x.High + y.High;
            var low = x.Low + y.Low;
            if (low < x.Low)
            {
                high += 1;
            }
            return new UInt128(high, low);
        }

        public static int Compare(UInt128 x, UInt128 y)
        {
            var result = x.High.CompareTo(y.High);
            if (result == 0)
            {
                result = x.Low.CompareTo(y.Low);
            }
            return result;
        }

        public static UInt128 Divide(UInt128 x, uint divisor, out uint remainder)
        {
            if (x.High == 0 && x.Low == 0)
            {
                remainder = 0;
                return UInt128.Zero;
            }

            var a = x.High >> 32;
            var b = x.High & 0xffffffff;
            var c = x.Low >> 32;
            var d = x.Low & 0xffffffff;

            var temp = a;
            a = (temp / divisor) & 0xffffffff;
            temp = ((temp % divisor) << 32) + b;
            b = (temp / divisor) & 0xffffffff;
            temp = ((temp % divisor) << 32) + c;
            c = (temp / divisor) & 0xffffffff;
            temp = ((temp % divisor) << 32) + d;
            d = (temp / divisor) & 0xffffffff;

            var high = (a << 32) + b;
            var low = (c << 32) + d;
            remainder = (uint)(temp % divisor);

            return new UInt128(high, low);
        }

        public static bool Equals(UInt128 x, UInt128 y)
        {
            return x.High == y.High && x.Low == y.Low;
        }

        public static UInt128 Multiply(UInt128 x, uint y)
        {
            var a = x.High >> 32;
            var b = x.High & 0xffffffff;
            var c = x.Low >> 32;
            var d = x.Low & 0xffffffff;

            d = d * y;
            c = c * y + (d >> 32);
            b = b * y + (c >> 32);
            a = a * y + (b >> 32);

            var low = (c << 32) + (d & 0xffffffff);
            var high = (a << 32) + (b & 0xffffffff);

            return new UInt128(high, low);
        }

        public static UInt128 Multiply(ulong x, ulong y)
        {
            // x = a * 2^32 + b
            // y = c * 2^32 + d
            // xy = ac * 2^64 + (ad + bc) * 2^32 + bd

            var a = x >> 32;
            var b = x & 0xffffffff;
            var c = y >> 32;
            var d = y & 0xffffffff;

            var ac = a * c;
            var ad = a * d;
            var bc = b * c;
            var bd = b * d;

            var mid = (ad & 0xffffffff) + (bc & 0xffffffff) + (bd >> 32);
            var high = ac + (ad >> 32) + (bc >> 32) + (mid >> 32);
            var low = (mid << 32) + (bd & 0xffffffff);

            return new UInt128(high, low);
        }

        public static UInt128 Parse(string s)
        {
            UInt128 value;
            if (!UInt128.TryParse(s, out value))
            {
                throw new FormatException($"Error parsing UInt128 string: \"{s}\".");
            }
            return value;
        }

        public static bool TryParse(string s, out UInt128 value)
        {
            if (s == null || s.Length == 0)
            {
                value = default(UInt128);
                return false;
            }

            // remove leading zeroes (and return true if value is zero)
            if (s[0] == '0')
            {
                if (s.Length == 1)
                {
                    value = UInt128.Zero;
                    return true;
                }
                else
                {
                    s = Regex.Replace(s, "^0+", "");
                    if (s.Length == 0)
                    {
                        value = UInt128.Zero;
                        return true;
                    }
                }
            }

            // parse 9 or fewer decimal digits at a time
            value = UInt128.Zero;
            while (s.Length > 0)
            {
                int fragmentSize = s.Length % 9;
                if (fragmentSize == 0)
                {
                    fragmentSize = 9;
                }
                var fragmentString = s.Substring(0, fragmentSize);

                uint fragmentValue;
                if (!uint.TryParse(fragmentString, out fragmentValue))
                {
                    value = default(UInt128);
                    return false;
                }

                var combinedValue = UInt128.Multiply(value, (uint)1000000000);
                combinedValue = UInt128.Add(combinedValue, new UInt128(0, fragmentValue));
                if (UInt128.Compare(combinedValue, value) < 0)
                {
                    // overflow means s represents a value larger than UInt128.MaxValue
                    value = default(UInt128);
                    return false;
                }
                value = combinedValue;

                s = s.Substring(fragmentSize);
            }

            return true;
        }
        #endregion

        // private fields
        private readonly ulong _high;
        private readonly ulong _low;

        // constructors
        public UInt128(ulong low)
        {
            _high = 0;
            _low = low;
        }

        public UInt128(ulong high, ulong low)
        {
            _high = high;
            _low = low;
        }

        // public properties
        public ulong High
        {
            get { return _high; }
        }

        public ulong Low
        {
            get { return _low; }
        }

        // public methods
        public int CompareTo(UInt128 other)
        {
            return UInt128.Compare(this, other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(UInt128))
            {
                return false;
            }
            return Equals((UInt128)obj);
        }

        public bool Equals(UInt128 other)
        {
            return _high == other._high && _low == other._low;
        }

        public override int GetHashCode()
        {
            return 37 * _high.GetHashCode() + _low.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = null; // don't create the builder until we actually need it
            var value = this;
            while (true)
            {
                // convert 9 decimal digits at a time to a string
                uint remainder;
                value = UInt128.Divide(value, (uint)1000000000, out remainder);
                var fragmentString = remainder.ToString(NumberFormatInfo.InvariantInfo);

                if (UInt128.Equals(value, UInt128.Zero))
                {
                    if (builder == null)
                    {
                        return fragmentString; // values with 9 decimal digits or less don't need the builder
                    }
                    else
                    {
                        builder.Insert(0, fragmentString);
                        return builder.ToString();
                    }
                }

                if (builder == null)
                {
                    builder = new StringBuilder(38);
                }
                builder.Insert(0, fragmentString);
                builder.Insert(0, "0", 9 - fragmentString.Length);
            }
        }
    }
}
