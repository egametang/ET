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
using System.Globalization;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for DateTimes.
    /// </summary>
    public class DateTimeSerializer : BsonBaseSerializer
    {
        // private static fields
        private static DateTimeSerializer __instance = new DateTimeSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the DateTimeSerializer class.
        /// </summary>
        public DateTimeSerializer()
#pragma warning disable 618
            : this(DateTimeSerializationOptions.Defaults)
#pragma warning restore
        {
        }

        /// <summary>
        /// Initializes a new instance of the DateTimeSerializer class.
        /// </summary>
        /// <param name="defaultSerializationOptions">The default serialization options.</param>
        public DateTimeSerializer(DateTimeSerializationOptions defaultSerializationOptions)
            : base(defaultSerializationOptions)
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the DateTimeSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static DateTimeSerializer Instance
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
            VerifyTypes(nominalType, actualType, typeof(DateTime));
            var dateTimeSerializationOptions = EnsureSerializationOptions<DateTimeSerializationOptions>(options);

            var bsonType = bsonReader.GetCurrentBsonType();
            DateTime value;
            switch (bsonType)
            {
                case BsonType.DateTime:
                    // use an intermediate BsonDateTime so MinValue and MaxValue are handled correctly
                    value = new BsonDateTime(bsonReader.ReadDateTime()).ToUniversalTime();
                    break;
                case BsonType.Document:
                    bsonReader.ReadStartDocument();
                    bsonReader.ReadDateTime("DateTime"); // ignore value (use Ticks instead)
                    value = new DateTime(bsonReader.ReadInt64("Ticks"), DateTimeKind.Utc);
                    bsonReader.ReadEndDocument();
                    break;
                case BsonType.Int64:
                    value = DateTime.SpecifyKind(new DateTime(bsonReader.ReadInt64()), DateTimeKind.Utc);
                    break;
                case BsonType.String:
                    // note: we're not using XmlConvert because of bugs in Mono
                    if (dateTimeSerializationOptions.DateOnly)
                    {
                        value = DateTime.SpecifyKind(DateTime.ParseExact(bsonReader.ReadString(), "yyyy-MM-dd", null), DateTimeKind.Utc);
                    }
                    else
                    {
                        var formats = new string[] { "yyyy-MM-ddK", "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK" };
                        value = DateTime.ParseExact(bsonReader.ReadString(), formats, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
                    }
                    break;
                default:
                    var message = string.Format("Cannot deserialize DateTime from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }

            if (dateTimeSerializationOptions.DateOnly)
            {
                if (value.TimeOfDay != TimeSpan.Zero)
                {
                    throw new Exception("TimeOfDay component for DateOnly DateTime value is not zero.");
                }
                value = DateTime.SpecifyKind(value, dateTimeSerializationOptions.Kind); // not ToLocalTime or ToUniversalTime!
            }
            else
            {
                switch (dateTimeSerializationOptions.Kind)
                {
                    case DateTimeKind.Local:
                    case DateTimeKind.Unspecified:
                        value = DateTime.SpecifyKind(BsonUtils.ToLocalTime(value), dateTimeSerializationOptions.Kind);
                        break;
                    case DateTimeKind.Utc:
                        value = BsonUtils.ToUniversalTime(value);
                        break;
                }
            }

            return value;
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
            var dateTime = (DateTime)value;
            var dateTimeSerializationOptions = EnsureSerializationOptions<DateTimeSerializationOptions>(options);

            DateTime utcDateTime;
            if (dateTimeSerializationOptions.DateOnly)
            {
                if (dateTime.TimeOfDay != TimeSpan.Zero)
                {
                    throw new BsonSerializationException("TimeOfDay component is not zero.");
                }
                utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc); // not ToLocalTime
            }
            else
            {
                utcDateTime = BsonUtils.ToUniversalTime(dateTime);
            }
            var millisecondsSinceEpoch = BsonUtils.ToMillisecondsSinceEpoch(utcDateTime);

            switch (dateTimeSerializationOptions.Representation)
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
                    if (dateTimeSerializationOptions.DateOnly)
                    {
                        bsonWriter.WriteString(dateTime.ToString("yyyy-MM-dd"));
                    }
                    else
                    {
                        // we're not using XmlConvert.ToString because of bugs in Mono
                        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
                        {
                            // serialize MinValue and MaxValue as Unspecified so we do NOT get the time zone offset
                            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
                        }
                        else if (dateTime.Kind == DateTimeKind.Unspecified)
                        {
                            // serialize Unspecified as Local se we get the time zone offset
                            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
                        }
                        bsonWriter.WriteString(dateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK"));
                    }
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid DateTime representation.", dateTimeSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
            }
        }
    }
}
