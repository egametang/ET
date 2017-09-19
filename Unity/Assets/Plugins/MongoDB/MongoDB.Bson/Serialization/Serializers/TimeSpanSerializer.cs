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
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Timespans.
    /// </summary>
    public class TimeSpanSerializer : StructSerializerBase<TimeSpan>, IRepresentationConfigurable<TimeSpanSerializer>
    {
        // private fields
        private readonly BsonType _representation;
        private readonly TimeSpanUnits _units;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanSerializer"/> class.
        /// </summary>
        public TimeSpanSerializer()
            : this(BsonType.String)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public TimeSpanSerializer(BsonType representation)
            : this(representation, TimeSpanUnits.Ticks)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <param name="units">The units.</param>
        public TimeSpanSerializer(BsonType representation, TimeSpanUnits units)
        {
            switch (representation)
            {
                case BsonType.Double:
                case BsonType.Int32:
                case BsonType.Int64:
                case BsonType.String:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for a TimeSpanSerializer.", representation);
                    throw new ArgumentException(message);
            }

            _representation = representation;
            _units = units;
        }

        // public properties
        /// <summary>
        /// Gets the representation.
        /// </summary>
        /// <value>
        /// The representation.
        /// </value>
        public BsonType Representation
        {
            get { return _representation; }
        }

        /// <summary>
        /// Gets the units.
        /// </summary>
        /// <value>
        /// The units.
        /// </value>
        public TimeSpanUnits Units
        {
            get { return _units; }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override TimeSpan Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Double:
                    return FromDouble(bsonReader.ReadDouble(), _units);

                case BsonType.Int32:
                    return FromInt32(bsonReader.ReadInt32(), _units);

                case BsonType.Int64:
                    return FromInt64(bsonReader.ReadInt64(), _units);

                case BsonType.String:
                    return TimeSpan.Parse(bsonReader.ReadString()); // not XmlConvert.ToTimeSpan (we're using .NET's format for TimeSpan)

                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeSpan value)
        {
            var bsonWriter = context.Writer;

            switch (_representation)
            {
                case BsonType.Double:
                    bsonWriter.WriteDouble(ToDouble(value, _units));
                    break;

                case BsonType.Int32:
                    bsonWriter.WriteInt32(ToInt32(value, _units));
                    break;

                case BsonType.Int64:
                    bsonWriter.WriteInt64(ToInt64(value, _units));
                    break;

                case BsonType.String:
                    bsonWriter.WriteString(value.ToString()); // not XmlConvert.ToString (we're using .NET's format for TimeSpan)
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid TimeSpan representation.", _representation);
                    throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public TimeSpanSerializer WithRepresentation(BsonType representation)
        {
            if (representation == _representation)
            {
                return this;
            }
            else
            {
                return new TimeSpanSerializer(representation, _units);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation and units.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <param name="units">The units.</param>
        /// <returns>
        /// The reconfigured serializer.
        /// </returns>
        public TimeSpanSerializer WithRepresentation(BsonType representation, TimeSpanUnits units)
        {
            if (representation == _representation && units == _units)
            {
                return this;
            }
            else
            {
                return new TimeSpanSerializer(representation, units);
            }
        }

        // private methods
        private TimeSpan FromDouble(double value, TimeSpanUnits units)
        {
            if (units == TimeSpanUnits.Nanoseconds)
            {
                return TimeSpan.FromTicks((long)(value / 100.0)); // divide first then cast to reduce chance of overflow
            }
            else
            {
                return TimeSpan.FromTicks((long)(value * TicksPerUnit(units))); // multiply first then cast to preserve fractional part of value
            }
        }

        private TimeSpan FromInt32(int value, TimeSpanUnits units)
        {
            if (units == TimeSpanUnits.Nanoseconds)
            {
                return TimeSpan.FromTicks(value / 100);
            }
            else
            {
                return TimeSpan.FromTicks(value * TicksPerUnit(units));
            }
        }

        private TimeSpan FromInt64(long value, TimeSpanUnits units)
        {
            if (units == TimeSpanUnits.Nanoseconds)
            {
                return TimeSpan.FromTicks(value / 100);
            }
            else
            {
                return TimeSpan.FromTicks(value * TicksPerUnit(units));
            }
        }

        private long TicksPerUnit(TimeSpanUnits units)
        {
            switch (units)
            {
                case TimeSpanUnits.Days: return TimeSpan.TicksPerDay;
                case TimeSpanUnits.Hours: return TimeSpan.TicksPerHour;
                case TimeSpanUnits.Minutes: return TimeSpan.TicksPerMinute;
                case TimeSpanUnits.Seconds: return TimeSpan.TicksPerSecond;
                case TimeSpanUnits.Milliseconds: return TimeSpan.TicksPerMillisecond;
                case TimeSpanUnits.Microseconds: return TimeSpan.TicksPerMillisecond / 1000;
                case TimeSpanUnits.Ticks: return 1;
                default:
                    var message = string.Format("Invalid TimeSpanUnits value: {0}.", units);
                    throw new ArgumentException(message);
            }
        }

        private double ToDouble(TimeSpan timeSpan, TimeSpanUnits units)
        {
            if (units == TimeSpanUnits.Nanoseconds)
            {
                return (double)(timeSpan.Ticks) * 100.0;
            }
            else
            {
                return (double)timeSpan.Ticks / (double)TicksPerUnit(units); // cast first then divide to preserve fractional part of result
            }
        }

        private int ToInt32(TimeSpan timeSpan, TimeSpanUnits units)
        {
            if (units == TimeSpanUnits.Nanoseconds)
            {
                return (int)(timeSpan.Ticks * 100); 
            }
            else
            {
                return (int)(timeSpan.Ticks / TicksPerUnit(units));
            }
        }

        private long ToInt64(TimeSpan timeSpan, TimeSpanUnits units)
        {
            if (units == TimeSpanUnits.Nanoseconds)
            {
                return timeSpan.Ticks * 100;
            }
            else
            {
                return timeSpan.Ticks / TicksPerUnit(units);
            }
        }

        // explicit interface implementations
        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}
