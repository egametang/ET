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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON value (this is an abstract class, see the various subclasses).
    /// </summary>
#if NET45
    [Serializable]
#endif
    public abstract class BsonValue : IComparable<BsonValue>, IConvertible, IEquatable<BsonValue>
    {
        // private static fields
        private static Dictionary<BsonType, int> __bsonTypeSortOrder = new Dictionary<BsonType, int>
        {
            { BsonType.MinKey, 1 },
            { BsonType.Undefined, 2 },
            { BsonType.Null, 3 },
            { BsonType.Decimal128, 4 },
            { BsonType.Double, 4 },
            { BsonType.Int32, 4 },
            { BsonType.Int64, 4 },
            { BsonType.String, 5 },
            { BsonType.Symbol, 5 },
            { BsonType.Document, 6 },
            { BsonType.Array, 7 },
            { BsonType.Binary, 8 },
            { BsonType.ObjectId, 9 },
            { BsonType.Boolean, 10 },
            { BsonType.DateTime, 11 },
            { BsonType.Timestamp, 11 },
            { BsonType.RegularExpression, 12 },
            { BsonType.JavaScript, 13 }, // TODO: confirm where JavaScript and JavaScriptWithScope are in the sort order
            { BsonType.JavaScriptWithScope, 14 },
            { BsonType.MaxKey, 15 }
        };

        // public properties
        /// <summary>
        /// Casts the BsonValue to a Boolean (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public bool AsBoolean
        {
            get { return ((BsonBoolean)this).Value; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonArray (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonArray AsBsonArray
        {
            get { return (BsonArray)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonBinaryData (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonBinaryData AsBsonBinaryData
        {
            get { return (BsonBinaryData)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonDateTime (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonDateTime AsBsonDateTime
        {
            get { return (BsonDateTime)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonDocument (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonDocument AsBsonDocument
        {
            get { return (BsonDocument)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonJavaScript (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonJavaScript AsBsonJavaScript
        {
            get { return (BsonJavaScript)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonJavaScriptWithScope (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonJavaScriptWithScope AsBsonJavaScriptWithScope
        {
            get { return (BsonJavaScriptWithScope)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonMaxKey (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonMaxKey AsBsonMaxKey
        {
            get { return (BsonMaxKey)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonMinKey (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonMinKey AsBsonMinKey
        {
            get { return (BsonMinKey)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonNull (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonNull AsBsonNull
        {
            get { return (BsonNull)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonRegularExpression (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonRegularExpression AsBsonRegularExpression
        {
            get { return (BsonRegularExpression)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonSymbol (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonSymbol AsBsonSymbol
        {
            get { return (BsonSymbol)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonTimestamp (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonTimestamp AsBsonTimestamp
        {
            get { return (BsonTimestamp)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonUndefined (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public BsonUndefined AsBsonUndefined
        {
            get { return (BsonUndefined)this; }
        }

        /// <summary>
        /// Casts the BsonValue to a BsonValue (a way of upcasting subclasses of BsonValue to BsonValue at compile time).
        /// </summary>
        public BsonValue AsBsonValue
        {
            get { return this; }
        }

        /// <summary>
        /// Casts the BsonValue to a Byte[] (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public byte[] AsByteArray
        {
            get { return ((BsonBinaryData)this).Bytes; }
        }

        /// <summary>
        /// Casts the BsonValue to a DateTime in UTC (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        [Obsolete("Use ToUniversalTime instead.")]
        public DateTime AsDateTime
        {
            get { return AsUniversalTime; }
        }

        /// <summary>
        /// Casts the BsonValue to a <see cref="decimal"/> (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public decimal AsDecimal
        {
            get { return (decimal)((BsonDecimal128)this).Value; }
        }

        /// <summary>
        /// Casts the BsonValue to a <see cref="Decimal128"/> (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public Decimal128 AsDecimal128
        {
            get { return ((BsonDecimal128)this).Value; }
        }

        /// <summary>
        /// Casts the BsonValue to a Double (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public double AsDouble
        {
            get { return ((BsonDouble)this).Value; }
        }

        /// <summary>
        /// Casts the BsonValue to a Guid (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public Guid AsGuid
        {
            get { return ((BsonBinaryData)this).ToGuid(); }
        }

        /// <summary>
        /// Casts the BsonValue to an Int32 (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public int AsInt32
        {
            get { return ((BsonInt32)this).Value; }
        }

        /// <summary>
        /// Casts the BsonValue to a DateTime in the local timezone (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        [Obsolete("Use ToLocalTime instead.")]
        public DateTime AsLocalTime
        {
            get { return ((BsonDateTime)this).ToLocalTime(); }
        }

        /// <summary>
        /// Casts the BsonValue to a Int64 (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public long AsInt64
        {
            get { return ((BsonInt64)this).Value; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Boolean} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public bool? AsNullableBoolean
        {
            get { return (BsonType == BsonType.Null) ? null : (bool?)AsBoolean; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{DateTime} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        [Obsolete("Use ToNullableUniversalTime instead.")]
        public DateTime? AsNullableDateTime
        {
            get { return (BsonType == BsonType.Null) ? null : (DateTime?)AsDateTime; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Decimal} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public decimal? AsNullableDecimal
        {
            get { return (BsonType == BsonType.Null) ? null : (decimal?)AsDecimal128; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Decimal128} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public Decimal128? AsNullableDecimal128
        {
            get { return (BsonType == BsonType.Null) ? null : (Decimal128?)AsDecimal128; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Double} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public double? AsNullableDouble
        {
            get { return (BsonType == BsonType.Null) ? null : (double?)AsDouble; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Guid} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public Guid? AsNullableGuid
        {
            get { return (BsonType == BsonType.Null) ? null : (Guid?)AsGuid; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Int32} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public int? AsNullableInt32
        {
            get { return (BsonType == BsonType.Null) ? null : (int?)AsInt32; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Int64} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public long? AsNullableInt64
        {
            get { return (BsonType == BsonType.Null) ? null : (long?)AsInt64; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{ObjectId} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public ObjectId? AsNullableObjectId
        {
            get { return (BsonType == BsonType.Null) ? null : (ObjectId?)AsObjectId; }
        }

        /// <summary>
        /// Casts the BsonValue to an ObjectId (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public ObjectId AsObjectId
        {
            get { return ((BsonObjectId)this).Value; }
        }

        /// <summary>
        /// Casts the BsonValue to a Regex (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public Regex AsRegex
        {
            get { return ((BsonRegularExpression)this).ToRegex(); }
        }

        /// <summary>
        /// Casts the BsonValue to a String (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public string AsString
        {
            get { return ((BsonString)this).Value; }
        }

        /// <summary>
        /// Casts the BsonValue to a DateTime in UTC (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        [Obsolete("Use ToUniversalTime instead.")]
        public DateTime AsUniversalTime
        {
            get { return ((BsonDateTime)this).ToUniversalTime(); }
        }

        /// <summary>
        /// Gets the BsonType of this BsonValue.
        /// </summary>
        public abstract BsonType BsonType { get; }

        /// <summary>
        /// Tests whether this BsonValue is a Boolean.
        /// </summary>
        public bool IsBoolean
        {
            get { return BsonType == BsonType.Boolean; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonArray.
        /// </summary>
        public bool IsBsonArray
        {
            get { return BsonType == BsonType.Array; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonBinaryData.
        /// </summary>
        public bool IsBsonBinaryData
        {
            get { return BsonType == BsonType.Binary; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonDateTime.
        /// </summary>
        public bool IsBsonDateTime
        {
            get { return BsonType == BsonType.DateTime; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonDocument.
        /// </summary>
        public bool IsBsonDocument
        {
            get { return BsonType == BsonType.Document; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonJavaScript.
        /// </summary>
        public bool IsBsonJavaScript
        {
            get { return BsonType == BsonType.JavaScript || BsonType == BsonType.JavaScriptWithScope; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonJavaScriptWithScope.
        /// </summary>
        public bool IsBsonJavaScriptWithScope
        {
            get { return BsonType == BsonType.JavaScriptWithScope; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonMaxKey.
        /// </summary>
        public bool IsBsonMaxKey
        {
            get { return BsonType == BsonType.MaxKey; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonMinKey.
        /// </summary>
        public bool IsBsonMinKey
        {
            get { return BsonType == BsonType.MinKey; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonNull.
        /// </summary>
        public bool IsBsonNull
        {
            get { return BsonType == BsonType.Null; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonRegularExpression.
        /// </summary>
        public bool IsBsonRegularExpression
        {
            get { return BsonType == BsonType.RegularExpression; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonSymbol .
        /// </summary>
        public bool IsBsonSymbol
        {
            get { return BsonType == BsonType.Symbol; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonTimestamp.
        /// </summary>
        public bool IsBsonTimestamp
        {
            get { return BsonType == BsonType.Timestamp; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonUndefined.
        /// </summary>
        public bool IsBsonUndefined
        {
            get { return BsonType == BsonType.Undefined; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a DateTime.
        /// </summary>
        [Obsolete("Use IsValidDateTime instead.")]
        public bool IsDateTime
        {
            get { return IsValidDateTime; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a Decimal128.
        /// </summary>
        public bool IsDecimal128
        {
            get { return BsonType == BsonType.Decimal128; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a Double.
        /// </summary>
        public bool IsDouble
        {
            get { return BsonType == BsonType.Double; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a Guid.
        /// </summary>
        public bool IsGuid
        {
            get
            {
                if (BsonType == BsonType.Binary)
                {
                    var subType = ((BsonBinaryData)this).SubType;
                    return subType == BsonBinarySubType.UuidStandard || subType == BsonBinarySubType.UuidLegacy;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Tests whether this BsonValue is an Int32.
        /// </summary>
        public bool IsInt32
        {
            get { return BsonType == BsonType.Int32; }
        }

        /// <summary>
        /// Tests whether this BsonValue is an Int64.
        /// </summary>
        public bool IsInt64
        {
            get { return BsonType == BsonType.Int64; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a numeric value.
        /// </summary>
        public bool IsNumeric
        {
            get
            {
                return
                    BsonType == BsonType.Decimal128 ||
                    BsonType == BsonType.Double ||
                    BsonType == BsonType.Int32 ||
                    BsonType == BsonType.Int64;
            }
        }

        /// <summary>
        /// Tests whether this BsonValue is an ObjectId .
        /// </summary>
        public bool IsObjectId
        {
            get { return BsonType == BsonType.ObjectId; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a String.
        /// </summary>
        public bool IsString
        {
            get { return BsonType == BsonType.String; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a valid DateTime.
        /// </summary>
        public virtual bool IsValidDateTime
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the raw value of this BsonValue (or null if this BsonValue doesn't have a single scalar value).
        /// </summary>
        // note: don't change return value to "this" or lots of things will break
        [Obsolete("Use Value property of subclasses or BsonTypeMapper.MapToDotNetValue instead.")]
        public virtual object RawValue
        {
            get { return null; } // subclasses that have a single value (e.g. Int32) override this
        }

        // public operators
        /// <summary>
        /// Casts a BsonValue to a bool.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A bool.</returns>
        public static explicit operator bool(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value.AsBoolean;
        }

        /// <summary>
        /// Casts a BsonValue to a bool?.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A bool?.</returns>
        public static explicit operator bool?(BsonValue value)
        {
            return (value == null) ? null : value.AsNullableBoolean;
        }

        /// <summary>
        /// Converts a bool to a BsonValue.
        /// </summary>
        /// <param name="value">A bool.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(bool value)
        {
            return (BsonBoolean)value;
        }

        /// <summary>
        /// Converts a bool? to a BsonValue.
        /// </summary>
        /// <param name="value">A bool?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(bool? value)
        {
            return value.HasValue ? (BsonValue)(BsonBoolean)value.Value : BsonNull.Value;
        }

        /// <summary>
        /// Converts a byte[] to a BsonValue.
        /// </summary>
        /// <param name="value">A byte[].</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(byte[] value)
        {
            return (value != null) ? (BsonValue)new BsonBinaryData(value) : null;
        }

        /// <summary>
        /// Converts a DateTime to a BsonValue.
        /// </summary>
        /// <param name="value">A DateTime.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(DateTime value)
        {
            return new BsonDateTime(value);
        }

        /// <summary>
        /// Converts a DateTime? to a BsonValue.
        /// </summary>
        /// <param name="value">A DateTime?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(DateTime? value)
        {
            return value.HasValue ? (BsonValue)new BsonDateTime(value.Value) : BsonNull.Value;
        }

        /// <summary>
        /// Converts a decimal to a BsonValue.
        /// </summary>
        /// <param name="value">A decimal.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(decimal value)
        {
            return (BsonDecimal128)(Decimal128)value;
        }

        /// <summary>
        /// Converts a decimal? to a BsonValue.
        /// </summary>
        /// <param name="value">A decimal?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(decimal? value)
        {
            return value.HasValue ? (BsonValue)(BsonDecimal128)(Decimal128)value.Value : BsonNull.Value;
        }

        /// <summary>
        /// Converts a <see cref="Decimal128"/> to a BsonValue.
        /// </summary>
        /// <param name="value">A Decimal128.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(Decimal128 value)
        {
            return (BsonDecimal128)value;
        }

        /// <summary>
        /// Converts a nullable <see cref="Decimal128"/> to a BsonValue.
        /// </summary>
        /// <param name="value">A Decimal128?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(Decimal128? value)
        {
            return value.HasValue ? (BsonValue)(BsonDecimal128)value.Value : BsonNull.Value;
        }

        /// <summary>
        /// Converts a double to a BsonValue.
        /// </summary>
        /// <param name="value">A double.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(double value)
        {
            return (BsonDouble)value;
        }

        /// <summary>
        /// Converts a double? to a BsonValue.
        /// </summary>
        /// <param name="value">A double?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(double? value)
        {
            return value.HasValue ? (BsonValue)(BsonDouble)value.Value : BsonNull.Value;
        }

        /// <summary>
        /// Converts an Enum to a BsonValue.
        /// </summary>
        /// <param name="value">An Enum.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(Enum value)
        {
            return BsonTypeMapper.MapToBsonValue(value);
        }

        /// <summary>
        /// Converts a Guid to a BsonValue.
        /// </summary>
        /// <param name="value">A Guid.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(Guid value)
        {
            return new BsonBinaryData(value);
        }

        /// <summary>
        /// Converts a Guid? to a BsonValue.
        /// </summary>
        /// <param name="value">A Guid?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(Guid? value)
        {
            return value.HasValue ? (BsonValue)new BsonBinaryData(value.Value) : BsonNull.Value;
        }

        /// <summary>
        /// Converts an int to a BsonValue.
        /// </summary>
        /// <param name="value">An int.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(int value)
        {
            return (BsonInt32)value;
        }

        /// <summary>
        /// Converts an int? to a BsonValue.
        /// </summary>
        /// <param name="value">An int?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(int? value)
        {
            return value.HasValue ? (BsonValue)(BsonInt32)value.Value : BsonNull.Value;
        }

        /// <summary>
        /// Converts a long to a BsonValue.
        /// </summary>
        /// <param name="value">A long.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(long value)
        {
            return (BsonInt64)value;
        }

        /// <summary>
        /// Converts a long? to a BsonValue.
        /// </summary>
        /// <param name="value">A long?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(long? value)
        {
            return value.HasValue ? (BsonValue)(BsonInt64)value.Value : BsonNull.Value;
        }

        /// <summary>
        /// Converts an ObjectId to a BsonValue.
        /// </summary>
        /// <param name="value">An ObjectId.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(ObjectId value)
        {
            return new BsonObjectId(value);
        }

        /// <summary>
        /// Converts an ObjectId? to a BsonValue.
        /// </summary>
        /// <param name="value">An ObjectId?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(ObjectId? value)
        {
            return value.HasValue ? (BsonValue)new BsonObjectId(value.Value) : BsonNull.Value;
        }

        /// <summary>
        /// Converts a Regex to a BsonValue.
        /// </summary>
        /// <param name="value">A Regex.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(Regex value)
        {
            return (value != null) ? (BsonValue)new BsonRegularExpression(value) : null;
        }

        /// <summary>
        /// Converts a string to a BsonValue.
        /// </summary>
        /// <param name="value">A string.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(string value)
        {
            return (value != null) ? (BsonValue)(BsonString)value : null;
        }

        /// <summary>
        /// Casts a BsonValue to a byte[].
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A byte[].</returns>
        public static explicit operator byte[](BsonValue value)
        {
            return (value == null) ? null : value.AsByteArray;
        }

        /// <summary>
        /// Casts a BsonValue to a DateTime.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A DateTime.</returns>
        public static explicit operator DateTime(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is BsonDateTime))
            {
                throw new InvalidCastException();
            }
            return value.ToUniversalTime();
        }

        /// <summary>
        /// Casts a BsonValue to a DateTime?.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A DateTime?.</returns>
        public static explicit operator DateTime?(BsonValue value)
        {
            if (value != null && !((value is BsonDateTime) || (value is BsonNull)))
            {
                throw new InvalidCastException();
            }
            return (value == null) ? null : value.ToNullableUniversalTime();
        }

        /// <summary>
        /// Casts a BsonValue to a decimal.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A decimal.</returns>
        public static explicit operator decimal(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value.AsDecimal;
        }

        /// <summary>
        /// Casts a BsonValue to a decimal?.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A decimal?.</returns>
        public static explicit operator decimal?(BsonValue value)
        {
            return (value == null) ? null : value.AsNullableDecimal;
        }

        /// <summary>
        /// Casts a BsonValue to a <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A <see cref="Decimal128"/>.</returns>
        public static explicit operator Decimal128(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value.AsDecimal128;
        }

        /// <summary>
        /// Casts a BsonValue to a nullable <see cref="Decimal128"/>?.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A nullable <see cref="Decimal128"/>.</returns>
        public static explicit operator Decimal128?(BsonValue value)
        {
            return (value == null) ? null : value.AsNullableDecimal128;
        }

        /// <summary>
        /// Casts a BsonValue to a double.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A double.</returns>
        public static explicit operator double(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value.AsDouble;
        }

        /// <summary>
        /// Casts a BsonValue to a double?.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A double?.</returns>
        public static explicit operator double?(BsonValue value)
        {
            return (value == null) ? null : value.AsNullableDouble;
        }

        /// <summary>
        /// Casts a BsonValue to a Guid.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A Guid.</returns>
        public static explicit operator Guid(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value.AsGuid;
        }

        /// <summary>
        /// Casts a BsonValue to a Guid?.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A Guid?.</returns>
        public static explicit operator Guid?(BsonValue value)
        {
            return (value == null) ? null : value.AsNullableGuid;
        }

        /// <summary>
        /// Casts a BsonValue to an int.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>An int.</returns>
        public static explicit operator int(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value.AsInt32;
        }

        /// <summary>
        /// Casts a BsonValue to an int?.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>An int?.</returns>
        public static explicit operator int?(BsonValue value)
        {
            return value == null ? null : value.AsNullableInt32;
        }

        /// <summary>
        /// Casts a BsonValue to a long.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A long.</returns>
        public static explicit operator long(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value.AsInt64;
        }

        /// <summary>
        /// Casts a BsonValue to a long?.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A long?.</returns>
        public static explicit operator long?(BsonValue value)
        {
            return (value == null) ? null : value.AsNullableInt64;
        }

        /// <summary>
        /// Casts a BsonValue to an ObjectId.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>An ObjectId.</returns>
        public static explicit operator ObjectId(BsonValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value.AsObjectId;
        }

        /// <summary>
        /// Casts a BsonValue to an ObjectId?.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>An ObjectId?.</returns>
        public static explicit operator ObjectId?(BsonValue value)
        {
            return (value == null) ? null : value.AsNullableObjectId;
        }

        /// <summary>
        /// Casts a BsonValue to a Regex.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A Regex.</returns>
        public static explicit operator Regex(BsonValue value)
        {
            return (value == null) ? null : value.AsRegex;
        }

        /// <summary>
        /// Casts a BsonValue to a string.
        /// </summary>
        /// <param name="value">The BsonValue.</param>
        /// <returns>A string.</returns>
        public static explicit operator string(BsonValue value)
        {
            return (value == null) ? null : value.AsString;
        }

        /// <summary>
        /// Compares two BsonValues.
        /// </summary>
        /// <param name="lhs">The first BsonValue.</param>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if the first BsonValue is less than the other one.</returns>
        public static bool operator <(BsonValue lhs, BsonValue rhs)
        {
            if (object.ReferenceEquals(lhs, null) && object.ReferenceEquals(rhs, null)) { return false; }
            if (object.ReferenceEquals(lhs, null)) { return true; }
            if (object.ReferenceEquals(rhs, null)) { return false; }
            return lhs.CompareTo(rhs) < 0;
        }

        /// <summary>
        /// Compares two BsonValues.
        /// </summary>
        /// <param name="lhs">The first BsonValue.</param>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if the first BsonValue is less than or equal to the other one.</returns>
        public static bool operator <=(BsonValue lhs, BsonValue rhs)
        {
            if (object.ReferenceEquals(lhs, null) && object.ReferenceEquals(rhs, null)) { return true; }
            if (object.ReferenceEquals(lhs, null)) { return true; }
            if (object.ReferenceEquals(rhs, null)) { return false; }
            return lhs.CompareTo(rhs) <= 0;
        }

        /// <summary>
        /// Compares two BsonValues.
        /// </summary>
        /// <param name="lhs">The first BsonValue.</param>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if the two BsonValues are not equal according to ==.</returns>
        public static bool operator !=(BsonValue lhs, BsonValue rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonValues.
        /// </summary>
        /// <param name="lhs">The first BsonValue.</param>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if the two BsonValues are equal according to ==.</returns>
        public static bool operator ==(BsonValue lhs, BsonValue rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            if (object.ReferenceEquals(rhs, null)) { return false; } // don't check type because sometimes different types can be ==
            return lhs.OperatorEqualsImplementation(rhs); // some subclasses override OperatorEqualsImplementation
        }

        /// <summary>
        /// Compares two BsonValues.
        /// </summary>
        /// <param name="lhs">The first BsonValue.</param>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if the first BsonValue is greater than the other one.</returns>
        public static bool operator >(BsonValue lhs, BsonValue rhs)
        {
            return !(lhs <= rhs);
        }

        /// <summary>
        /// Compares two BsonValues.
        /// </summary>
        /// <param name="lhs">The first BsonValue.</param>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if the first BsonValue is greater than or equal to the other one.</returns>
        public static bool operator >=(BsonValue lhs, BsonValue rhs)
        {
            return !(lhs < rhs);
        }

        // public indexers
        /// <summary>
        /// Gets or sets a value by position (only applies to BsonDocument and BsonArray).
        /// </summary>
        /// <param name="index">The position.</param>
        /// <returns>The value.</returns>
        public virtual BsonValue this[int index]
        {
            get
            {
                var message = string.Format("{0} does not support indexing by position (only BsonDocument and BsonArray do).", this.GetType().Name);
                throw new NotSupportedException(message);
            }
            set
            {
                var message = string.Format("{0} does not support indexing by position (only BsonDocument and BsonArray do).", this.GetType().Name);
                throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Gets or sets a value by name (only applies to BsonDocument).
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The value.</returns>
        public virtual BsonValue this[string name]
        {
            get
            {
                var message = string.Format("{0} does not support indexing by name (only BsonDocument does).", this.GetType().Name);
                throw new NotSupportedException(message);
            }
            set
            {
                var message = string.Format("{0} does not support indexing by name (only BsonDocument does).", this.GetType().Name);
                throw new NotSupportedException(message);
            }
        }

        // public static methods
        /// <summary>
        /// Creates a new instance of the BsonValue class.
        /// </summary>
        /// <param name="value">A value to be mapped to a BsonValue.</param>
        /// <returns>A BsonValue.</returns>
        public static BsonValue Create(object value)
        {
            // optimize away the call to MapToBsonValue for the most common cases
            if (value == null)
            {
                return BsonNull.Value;
            }
            else if (value is BsonValue)
            {
                return (BsonValue)value;
            }
            else if (value is int)
            {
                return (BsonInt32)(int)value;
            }
            else if (value is string)
            {
                return (BsonString)(string)value;
            }
            else if (value is bool)
            {
                return (BsonBoolean)((bool)value);
            }
            else if (value is DateTime)
            {
                return new BsonDateTime((DateTime)value);
            }
            else if (value is long)
            {
                return (BsonInt64)(long)value;
            }
            else if (value is double)
            {
                return (BsonDouble)(double)value;
            }
            else
            {
                return BsonTypeMapper.MapToBsonValue(value);
            }
        }

        // public methods
        /// <summary>
        /// Creates a shallow clone of the BsonValue (see also DeepClone).
        /// </summary>
        /// <returns>A shallow clone of the BsonValue.</returns>
        public virtual BsonValue Clone()
        {
            return this; // subclasses override Clone if necessary
        }

        /// <summary>
        /// Compares this BsonValue to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonValue is less than, equal to, or greather than the other BsonValue.</returns>
        public abstract int CompareTo(BsonValue other);

        /// <summary>
        /// Compares the type of this BsonValue to the type of another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether the type of this BsonValue is less than, equal to, or greather than the type of the other BsonValue.</returns>
        public int CompareTypeTo(BsonValue other)
        {
            if (object.ReferenceEquals(other, null)) { return 1; }
            return __bsonTypeSortOrder[BsonType].CompareTo(__bsonTypeSortOrder[other.BsonType]);
        }

        /// <summary>
        /// Creates a deep clone of the BsonValue (see also Clone).
        /// </summary>
        /// <returns>A deep clone of the BsonValue.</returns>
        public virtual BsonValue DeepClone()
        {
            return this; // subclasses override DeepClone if necessary
        }

        /// <summary>
        /// Compares this BsonValue to another BsonValue.
        /// </summary>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if the two BsonValue values are equal.</returns>
        public bool Equals(BsonValue rhs)
        {
            return Equals((object)rhs);
        }

        /// <summary>
        /// Compares this BsonValue to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonValue and equal to this one.</returns>
        public abstract override bool Equals(object obj);

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public abstract override int GetHashCode();

        /// <summary>
        /// Converts this BsonValue to a Boolean (using the JavaScript definition of truthiness).
        /// </summary>
        /// <returns>A Boolean.</returns>
        public virtual bool ToBoolean()
        {
            // some subclasses override as appropriate
            return true; // everything else is true        
        }

        /// <summary>
        /// Converts this BsonValue to a Decimal.
        /// </summary>
        /// <returns>A Decimal.</returns>
        public virtual decimal ToDecimal()
        {
            var message = string.Format("{0} does not support ToDecimal.", this.GetType().Name);
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Converts this BsonValue to a Decimal128.
        /// </summary>
        /// <returns>A Decimal128.</returns>
        public virtual Decimal128 ToDecimal128()
        {
            var message = string.Format("{0} does not support ToDecimal128.", this.GetType().Name);
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Converts this BsonValue to a Double.
        /// </summary>
        /// <returns>A Double.</returns>
        public virtual double ToDouble()
        {
            var message = string.Format("{0} does not support ToDouble.", this.GetType().Name);
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Converts this BsonValue to an Int32.
        /// </summary>
        /// <returns>An Int32.</returns>
        public virtual int ToInt32()
        {
            var message = string.Format("{0} does not support ToInt32.", this.GetType().Name);
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Converts this BsonValue to an Int64.
        /// </summary>
        /// <returns>An Int64.</returns>
        public virtual long ToInt64()
        {
            var message = string.Format("{0} does not support ToInt64.", this.GetType().Name);
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Converts this BsonValue to a DateTime in local time.
        /// </summary>
        /// <returns>A DateTime.</returns>
        public virtual DateTime ToLocalTime()
        {
            var message = string.Format("{0} does not support ToLocalTime.", this.GetType().Name);
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Converts this BsonValue to a DateTime? in local time.
        /// </summary>
        /// <returns>A DateTime?.</returns>
        public virtual DateTime? ToNullableLocalTime()
        {
            var message = string.Format("{0} does not support ToNullableLocalTime.", this.GetType().Name);
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Converts this BsonValue to a DateTime? in UTC.
        /// </summary>
        /// <returns>A DateTime?.</returns>
        public virtual DateTime? ToNullableUniversalTime()
        {
            var message = string.Format("{0} does not support ToNullableUniversalTime.", this.GetType().Name);
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Converts this BsonValue to a DateTime in UTC.
        /// </summary>
        /// <returns>A DateTime.</returns>
        public virtual DateTime ToUniversalTime()
        {
            var message = string.Format("{0} does not support ToUniversalTime.", this.GetType().Name);
            throw new NotSupportedException(message);
        }

        // protected methods
        /// <summary>
        /// Implementation of the IConvertible GetTypeCode method.
        /// </summary>
        /// <returns>The TypeCode.</returns>
        protected virtual TypeCode IConvertibleGetTypeCodeImplementation()
        {
            return TypeCode.Object;
        }

        /// <summary>
        /// Implementation of the IConvertible ToBoolean method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A bool.</returns>
        protected virtual bool IConvertibleToBooleanImplementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToByte method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A byte.</returns>
        protected virtual byte IConvertibleToByteImplementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToChar method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A char.</returns>
        protected virtual char IConvertibleToCharImplementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToDateTime method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A DateTime.</returns>
        protected virtual DateTime IConvertibleToDateTimeImplementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToDecimal method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A decimal.</returns>
        protected virtual decimal IConvertibleToDecimalImplementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToDouble method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A double.</returns>
        protected virtual double IConvertibleToDoubleImplementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToInt16 method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A short.</returns>
        protected virtual short IConvertibleToInt16Implementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToInt32 method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>An int.</returns>
        protected virtual int IConvertibleToInt32Implementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToInt64 method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A long.</returns>
        protected virtual long IConvertibleToInt64Implementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToSByte method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>An sbyte.</returns>
#pragma warning disable 3002
        protected virtual sbyte IConvertibleToSByteImplementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
#pragma warning restore

        /// <summary>
        /// Implementation of the IConvertible ToSingle method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A float.</returns>
        protected virtual float IConvertibleToSingleImplementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToString method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A string.</returns>
        protected virtual string IConvertibleToStringImplementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Implementation of the IConvertible ToUInt16 method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A ushort.</returns>
#pragma warning disable 3002
        protected virtual ushort IConvertibleToUInt16Implementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
#pragma warning restore

        /// <summary>
        /// Implementation of the IConvertible ToUInt32 method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A uint.</returns>
#pragma warning disable 3002
        protected virtual uint IConvertibleToUInt32Implementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
#pragma warning restore

        /// <summary>
        /// Implementation of the IConvertible ToUInt64 method.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A ulong.</returns>
#pragma warning disable 3002
        protected virtual ulong IConvertibleToUInt64Implementation(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
#pragma warning restore

        /// <summary>
        /// Implementation of operator ==.
        /// </summary>
        /// <param name="rhs">The other BsonValue.</param>
        /// <returns>True if the two BsonValues are equal according to ==.</returns>
        protected virtual bool OperatorEqualsImplementation(BsonValue rhs)
        {
            return Equals(rhs); // default implementation of == is to call Equals
        }

        // explicit IConvertible implementation
        TypeCode IConvertible.GetTypeCode()
        {
            return IConvertibleGetTypeCodeImplementation();
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return IConvertibleToBooleanImplementation(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return IConvertibleToByteImplementation(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return IConvertibleToCharImplementation(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return IConvertibleToDateTimeImplementation(provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return IConvertibleToDecimalImplementation(provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return IConvertibleToDoubleImplementation(provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return IConvertibleToInt16Implementation(provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return IConvertibleToInt32Implementation(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return IConvertibleToInt64Implementation(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return IConvertibleToSByteImplementation(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return IConvertibleToSingleImplementation(provider);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return IConvertibleToStringImplementation(provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(object))
            {
                return this;
            }

            switch (BsonType)
            {
                case BsonType.Boolean: return Convert.ChangeType(this.AsBoolean, conversionType, provider);
                case BsonType.DateTime: return Convert.ChangeType(this.ToUniversalTime(), conversionType, provider);
                case BsonType.Decimal128: return Convert.ChangeType(this.AsDecimal128, conversionType, provider);
                case BsonType.Double: return Convert.ChangeType(this.AsDouble, conversionType, provider);
                case BsonType.Int32: return Convert.ChangeType(this.AsInt32, conversionType, provider);
                case BsonType.Int64: return Convert.ChangeType(this.AsInt64, conversionType, provider);
                case BsonType.ObjectId: return Convert.ChangeType(this.AsObjectId, conversionType, provider);
                case BsonType.String: return Convert.ChangeType(this.AsString, conversionType, provider);
                default: throw new InvalidCastException();
            }
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return IConvertibleToUInt16Implementation(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return IConvertibleToUInt32Implementation(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return IConvertibleToUInt64Implementation(provider);
        }
    }
}
