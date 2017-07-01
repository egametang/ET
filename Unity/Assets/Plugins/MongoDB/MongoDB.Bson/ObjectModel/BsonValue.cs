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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON value (this is an abstract class, see the various subclasses).
    /// </summary>
    [Serializable]
    public abstract class BsonValue : IComparable<BsonValue>, IConvertible, IEquatable<BsonValue>
    {
        // private static fields
        private static Dictionary<BsonType, int> __bsonTypeSortOrder = new Dictionary<BsonType, int>
        {
            { BsonType.MinKey, 1 },
            { BsonType.Undefined, 2 },
            { BsonType.Null, 3 },
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

        // private fields
        private BsonType _bsonType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonValue class.
        /// </summary>
        /// <param name="bsonType">The BsonType of this BsonValue.</param>
        protected BsonValue(BsonType bsonType)
        {
            _bsonType = bsonType;
        }

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
            get { return (_bsonType == BsonType.Null) ? null : (bool?)AsBoolean; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{DateTime} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        [Obsolete("Use ToNullableUniversalTime instead.")]
        public DateTime? AsNullableDateTime
        {
            get { return (_bsonType == BsonType.Null) ? null : (DateTime?)AsDateTime; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Double} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public double? AsNullableDouble
        {
            get { return (_bsonType == BsonType.Null) ? null : (double?)AsDouble; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Guid} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public Guid? AsNullableGuid
        {
            get { return (_bsonType == BsonType.Null) ? null : (Guid?)AsGuid; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Int32} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public int? AsNullableInt32
        {
            get { return (_bsonType == BsonType.Null) ? null : (int?)AsInt32; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{Int64} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public long? AsNullableInt64
        {
            get { return (_bsonType == BsonType.Null) ? null : (long?)AsInt64; }
        }

        /// <summary>
        /// Casts the BsonValue to a Nullable{ObjectId} (throws an InvalidCastException if the cast is not valid).
        /// </summary>
        public ObjectId? AsNullableObjectId
        {
            get { return (_bsonType == BsonType.Null) ? null : (ObjectId?)AsObjectId; }
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
        public BsonType BsonType
        {
            get { return _bsonType; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a Boolean.
        /// </summary>
        public bool IsBoolean
        {
            get { return _bsonType == BsonType.Boolean; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonArray.
        /// </summary>
        public bool IsBsonArray
        {
            get { return _bsonType == BsonType.Array; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonBinaryData.
        /// </summary>
        public bool IsBsonBinaryData
        {
            get { return _bsonType == BsonType.Binary; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonDateTime.
        /// </summary>
        public bool IsBsonDateTime
        {
            get { return _bsonType == BsonType.DateTime; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonDocument.
        /// </summary>
        public bool IsBsonDocument
        {
            get { return _bsonType == BsonType.Document; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonJavaScript.
        /// </summary>
        public bool IsBsonJavaScript
        {
            get { return _bsonType == BsonType.JavaScript || _bsonType == BsonType.JavaScriptWithScope; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonJavaScriptWithScope.
        /// </summary>
        public bool IsBsonJavaScriptWithScope
        {
            get { return _bsonType == BsonType.JavaScriptWithScope; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonMaxKey.
        /// </summary>
        public bool IsBsonMaxKey
        {
            get { return _bsonType == BsonType.MaxKey; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonMinKey.
        /// </summary>
        public bool IsBsonMinKey
        {
            get { return _bsonType == BsonType.MinKey; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonNull.
        /// </summary>
        public bool IsBsonNull
        {
            get { return _bsonType == BsonType.Null; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonRegularExpression.
        /// </summary>
        public bool IsBsonRegularExpression
        {
            get { return _bsonType == BsonType.RegularExpression; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonSymbol .
        /// </summary>
        public bool IsBsonSymbol
        {
            get { return _bsonType == BsonType.Symbol; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonTimestamp.
        /// </summary>
        public bool IsBsonTimestamp
        {
            get { return _bsonType == BsonType.Timestamp; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a BsonUndefined.
        /// </summary>
        public bool IsBsonUndefined
        {
            get { return _bsonType == BsonType.Undefined; }
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
        /// Tests whether this BsonValue is a Double.
        /// </summary>
        public bool IsDouble
        {
            get { return _bsonType == BsonType.Double; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a Guid.
        /// </summary>
        public bool IsGuid
        {
            get
            {
                if (_bsonType == BsonType.Binary)
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
            get { return _bsonType == BsonType.Int32; }
        }

        /// <summary>
        /// Tests whether this BsonValue is an Int64.
        /// </summary>
        public bool IsInt64
        {
            get { return _bsonType == BsonType.Int64; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a numeric value.
        /// </summary>
        public bool IsNumeric
        {
            get
            {
                return
                    _bsonType == BsonType.Double ||
                    _bsonType == BsonType.Int32 ||
                    _bsonType == BsonType.Int64;
            }
        }

        /// <summary>
        /// Tests whether this BsonValue is an ObjectId .
        /// </summary>
        public bool IsObjectId
        {
            get { return _bsonType == BsonType.ObjectId; }
        }

        /// <summary>
        /// Tests whether this BsonValue is a String.
        /// </summary>
        public bool IsString
        {
            get { return _bsonType == BsonType.String; }
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
            return value ? BsonBoolean.True : BsonBoolean.False;
        }

        /// <summary>
        /// Converts a bool? to a BsonValue.
        /// </summary>
        /// <param name="value">A bool?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(bool? value)
        {
            return value.HasValue ? (BsonValue)(value.Value ? BsonBoolean.True : BsonBoolean.False) : BsonNull.Value;
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
        /// Converts a double to a BsonValue.
        /// </summary>
        /// <param name="value">A double.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(double value)
        {
            return new BsonDouble(value);
        }

        /// <summary>
        /// Converts a double? to a BsonValue.
        /// </summary>
        /// <param name="value">A double?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(double? value)
        {
            return value.HasValue ? (BsonValue)new BsonDouble(value.Value) : BsonNull.Value;
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
            return new BsonInt32(value);
        }

        /// <summary>
        /// Converts an int? to a BsonValue.
        /// </summary>
        /// <param name="value">An int?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(int? value)
        {
            return value.HasValue ? (BsonValue)new BsonInt32(value.Value) : BsonNull.Value;
        }

        /// <summary>
        /// Converts a long to a BsonValue.
        /// </summary>
        /// <param name="value">A long.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(long value)
        {
            return new BsonInt64(value);
        }

        /// <summary>
        /// Converts a long? to a BsonValue.
        /// </summary>
        /// <param name="value">A long?.</param>
        /// <returns>A BsonValue.</returns>
        public static implicit operator BsonValue(long? value)
        {
            return value.HasValue ? (BsonValue)new BsonInt64(value.Value) : BsonNull.Value;
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
            return (value != null) ? (BsonValue)new BsonString(value) : null;
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
        // TODO: implement more Create methods for .NET types (int, string, etc...)? Not sure... already have implicit conversions

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
                return null; // not BsonNull.Value to be consistent with other Create methods
            }
            else if (value is BsonValue)
            {
                return (BsonValue)value;
            }
            else if (value is int)
            {
                return new BsonInt32((int)value);
            }
            else if (value is string)
            {
                return new BsonString((string)value);
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
                return new BsonInt64((long)value);
            }
            else if (value is double)
            {
                return new BsonDouble((double)value);
            }
            else
            {
                return BsonTypeMapper.MapToBsonValue(value);
            }
        }

        /// <summary>
        /// Reads one BsonValue from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The reader.</param>
        /// <returns>A BsonValue.</returns>
        [Obsolete("Use BsonSerializer.Deserialize<BsonValue> instead.")]
        public static BsonValue ReadFrom(BsonReader bsonReader)
        {
            return BsonSerializer.Deserialize<BsonValue>(bsonReader);
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
            return __bsonTypeSortOrder[_bsonType].CompareTo(__bsonTypeSortOrder[other._bsonType]);
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
        public override bool Equals(object obj)
        {
            throw new BsonInternalException("A subclass of BsonValue did not override Equals.");
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            throw new BsonInternalException("A subclass of BsonValue did not override GetHashCode.");
        }

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

        /// <summary>
        /// Writes the BsonValue to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The writer.</param>
        [Obsolete("Use BsonSerializer.Serialize<BsonValue> instead.")]
        public void WriteTo(BsonWriter bsonWriter)
        {
            BsonSerializer.Serialize(bsonWriter, this);
        }

        // protected methods
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
            switch (_bsonType)
            {
                case BsonType.Boolean: return TypeCode.Boolean;
                case BsonType.DateTime: return TypeCode.DateTime;
                case BsonType.Double: return TypeCode.Double;
                case BsonType.Int32: return TypeCode.Int32;
                case BsonType.Int64: return TypeCode.Int64;
                case BsonType.String: return TypeCode.String;
                default: return TypeCode.Object;
            }
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return this.AsBoolean;
                case BsonType.Double: return Convert.ToBoolean(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToBoolean(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToBoolean(this.AsInt64, provider);
                case BsonType.String: return Convert.ToBoolean(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToByte(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToByte(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToByte(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToByte(this.AsInt64, provider);
                case BsonType.String: return Convert.ToByte(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Int32: return Convert.ToChar(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToChar(this.AsInt64, provider);
                case BsonType.String: return Convert.ToChar(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.DateTime: return this.ToUniversalTime();
                case BsonType.String: return Convert.ToDateTime(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToDecimal(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToDecimal(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToDecimal(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToDecimal(this.AsInt64, provider);
                case BsonType.String: return Convert.ToDecimal(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToDouble(this.AsBoolean, provider);
                case BsonType.Double: return this.AsDouble;
                case BsonType.Int32: return Convert.ToDouble(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToDouble(this.AsInt64, provider);
                case BsonType.String: return Convert.ToDouble(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToInt16(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToInt16(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToInt16(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToInt16(this.AsInt64, provider);
                case BsonType.String: return Convert.ToInt16(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToInt32(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToInt32(this.AsDouble, provider);
                case BsonType.Int32: return this.AsInt32;
                case BsonType.Int64: return Convert.ToInt32(this.AsInt64, provider);
                case BsonType.String: return Convert.ToInt32(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToInt64(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToInt64(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToInt64(this.AsInt32, provider);
                case BsonType.Int64: return this.AsInt64;
                case BsonType.String: return Convert.ToInt64(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToSByte(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToSByte(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToSByte(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToSByte(this.AsInt64, provider);
                case BsonType.String: return Convert.ToSByte(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToSingle(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToSingle(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToSingle(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToSingle(this.AsInt64, provider);
                case BsonType.String: return Convert.ToSingle(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToString(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToString(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToString(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToString(this.AsInt64, provider);
                case BsonType.ObjectId: return this.AsObjectId.ToString();
                case BsonType.String: return this.AsString;
                default: throw new InvalidCastException();
            }
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(object))
            {
                return this;
            }

            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ChangeType(this.AsBoolean, conversionType, provider);
                case BsonType.DateTime: return Convert.ChangeType(this.ToUniversalTime(), conversionType, provider);
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
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToUInt16(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToUInt16(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToUInt16(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToUInt16(this.AsInt64, provider);
                case BsonType.String: return Convert.ToUInt16(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToUInt32(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToUInt32(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToUInt16(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToUInt32(this.AsInt64, provider);
                case BsonType.String: return Convert.ToUInt32(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            switch (_bsonType)
            {
                case BsonType.Boolean: return Convert.ToUInt64(this.AsBoolean, provider);
                case BsonType.Double: return Convert.ToUInt64(this.AsDouble, provider);
                case BsonType.Int32: return Convert.ToUInt64(this.AsInt32, provider);
                case BsonType.Int64: return Convert.ToUInt16(this.AsInt64, provider);
                case BsonType.String: return Convert.ToUInt64(this.AsString, provider);
                default: throw new InvalidCastException();
            }
        }
    }
}
