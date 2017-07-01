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
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Timespans.
    /// </summary>
    public class TimeSpanSerializer : BsonBaseSerializer
    {
        // private static fields
        private static TimeSpanSerializer __instance = new TimeSpanSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the TimeSpanSerializer class.
        /// </summary>
        public TimeSpanSerializer()
            : base(new TimeSpanSerializationOptions(BsonType.String))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the TimeSpanSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static TimeSpanSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof(TimeSpan));

            // support RepresentationSerializationOptions for backward compatibility
            var representationSerializationOptions = options as RepresentationSerializationOptions;
            if (representationSerializationOptions != null)
            {
                options = new TimeSpanSerializationOptions(representationSerializationOptions.Representation);
            }
            var timeSpanSerializationOptions = EnsureSerializationOptions<TimeSpanSerializationOptions>(options);

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Double:
                    return FromDouble(bsonReader.ReadDouble(), timeSpanSerializationOptions.Units);
                case BsonType.Int32:
                    return FromInt32(bsonReader.ReadInt32(), timeSpanSerializationOptions.Units);
                case BsonType.Int64:
                    return FromInt64(bsonReader.ReadInt64(), timeSpanSerializationOptions.Units);
                case BsonType.String:
                    return TimeSpan.Parse(bsonReader.ReadString()); // not XmlConvert.ToTimeSpan (we're using .NET's format for TimeSpan)
                default:
                    var message = string.Format("Cannot deserialize TimeSpan from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            var timeSpan = (TimeSpan)value;

            // support RepresentationSerializationOptions for backward compatibility
            var representationSerializationOptions = options as RepresentationSerializationOptions;
            if (representationSerializationOptions != null)
            {
                options = new TimeSpanSerializationOptions(representationSerializationOptions.Representation);
            }
            var timeSpanSerializationOptions = EnsureSerializationOptions<TimeSpanSerializationOptions>(options);

            switch (timeSpanSerializationOptions.Representation)
            {
                case BsonType.Double:
                    bsonWriter.WriteDouble(ToDouble(timeSpan, timeSpanSerializationOptions.Units));
                    break;
                case BsonType.Int32:
                    bsonWriter.WriteInt32(ToInt32(timeSpan, timeSpanSerializationOptions.Units));
                    break;
                case BsonType.Int64:
                    bsonWriter.WriteInt64(ToInt64(timeSpan, timeSpanSerializationOptions.Units));
                    break;
                case BsonType.String:
                    bsonWriter.WriteString(timeSpan.ToString()); // not XmlConvert.ToString (we're using .NET's format for TimeSpan)
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid TimeSpan representation.", timeSpanSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
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
    }
}
