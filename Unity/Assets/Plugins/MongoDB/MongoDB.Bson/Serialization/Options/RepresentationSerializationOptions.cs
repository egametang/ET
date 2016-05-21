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
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Bson.Serialization.Options
{
    /// <summary>
    /// Represents the external representation of a field or property.
    /// </summary>
    public class RepresentationSerializationOptions : BsonBaseSerializationOptions
    {
        // private fields
        private BsonType _representation;
        private bool _allowOverflow;
        private bool _allowTruncation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the RepresentationSerializationOptions class.
        /// </summary>
        /// <param name="representation">The external representation.</param>
        public RepresentationSerializationOptions(BsonType representation)
        {
            _representation = representation;
        }

        /// <summary>
        /// Initializes a new instance of the RepresentationSerializationOptions class.
        /// </summary>
        /// <param name="representation">The external representation.</param>
        /// <param name="allowOverflow">Whether to allow overflow.</param>
        /// <param name="allowTruncation">Whether to allow truncation.</param>
        public RepresentationSerializationOptions(BsonType representation, bool allowOverflow, bool allowTruncation)
        {
            _representation = representation;
            _allowOverflow = allowOverflow;
            _allowTruncation = allowTruncation;
        }

        // public properties
        /// <summary>
        /// Gets the external representation.
        /// </summary>
        public BsonType Representation
        {
            get { return _representation; }
            set
            {
                EnsureNotFrozen();
                _representation = value;
            }
        }

        /// <summary>
        /// Gets whether to allow overflow.
        /// </summary>
        public bool AllowOverflow
        {
            get { return _allowOverflow; }
            set
            {
                EnsureNotFrozen();
                _allowOverflow = value;
            }
        }

        /// <summary>
        /// Gets whether to allow truncation.
        /// </summary>
        public bool AllowTruncation
        {
            get { return _allowTruncation; }
            set
            {
                EnsureNotFrozen();
                _allowTruncation = value;
            }
        }

        // public methods
        /// <summary>
        /// Apply an attribute to these serialization options and modify the options accordingly.
        /// </summary>
        /// <param name="serializer">The serializer that these serialization options are for.</param>
        /// <param name="attribute">The serialization options attribute.</param>
        public override void ApplyAttribute(IBsonSerializer serializer, Attribute attribute)
        {
            EnsureNotFrozen();
            var representationAttribute = attribute as BsonRepresentationAttribute;
            if (representationAttribute != null)
            {
                _allowOverflow = representationAttribute.AllowOverflow;
                _allowTruncation = representationAttribute.AllowTruncation;
                _representation = representationAttribute.Representation;
                return;
            }

            var message = string.Format("A serialization options attribute of type {0} cannot be applied to serialization options of type {1}.",
                BsonUtils.GetFriendlyTypeName(attribute.GetType()), BsonUtils.GetFriendlyTypeName(GetType()));
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Clones the serialization options.
        /// </summary>
        /// <returns>A cloned copy of the serialization options.</returns>
        public override IBsonSerializationOptions Clone()
        {
            return new RepresentationSerializationOptions(_representation);
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
			if (decimalValue < decimal.MinValue || decimalValue > decimal.MaxValue)
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
        public double ToDouble(uint value)
        {
            return value;
        }

        /// <summary>
        /// Converts a UInt64 to a Double.
        /// </summary>
        /// <param name="value">A UInt64.</param>
        /// <returns>A Double.</returns>
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
        public double ToDouble(ushort value)
        {
            return value;
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
        public long ToInt64(uint value)
        {
            return (long)value;
        }

        /// <summary>
        /// Converts a UInt64 to an Int64.
        /// </summary>
        /// <param name="value">A UInt64.</param>
        /// <returns>An Int64.</returns>
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
        public long ToInt64(ushort value)
        {
            return value;
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
        /// Converts a Double to a UInt16.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A UInt16.</returns>
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
        public ushort ToUInt16(long value)
        {
            if (value < ushort.MinValue || value > ushort.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (ushort)value;
        }

        /// <summary>
        /// Converts a Double to a UInt32.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A UInt32.</returns>
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
        public uint ToUInt32(long value)
        {
            if (value < uint.MinValue || value > uint.MaxValue)
            {
                if (!_allowOverflow) { throw new OverflowException(); }
            }
            return (uint)value;
        }

        /// <summary>
        /// Converts a Double to a UInt64.
        /// </summary>
        /// <param name="value">A Double.</param>
        /// <returns>A UInt64.</returns>
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
