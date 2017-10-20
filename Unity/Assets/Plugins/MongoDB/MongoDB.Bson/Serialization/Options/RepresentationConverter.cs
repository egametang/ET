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
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Bson.Serialization.Options
{
    /// <summary>
    /// Represents the external representation of a field or property.
    /// </summary>
    public class RepresentationConverter
    {
        // private fields
        private bool _allowOverflow;
        private bool _allowTruncation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the RepresentationConverter class.
        /// </summary>
        /// <param name="allowOverflow">Whether to allow overflow.</param>
        /// <param name="allowTruncation">Whether to allow truncation.</param>
        public RepresentationConverter(bool allowOverflow, bool allowTruncation)
        {
            _allowOverflow = allowOverflow;
            _allowTruncation = allowTruncation;
        }

        // public properties
        /// <summary>
        /// Gets whether to allow overflow.
        /// </summary>
        public bool AllowOverflow
        {
            get { return _allowOverflow; }
        }

        /// <summary>
        /// Gets whether to allow truncation.
        /// </summary>
        public bool AllowTruncation
        {
            get { return _allowTruncation; }
        }

        // public methods
        /// <summary>
        /// Converts a Decimal128 to a Decimal.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>A Decimal.</returns>
        public decimal ToDecimal(Decimal128 value)
        {
            if (value == Decimal128.MaxValue)
            {
                return decimal.MaxValue;
            }
            else if (value == Decimal128.MinValue)
            {
                return decimal.MinValue;
            }
            else if (Decimal128.IsInfinity(value) || Decimal128.IsNaN(value))
            {
                throw new OverflowException();
            }

            decimal decimalValue;
            if (_allowOverflow)
            {
                try { decimalValue = (decimal)value; } catch (OverflowException) { decimalValue = Decimal128.IsNegative(value) ? decimal.MinValue : decimal.MaxValue; }
            }
            else
            {
                decimalValue = (decimal)value;
            }

            if (!_allowTruncation && value != (Decimal128)decimalValue)
            {
                throw new TruncationException();
            }

            return decimalValue;
        }

        /// <summary>
        /// Converts a Double to a Decimal.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A Decimal.</returns>
        public decimal ToDecimal(double value)
        {
            if (value == double.MinValue)
            {
                return decimal.MinValue;
            }
            else if (value == double.MaxValue)
            {
                return decimal.MaxValue;
            }

            var decimalValue = (decimal)value;
            if (value < (double)decimal.MinValue || value > (double)decimal.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (double)decimalValue)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return decimalValue;
        }

        /// <summary>
        /// Converts an Int32 to a Decimal.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>A Decimal.</returns>
        public decimal ToDecimal(int value)
        {
            return (decimal)value;
        }

        /// <summary>
        /// Converts an Int64 to a Decimal.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>A Decimal.</returns>
        public decimal ToDecimal(long value)
        {
            return (decimal)value;
        }

        /// <summary>
        /// Converts a decimal to a Decimal128.
        /// </summary>
        /// <param name="value">A decimal.</param>
        /// <returns>A Decimal128.</returns>
        public Decimal128 ToDecimal128(decimal value)
        {
            if (value == decimal.MaxValue)
            {
                return Decimal128.MaxValue;
            }
            else if (value == decimal.MinValue)
            {
                return Decimal128.MinValue;
            }

            // conversion from decimal to Decimal128 is lossless so need to check for overflow or truncation
            return (Decimal128)value;
        }

        /// <summary>
        /// Converts a Double to a Decimal128.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A Decimal128.</returns>
        public Decimal128 ToDecimal128(double value)
        {
            if (value == double.MaxValue)
            {
                return Decimal128.MaxValue;
            }
            else if (value == double.MinValue)
            {
                return Decimal128.MinValue;
            }
            else if (double.IsPositiveInfinity(value))
            {
                return Decimal128.PositiveInfinity;
            }
            else if (double.IsNegativeInfinity(value))
            {
                return Decimal128.NegativeInfinity;
            }
            else if (double.IsNaN(value))
            {
                return Decimal128.QNaN;
            }

            var decimal128Value = (Decimal128)value;
            if (!_allowTruncation && value != (double)decimal128Value)
            {
                throw new TruncationException();
            }
            return decimal128Value;
        }

        /// <summary>
        /// Converts an Int32 to a Decimal128.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>A Decimal128.</returns>
        public Decimal128 ToDecimal128(int value)
        {
            return (Decimal128)value;
        }

        /// <summary>
        /// Converts an Int64 to a Decimal128.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>A Decimal128.</returns>
        public Decimal128 ToDecimal128(long value)
        {
            return (Decimal128)value;
        }

        /// <summary>
        /// Converts a UInt64 to a Decimal128.
        /// </summary>
        /// <param name="value">A UInt64.</param>
        /// <returns>A Decimal128.</returns>
        [CLSCompliant(false)]
        public Decimal128 ToDecimal128(ulong value)
        {
            return (Decimal128)value;
        }

        /// <summary>
        /// Converts a Decimal to a Double.
        /// </summary>
        /// <param name="value">A Decimal.</param>
        /// <returns>A Double.</returns>
        public double ToDouble(decimal value)
        {
            if (value == decimal.MinValue)
            {
                return double.MinValue;
            }
            else if (value == decimal.MaxValue)
            {
                return double.MaxValue;
            }

            var doubleValue = (double)value;
            if (value != (decimal)doubleValue)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return doubleValue;
        }

        /// <summary>
        /// Converts a Decimal128 to a Double.
        /// </summary>
        /// <param name="value">A Decimal.</param>
        /// <returns>A Double.</returns>
        public double ToDouble(Decimal128 value)
        {
            if (value == Decimal128.MaxValue)
            {
                return double.MaxValue;
            }
            else if (value == Decimal128.MinValue)
            {
                return double.MinValue;
            }
            else if (Decimal128.IsPositiveInfinity(value))
            {
                return double.PositiveInfinity;
            }
            else if (Decimal128.IsNegativeInfinity(value))
            {
                return double.NegativeInfinity;
            }
            else if (Decimal128.IsNaN(value))
            {
                return double.NaN;
            }

            double doubleValue;
            if (_allowOverflow)
            {
                try { doubleValue = (double)value; } catch (OverflowException) { doubleValue = Decimal128.IsNegative(value) ? double.MinValue : double.MaxValue; }
            }
            else
            {
                doubleValue = (double)value;
            }

            if (!_allowTruncation && value != (Decimal128)doubleValue)
            {
                throw new TruncationException();
            }

            return doubleValue;
        }

        /// <summary>
        /// Converts a Double to a Double.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A Double.</returns>
        public double ToDouble(double value)
        {
            return value;
        }

        /// <summary>
        /// Converts a Single to a Double.
        /// </summary>
        /// <param name="value">A Single.</param>
        /// <returns>A Double.</returns>
        public double ToDouble(float value)
        {
            if (value == float.MinValue)
            {
                return double.MinValue;
            }
            else if (value == float.MaxValue)
            {
                return double.MaxValue;
            }
            else if (float.IsNegativeInfinity(value))
            {
                return double.NegativeInfinity;
            }
            else if (float.IsPositiveInfinity(value))
            {
                return double.PositiveInfinity;
            }
            else if (float.IsNaN(value))
            {
                return double.NaN;
            }
            return value;
        }

        /// <summary>
        /// Converts an Int32 to a Double.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>A Double.</returns>
        public double ToDouble(int value)
        {
            return value;
        }

        /// <summary>
        /// Converts an Int64 to a Double.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>A Double.</returns>
        public double ToDouble(long value)
        {
            var doubleValue = (double)value;
            if (value != (long)doubleValue)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return doubleValue;
        }

        /// <summary>
        /// Converts an Int16 to a Double.
        /// </summary>
        /// <param name="value">An Int16.</param>
        /// <returns>A Double.</returns>
        public double ToDouble(short value)
        {
            return value;
        }

        /// <summary>
        /// Converts a UInt32 to a Double.
        /// </summary>
        /// <param name="value">A UInt32.</param>
        /// <returns>A Double.</returns>
        [CLSCompliant(false)]
        public double ToDouble(uint value)
        {
            return value;
        }

        /// <summary>
        /// Converts a UInt64 to a Double.
        /// </summary>
        /// <param name="value">A UInt64.</param>
        /// <returns>A Double.</returns>
        [CLSCompliant(false)]
        public double ToDouble(ulong value)
        {
            var doubleValue = (double)value;
            if (value != (ulong)doubleValue)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return doubleValue;
        }

        /// <summary>
        /// Converts a UInt16 to a Double.
        /// </summary>
        /// <param name="value">A UInt16.</param>
        /// <returns>A Double.</returns>
        [CLSCompliant(false)]
        public double ToDouble(ushort value)
        {
            return value;
        }

        /// <summary>
        /// Converts a Decimal128 to an Int16.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>An Int16.</returns>
        public short ToInt16(Decimal128 value)
        {
            short shortValue;
            if (_allowOverflow)
            {
                try { shortValue = (short)value; } catch (OverflowException) { shortValue = Decimal128.IsNegative(value) ? short.MinValue : short.MaxValue; }
            }
            else
            {
                shortValue = (short)value;
            }

            if (!_allowTruncation && value != (Decimal128)shortValue)
            {
                throw new TruncationException();
            }

            return shortValue;
        }

        /// <summary>
        /// Converts a Double to an Int16.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>An Int16.</returns>
        public short ToInt16(double value)
        {
            var int16Value = (short)value;
            if (value < short.MinValue || value > short.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (double)int16Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return int16Value;
        }

        /// <summary>
        /// Converts an Int32 to an Int16.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>An Int16.</returns>
        public short ToInt16(int value)
        {
            if (value < short.MinValue || value > short.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (short)value;
        }

        /// <summary>
        /// Converts an Int64 to an Int16.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>An Int16.</returns>
        public short ToInt16(long value)
        {
            if (value < short.MinValue || value > short.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (short)value;
        }

        /// <summary>
        /// Converts a Decimal to an Int32.
        /// </summary>
        /// <param name="value">A Decimal.</param>
        /// <returns>An Int32.</returns>
        public int ToInt32(decimal value)
        {
            if (value == decimal.MinValue)
            {
                return int.MinValue;
            }
            else if (value == decimal.MaxValue)
            {
                return int.MaxValue;
            }

            var int32Value = (int)value;
            if (value < int.MinValue || value > int.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (decimal)int32Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return int32Value;
        }

        /// <summary>
        /// Converts a Decimal128 to an Int32.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>An Int32.</returns>
        public int ToInt32(Decimal128 value)
        {
            int intValue;
            if (_allowOverflow)
            {
                try { intValue = (int)value; } catch (OverflowException) { intValue = Decimal128.IsNegative(value) ? int.MinValue : int.MaxValue; }
            }
            else
            {
                intValue = (int)value;
            }
            if (!_allowTruncation && value != (Decimal128)intValue)
            {
                throw new TruncationException();
            }
            return intValue;
        }

        /// <summary>
        /// Converts a Double to an Int32.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>An Int32.</returns>
        public int ToInt32(double value)
        {
            var int32Value = (int)value;
            if (value < int.MinValue || value > int.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (double)int32Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return int32Value;
        }

        /// <summary>
        /// Converts a Single to an Int32.
        /// </summary>
        /// <param name="value">A Single.</param>
        /// <returns>An Int32.</returns>
        public int ToInt32(float value)
        {
            var int32Value = (int)value;
            if (value < int.MinValue || value > int.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (float)int32Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return int32Value;
        }

        /// <summary>
        /// Converts an Int32 to an Int32.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>An Int32.</returns>
        public int ToInt32(int value)
        {
            return value;
        }

        /// <summary>
        /// Converts an Int64 to an Int32.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>An Int32.</returns>
        public int ToInt32(long value)
        {
            if (value < int.MinValue || value > int.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (int)value;
        }

        /// <summary>
        /// Converts an Int16 to an Int32.
        /// </summary>
        /// <param name="value">An Int16.</param>
        /// <returns>An Int32.</returns>
        public int ToInt32(short value)
        {
            return value;
        }

        /// <summary>
        /// Converts a UInt32 to an Int32.
        /// </summary>
        /// <param name="value">A UInt32.</param>
        /// <returns>An Int32.</returns>
        [CLSCompliant(false)]
        public int ToInt32(uint value)
        {
            if (value > (uint)int.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (int)value;
        }

        /// <summary>
        /// Converts a UInt64 to an Int32.
        /// </summary>
        /// <param name="value">A UInt64.</param>
        /// <returns>An Int32.</returns>
        [CLSCompliant(false)]
        public int ToInt32(ulong value)
        {
            if (value > (ulong)int.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (int)value;
        }

        /// <summary>
        /// Converts a UInt16 to an Int32.
        /// </summary>
        /// <param name="value">A UInt16.</param>
        /// <returns>An Int32.</returns>
        [CLSCompliant(false)]
        public int ToInt32(ushort value)
        {
            return value;
        }

        /// <summary>
        /// Converts a Decimal to an Int64.
        /// </summary>
        /// <param name="value">A Decimal.</param>
        /// <returns>An Int64.</returns>
        public long ToInt64(decimal value)
        {
            if (value == decimal.MinValue)
            {
                return long.MinValue;
            }
            else if (value == decimal.MaxValue)
            {
                return long.MaxValue;
            }

            var int64Value = (long)value;
            if (value < long.MinValue || value > long.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (decimal)int64Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return int64Value;
        }

        /// <summary>
        /// Converts a Decimal128 to an Int64.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>An Int64.</returns>
        public long ToInt64(Decimal128 value)
        {
            long longValue;
            if (_allowOverflow)
            {
                try { longValue = (long)value; } catch (OverflowException) { longValue = Decimal128.IsNegative(value) ? long.MinValue : long.MaxValue; }
            }
            else
            {
                longValue = (long)value;
            }
            if (!_allowTruncation && value != (Decimal128)longValue)
            {
                throw new TruncationException();
            }
            return longValue;
        }

        /// <summary>
        /// Converts a Double to an Int64.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>An Int64.</returns>
        public long ToInt64(double value)
        {
            var int64Value = (long)value;
            if (value < long.MinValue || value > long.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (double)int64Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return int64Value;
        }

        /// <summary>
        /// Converts a Single to an Int64.
        /// </summary>
        /// <param name="value">A Single.</param>
        /// <returns>An Int64.</returns>
        public long ToInt64(float value)
        {
            var int64Value = (long)value;
            if (value < long.MinValue || value > long.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (float)int64Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return int64Value;
        }

        /// <summary>
        /// Converts an Int32 to an Int64.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>An Int64.</returns>
        public long ToInt64(int value)
        {
            return value;
        }

        /// <summary>
        /// Converts an Int64 to an Int64.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>An Int64.</returns>
        public long ToInt64(long value)
        {
            return value;
        }

        /// <summary>
        /// Converts an Int16 to an Int64.
        /// </summary>
        /// <param name="value">An Int16.</param>
        /// <returns>An Int64.</returns>
        public long ToInt64(short value)
        {
            return value;
        }

        /// <summary>
        /// Converts a UInt32 to an Int64.
        /// </summary>
        /// <param name="value">A UInt32.</param>
        /// <returns>An Int64.</returns>
        [CLSCompliant(false)]
        public long ToInt64(uint value)
        {
            return (long)value;
        }

        /// <summary>
        /// Converts a UInt64 to an Int64.
        /// </summary>
        /// <param name="value">A UInt64.</param>
        /// <returns>An Int64.</returns>
        [CLSCompliant(false)]
        public long ToInt64(ulong value)
        {
            if (value > (ulong)long.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (long)value;
        }

        /// <summary>
        /// Converts a UInt16 to an Int64.
        /// </summary>
        /// <param name="value">A UInt16.</param>
        /// <returns>An Int64.</returns>
        [CLSCompliant(false)]
        public long ToInt64(ushort value)
        {
            return value;
        }

        /// <summary>
        /// Converts a Decimal128 to a Single.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>A Single.</returns>
        public float ToSingle(Decimal128 value)
        {
            if (value == Decimal128.MaxValue)
            {
                return float.MaxValue;
            }
            else if (value == Decimal128.MinValue)
            {
                return float.MinValue;
            }
            else if (Decimal128.IsPositiveInfinity(value))
            {
                return float.PositiveInfinity;
            }
            else if (Decimal128.IsNegativeInfinity(value))
            {
                return float.NegativeInfinity;
            }
            else if (Decimal128.IsNaN(value))
            {
                return float.NaN;
            }

            float floatValue;
            if (_allowOverflow)
            {
                try { floatValue = (float)value; } catch (OverflowException) { floatValue = Decimal128.IsNegative(value) ? float.MinValue : float.MaxValue; }
            }
            else
            {
                floatValue = (float)value;
            }

            if (!_allowTruncation && value != (Decimal128)floatValue)
            {
                throw new TruncationException();
            }

            return floatValue;
        }

        /// <summary>
        /// Converts a Double to a Single.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A Single.</returns>
        public float ToSingle(double value)
        {
            if (value == double.MinValue)
            {
                return float.MinValue;
            }
            else if (value == double.MaxValue)
            {
                return float.MaxValue;
            }
            else if (double.IsNegativeInfinity(value))
            {
                return float.NegativeInfinity;
            }
            else if (double.IsPositiveInfinity(value))
            {
                return float.PositiveInfinity;
            }
            else if (double.IsNaN(value))
            {
                return float.NaN;
            }

            var floatValue = (float)value;
            if (value < float.MinValue || value > float.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (double)floatValue)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return floatValue;
        }

        /// <summary>
        /// Converts an Int32 to a Single.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>A Single.</returns>
        public float ToSingle(int value)
        {
            var floatValue = (float)value;
            if (value != (int)floatValue)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return floatValue;
        }

        /// <summary>
        /// Converts an Int64 to a Single.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>A Single.</returns>
        public float ToSingle(long value)
        {
            var floatValue = (float)value;
            if (value != (long)floatValue)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return floatValue;
        }

        /// <summary>
        /// Converts a Decimal128 to a UInt16.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>A UInt16.</returns>
        [CLSCompliant(false)]
        public ushort ToUInt16(Decimal128 value)
        {
            ushort ushortValue;
            if (_allowOverflow)
            {
                try { ushortValue = (ushort)value; } catch (OverflowException) { ushortValue = Decimal128.IsNegative(value) ? ushort.MinValue : ushort.MaxValue; }
            }
            else
            {
                ushortValue = (ushort)value;
            }

            if (!_allowTruncation && value != (Decimal128)ushortValue)
            {
                throw new TruncationException();
            }

            return ushortValue;
        }

        /// <summary>
        /// Converts a Double to a UInt16.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A UInt16.</returns>
        [CLSCompliant(false)]
        public ushort ToUInt16(double value)
        {
            var uint16Value = (ushort)value;
            if (value < ushort.MinValue || value > ushort.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (double)uint16Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return uint16Value;
        }

        /// <summary>
        /// Converts an Int32 to a UInt16.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>A UInt16.</returns>
        [CLSCompliant(false)]
        public ushort ToUInt16(int value)
        {
            if (value < ushort.MinValue || value > ushort.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (ushort)value;
        }

        /// <summary>
        /// Converts an Int64 to a UInt16.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>A UInt16.</returns>
        [CLSCompliant(false)]
        public ushort ToUInt16(long value)
        {
            if (value < ushort.MinValue || value > ushort.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (ushort)value;
        }

        /// <summary>
        /// Converts a Decimal128 to a UInt32.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>A UInt32.</returns>
        [CLSCompliant(false)]
        public uint ToUInt32(Decimal128 value)
        {
            uint uintValue;
            if (_allowOverflow)
            {
                try { uintValue = (uint)value; } catch (OverflowException) { uintValue = Decimal128.IsNegative(value) ? uint.MinValue : uint.MaxValue; }
            }
            else
            {
                uintValue = (uint)value;
            }

            if (!_allowTruncation && value != (Decimal128)uintValue)
            {
                throw new TruncationException();
            }

            return uintValue;
        }

        /// <summary>
        /// Converts a Double to a UInt32.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A UInt32.</returns>
        [CLSCompliant(false)]
        public uint ToUInt32(double value)
        {
            var uint32Value = (uint)value;
            if (value < uint.MinValue || value > uint.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (double)uint32Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return uint32Value;
        }

        /// <summary>
        /// Converts an Int32 to a UInt32.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>A UInt32.</returns>
        [CLSCompliant(false)]
        public uint ToUInt32(int value)
        {
            if (value < uint.MinValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (uint)value;
        }

        /// <summary>
        /// Converts an Int64 to a UInt32.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>A UInt32.</returns>
        [CLSCompliant(false)]
        public uint ToUInt32(long value)
        {
            if (value < uint.MinValue || value > uint.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (uint)value;
        }

        /// <summary>
        /// Converts a Decimal128 to a UInt64.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>A UInt64.</returns>
        [CLSCompliant(false)]
        public ulong ToUInt64(Decimal128 value)
        {
            ulong ulongValue;
            if (_allowOverflow)
            {
                try { ulongValue = (ulong)value; } catch (OverflowException) { ulongValue = Decimal128.IsNegative(value) ? ulong.MinValue : ulong.MaxValue; }
            }
            else
            {
                ulongValue = (ulong)value;
            }

            if (!_allowTruncation && value != (Decimal128)ulongValue)
            {
                throw new TruncationException();
            }

            return ulongValue;
        }

        /// <summary>
        /// Converts a Double to a UInt64.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A UInt64.</returns>
        [CLSCompliant(false)]
        public ulong ToUInt64(double value)
        {
            var uint64Value = (ulong)value;
            if (value < ulong.MinValue || value > ulong.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            else if (value != (double)uint64Value)
            {
                if (!_allowTruncation) { throw new TruncationException(); }
            }
            return uint64Value;
        }

        /// <summary>
        /// Converts an Int32 to a UInt64.
        /// </summary>
        /// <param name="value">An Int32.</param>
        /// <returns>A UInt64.</returns>
        [CLSCompliant(false)]
        public ulong ToUInt64(int value)
        {
            if (value < (int)ulong.MinValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (ulong)value;
        }

        /// <summary>
        /// Converts an Int64 to a UInt64.
        /// </summary>
        /// <param name="value">An Int64.</param>
        /// <returns>A UInt64.</returns>
        [CLSCompliant(false)]
        public ulong ToUInt64(long value)
        {
            if (value < (int)ulong.MinValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (ulong)value;
        }
    }
}
