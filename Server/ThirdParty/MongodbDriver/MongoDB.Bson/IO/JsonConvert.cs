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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Encodes and decodes scalar values to JSON compatible strings.
    /// </summary>
    public static class JsonConvert
    {
        /// <summary>
        /// Converts a string to a Boolean.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A Boolean.</returns>
        public static bool ToBoolean(string value)
        {
            return bool.Parse(value);
        }

        /// <summary>
        /// Converts a string to a DateTime.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A DateTime.</returns>
        public static DateTime ToDateTime(string value)
        {
            var formats = new[]
            {
                "yyyy-MM-ddK",
                "yyyy-MM-ddTHH:mm:ssK",
                "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"
            };
            var style = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
            return DateTime.ParseExact(value, formats, DateTimeFormatInfo.InvariantInfo, style);
        }

        /// <summary>
        /// Converts a string to a DateTimeOffset.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns> A DateTimeOffset.</returns>
        public static DateTimeOffset ToDateTimeOffset(string value)
        {
            return DateTimeOffset.ParseExact(value, "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts a string to a Decimal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A Decimal.</returns>
        public static decimal ToDecimal(string value)
        {
            return decimal.Parse(value, NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts a string to a <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="Decimal128"/>.</returns>
        public static Decimal128 ToDecimal128(string value)
        {
            return Decimal128.Parse(value);
        }

        /// <summary>
        /// Converts a string to a Double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A Double.</returns>
        public static double ToDouble(string value)
        {
            return double.Parse(value, NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts a string to an Int16.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>An Int16.</returns>
        public static short ToInt16(string value)
        {
            return Int16.Parse(value);
        }

        /// <summary>
        /// Converts a string to an Int32.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>An Int32.</returns>
        public static int ToInt32(string value)
        {
            return Int32.Parse(value);
        }

        /// <summary>
        /// Converts a string to an Int64.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>An Int64.</returns>
        public static long ToInt64(string value)
        {
            return Int64.Parse(value);
        }

        /// <summary>
        /// Converts a string to a Single.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A Single.</returns>
        public static float ToSingle(string value)
        {
            return float.Parse(value, NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts a Boolean to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(bool value)
        {
            return value ? "true" : "false";
        }

        /// <summary>
        /// Converts a DateTime to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(DateTime value)
        {
            return value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK", DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts a DateTimeOffset to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(DateTimeOffset value)
        {
            return value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK", DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts a Decimal to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(decimal value)
        {
            return value.ToString("G", NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts a <see cref="Decimal128"/> to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(Decimal128 value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts a Double to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(double value)
        {
            return value.ToString("G17", NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts a Single to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(float value)
        {
            return value.ToString("R", NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts an Int32 to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(int value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts an Int64 to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(long value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts an Int16 to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        public static string ToString(short value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts a UInt32 to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        [CLSCompliant(false)]
        public static string ToString(uint value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts a UInt64 to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        [CLSCompliant(false)]
        public static string ToString(ulong value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts a UInt16 to a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A string.</returns>
        [CLSCompliant(false)]
        public static string ToString(ushort value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts a string to a UInt16.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A UInt16.</returns>
        [CLSCompliant(false)]
        public static ushort ToUInt16(string value)
        {
            return UInt16.Parse(value);
        }

        /// <summary>
        /// Converts a string to a UInt32.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A UInt32.</returns>
        [CLSCompliant(false)]
        public static uint ToUInt32(string value)
        {
            return UInt32.Parse(value);
        }

        /// <summary>
        /// Converts a string to a UInt64.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A UInt64.</returns>
        [CLSCompliant(false)]
        public static ulong ToUInt64(string value)
        {
            return UInt64.Parse(value);
        }
    }
}
