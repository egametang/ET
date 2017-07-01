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
    /// Represents serialization options for a DateTime value.
    /// </summary>
    public class DateTimeSerializationOptions : BsonBaseSerializationOptions
    {
        // private static fields
        private static DateTimeSerializationOptions __dateOnlyInstance = new DateTimeSerializationOptions(true);
        private static DateTimeSerializationOptions __defaults = new DateTimeSerializationOptions();
        private static DateTimeSerializationOptions __localInstance = new DateTimeSerializationOptions(DateTimeKind.Local);
        private static DateTimeSerializationOptions __utcInstance = new DateTimeSerializationOptions(DateTimeKind.Utc);

        // private fields
        private bool _dateOnly = false;
        private DateTimeKind _kind = DateTimeKind.Utc;
        private BsonType _representation = BsonType.DateTime;

        // constructors
        /// <summary>
        /// Initializes a new instance of the DateTimeSerializationOptions class.
        /// </summary>
        public DateTimeSerializationOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the DateTimeSerializationOptions class.
        /// </summary>
        /// <param name="dateOnly">Whether this DateTime consists of a Date only.</param>
        public DateTimeSerializationOptions(bool dateOnly)
        {
            _dateOnly = dateOnly;
        }

        /// <summary>
        /// Initializes a new instance of the DateTimeSerializationOptions class.
        /// </summary>
        /// <param name="dateOnly">Whether this DateTime consists of a Date only.</param>
        /// <param name="representation">The external representation.</param>
        public DateTimeSerializationOptions(bool dateOnly, BsonType representation)
        {
            _dateOnly = dateOnly;
            _representation = representation;
        }

        /// <summary>
        /// Initializes a new instance of theDateTimeSerializationOptions  class.
        /// </summary>
        /// <param name="kind">The DateTimeKind (Local, Unspecified or Utc).</param>
        public DateTimeSerializationOptions(DateTimeKind kind)
        {
            _kind = kind;
        }

        /// <summary>
        /// Initializes a new instance of the DateTimeSerializationOptions class.
        /// </summary>
        /// <param name="kind">The DateTimeKind (Local, Unspecified or Utc).</param>
        /// <param name="representation">The external representation.</param>
        public DateTimeSerializationOptions(DateTimeKind kind, BsonType representation)
        {
            _kind = kind;
            _representation = representation;
        }

        // public static properties
        /// <summary>
        /// Gets an instance of DateTimeSerializationOptions with DateOnly=true.
        /// </summary>
        public static DateTimeSerializationOptions DateOnlyInstance
        {
            get { return __dateOnlyInstance; }
        }

        /// <summary>
        /// Gets or sets the default DateTime serialization options.
        /// </summary>
        [Obsolete("Create and register a DateTimeSerializer with the desired options instead.")]
        public static DateTimeSerializationOptions Defaults
        {
            get { return __defaults; }
            set { __defaults = value; }
        }

        /// <summary>
        /// Gets an instance of DateTimeSerializationOptions with Kind=Local.
        /// </summary>
        public static DateTimeSerializationOptions LocalInstance
        {
            get { return __localInstance; }
        }

        /// <summary>
        /// Gets an instance of DateTimeSerializationOptions with Kind=Utc.
        /// </summary>
        public static DateTimeSerializationOptions UtcInstance
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
            set
            {
                EnsureNotFrozen();
                _dateOnly = value;
            }
        }

        /// <summary>
        /// Gets the DateTimeKind (Local, Unspecified or Utc).
        /// </summary>
        public DateTimeKind Kind
        {
            get { return _kind; }
            set
            {
                EnsureNotFrozen();
                _kind = value;
            }
        }

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

        // public methods
        /// <summary>
        /// Apply an attribute to these serialization options and modify the options accordingly.
        /// </summary>
        /// <param name="serializer">The serializer that these serialization options are for.</param>
        /// <param name="attribute">The serialization options attribute.</param>
        public override void ApplyAttribute(IBsonSerializer serializer, Attribute attribute)
        {
            EnsureNotFrozen();
            var dateTimeSerializationOptionsAttribute = attribute as BsonDateTimeOptionsAttribute;
            if (dateTimeSerializationOptionsAttribute != null)
            {
                _dateOnly = dateTimeSerializationOptionsAttribute.DateOnly;
                _kind = dateTimeSerializationOptionsAttribute.Kind;
                _representation = dateTimeSerializationOptionsAttribute.Representation;
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
            if (_dateOnly)
            {
                return new DateTimeSerializationOptions(_dateOnly, _representation);
            }
            else
            {
                return new DateTimeSerializationOptions(_kind, _representation);
            }
        }
    }
}
