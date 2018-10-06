/* Copyright 2010-2015 MongoDB Inc.
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
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for DateTimes.
    /// </summary>
    public class DateTimeSerializer : StructSerializerBase<DateTime>, IRepresentationConfigurable<DateTimeSerializer>
    {
        // private constants
        private static class Flags
        {
            public const long DateTime = 1;
            public const long Ticks = 2;
        }

        // private static fields
        private static readonly DateTimeSerializer __dateOnlyInstance = new DateTimeSerializer(true);
        private static readonly DateTimeSerializer __localInstance = new DateTimeSerializer(DateTimeKind.Local);
        private static readonly DateTimeSerializer __utcInstance = new DateTimeSerializer(DateTimeKind.Utc);

        // private fields
        private readonly bool _dateOnly;
        private readonly SerializerHelper _helper;
        private readonly Int64Serializer _int64Serializer = new Int64Serializer();
        private readonly DateTimeKind _kind;
        private readonly BsonType _representation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSerializer"/> class.
        /// </summary>
        public DateTimeSerializer()
            : this(DateTimeKind.Utc, BsonType.DateTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSerializer"/> class.
        /// </summary>
        /// <param name="dateOnly">if set to <c>true</c> [date only].</param>
        public DateTimeSerializer(bool dateOnly)
            : this(dateOnly, BsonType.DateTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSerializer"/> class.
        /// </summary>
        /// <param name="dateOnly">if set to <c>true</c> [date only].</param>
        /// <param name="representation">The representation.</param>
        public DateTimeSerializer(bool dateOnly, BsonType representation)
            : this(dateOnly, DateTimeKind.Utc, representation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public DateTimeSerializer(BsonType representation)
            : this(DateTimeKind.Utc, representation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSerializer"/> class.
        /// </summary>
        /// <param name="kind">The kind.</param>
        public DateTimeSerializer(DateTimeKind kind)
            : this(kind, BsonType.DateTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSerializer"/> class.
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <param name="representation">The representation.</param>
        public DateTimeSerializer(DateTimeKind kind, BsonType representation)
            : this(false, kind, representation)
        {
        }

        private DateTimeSerializer(bool dateOnly, DateTimeKind kind, BsonType representation)
        {
            switch (representation)
            {
                case BsonType.DateTime:
                case BsonType.Document:
                case BsonType.Int64:
                case BsonType.String:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for a DateTimeSerializer.", representation);
                    throw new ArgumentException(message);
            }

            _dateOnly = dateOnly;
            _kind = kind;
            _representation = representation;

            _helper = new SerializerHelper
            (
                new SerializerHelper.Member("DateTime", Flags.DateTime),
                new SerializerHelper.Member("Ticks", Flags.Ticks)
            );
        }

        // public static properties
        /// <summary>
        /// Gets an instance of DateTimeSerializer with DateOnly=true.
        /// </summary>
        public static DateTimeSerializer DateOnlyInstance
        {
            get { return __dateOnlyInstance; }
        }

        /// <summary>
        /// Gets an instance of DateTimeSerializer with Kind=Local.
        /// </summary>
        public static DateTimeSerializer LocalInstance
        {
            get { return __localInstance; }
        }

        /// <summary>
        /// Gets an instance of DateTimeSerializer with Kind=Utc.
        /// </summary>
        public static DateTimeSerializer UtcInstance
        {
            get { return __utcInstance; }
        }

        // public properties
        /// <summary>
        /// Gets whether this DateTime consists of a Date only.
        /// </summary>
        public bool DateOnly
        {
            get { return _dateOnly; }
        }

        /// <summary>
        /// Gets the DateTimeKind (Local, Unspecified or Utc).
        /// </summary>
        public DateTimeKind Kind
        {
            get { return _kind; }
        }

        /// <summary>
        /// Gets the external representation.
        /// </summary>
        /// <value>
        /// The representation.
        /// </value>
        public BsonType Representation
        {
            get { return _representation; }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            DateTime value;

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.DateTime:
                    // use an intermediate BsonDateTime so MinValue and MaxValue are handled correctly
                    value = new BsonDateTime(bsonReader.ReadDateTime()).ToUniversalTime();
                    break;

                case BsonType.Document:
                    value = default(DateTime);
                    _helper.DeserializeMembers(context, (elementName, flag) =>
                    {
                        switch (flag)
                        {
                            case Flags.DateTime: bsonReader.SkipValue(); break; // ignore value (use Ticks instead)
                            case Flags.Ticks: value = new DateTime(_int64Serializer.Deserialize(context), DateTimeKind.Utc); break;
                        }
                    });
                    break;

                case BsonType.Int64:
                    value = DateTime.SpecifyKind(new DateTime(bsonReader.ReadInt64()), DateTimeKind.Utc);
                    break;

                case BsonType.String:
                    if (_dateOnly)
                    {
                        value = DateTime.SpecifyKind(DateTime.ParseExact(bsonReader.ReadString(), "yyyy-MM-dd", null), DateTimeKind.Utc);
                    }
                    else
                    {
                        value = JsonConvert.ToDateTime(bsonReader.ReadString());
                    }
                    break;

                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }

            if (_dateOnly)
            {
                if (value.TimeOfDay != TimeSpan.Zero)
                {
                    throw new FormatException("TimeOfDay component for DateOnly DateTime value is not zero.");
                }
                value = DateTime.SpecifyKind(value, _kind); // not ToLocalTime or ToUniversalTime!
            }
            else
            {
                switch (_kind)
                {
                    case DateTimeKind.Local:
                    case DateTimeKind.Unspecified:
                        value = DateTime.SpecifyKind(BsonUtils.ToLocalTime(value), _kind);
                        break;
                    case DateTimeKind.Utc:
                        value = BsonUtils.ToUniversalTime(value);
                        break;
                }
            }

            return value;
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
        {
            var bsonWriter = context.Writer;

            DateTime utcDateTime;
            if (_dateOnly)
            {
                if (value.TimeOfDay != TimeSpan.Zero)
                {
                    throw new BsonSerializationException("TimeOfDay component is not zero.");
                }
                utcDateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc); // not ToLocalTime
            }
            else
            {
                utcDateTime = BsonUtils.ToUniversalTime(value);
            }
            var millisecondsSinceEpoch = BsonUtils.ToMillisecondsSinceEpoch(utcDateTime);

            switch (_representation)
            {
                case BsonType.DateTime:
                    bsonWriter.WriteDateTime(millisecondsSinceEpoch);
                    break;

                case BsonType.Document:
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteDateTime("DateTime", millisecondsSinceEpoch);
                    bsonWriter.WriteInt64("Ticks", utcDateTime.Ticks);
                    bsonWriter.WriteEndDocument();
                    break;

                case BsonType.Int64:
                    bsonWriter.WriteInt64(utcDateTime.Ticks);
                    break;

                case BsonType.String:
                    if (_dateOnly)
                    {
                        bsonWriter.WriteString(value.ToString("yyyy-MM-dd"));
                    }
                    else
                    {
                        if (value == DateTime.MinValue || value == DateTime.MaxValue)
                        {
                            // serialize MinValue and MaxValue as Unspecified so we do NOT get the time zone offset
                            value = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
                        }
                        else if (value.Kind == DateTimeKind.Unspecified)
                        {
                            // serialize Unspecified as Local se we get the time zone offset
                            value = DateTime.SpecifyKind(value, DateTimeKind.Local);
                        }
                        bsonWriter.WriteString(JsonConvert.ToString(value));
                    }
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid DateTime representation.", _representation);
                    throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified dateOnly value.
        /// </summary>
        /// <param name="dateOnly">if set to <c>true</c> the values will be required to be Date's only (zero time component).</param>
        /// <returns>
        /// The reconfigured serializer.
        /// </returns>
        public DateTimeSerializer WithDateOnly(bool dateOnly)
        {
            if (dateOnly == _dateOnly)
            {
                return this;
            }
            else
            {
                return new DateTimeSerializer(dateOnly, _representation);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified dateOnly value and representation.
        /// </summary>
        /// <param name="dateOnly">if set to <c>true</c> the values will be required to be Date's only (zero time component).</param>
        /// <param name="representation">The representation.</param>
        /// <returns>
        /// The reconfigured serializer.
        /// </returns>
        public DateTimeSerializer WithDateOnly(bool dateOnly, BsonType representation)
        {
            if (dateOnly == _dateOnly && representation == _representation)
            {
                return this;
            }
            else
            {
                return new DateTimeSerializer(dateOnly, representation);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified DateTimeKind value.
        /// </summary>
        /// <param name="kind">The DateTimeKind.</param>
        /// <returns>
        /// The reconfigured serializer.
        /// </returns>
        public DateTimeSerializer WithKind(DateTimeKind kind)
        {
            if (kind == _kind && _dateOnly == false)
            {
                return this;
            }
            else
            {
                return new DateTimeSerializer(kind, _representation);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified DateTimeKind value and representation.
        /// </summary>
        /// <param name="kind">The DateTimeKind.</param>
        /// <param name="representation">The representation.</param>
        /// <returns>
        /// The reconfigured serializer.
        /// </returns>
        public DateTimeSerializer WithKind(DateTimeKind kind, BsonType representation)
        {
            if (kind == _kind && representation == _representation && _dateOnly == false)
            {
                return this;
            }
            else
            {
                return new DateTimeSerializer(kind, representation);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public DateTimeSerializer WithRepresentation(BsonType representation)
        {
            if (representation == _representation)
            {
                return this;
            }
            else
            {
                if (_dateOnly)
                {
                    return new DateTimeSerializer(_dateOnly, representation);
                }
                else
                {
                    return new DateTimeSerializer(_kind, representation);
                }
            }
        }

        // explicit interface implementations
        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}
