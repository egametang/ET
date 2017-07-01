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
    /// Represents serialization options for a TimeSpan value.
    /// </summary>
    public class TimeSpanSerializationOptions : BsonBaseSerializationOptions
    {
        // private fields
        private BsonType _representation;
        private TimeSpanUnits _units;

        // constructors
        /// <summary>
        /// Initializes a new instance of the TimeSpanSerializationOptions class.
        /// </summary>
        /// <param name="representation">The representation for serialized TimeSpans.</param>
        public TimeSpanSerializationOptions(BsonType representation)
            : this(representation, TimeSpanUnits.Ticks)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TimeSpanSerializationOptions class.
        /// </summary>
        /// <param name="representation">The representation for serialized TimeSpans.</param>
        /// <param name="units">The units for serialized TimeSpans.</param>
        public TimeSpanSerializationOptions(BsonType representation, TimeSpanUnits units)
        {
            _representation = representation;
            _units = units;
        }

        // public properties
        /// <summary>
        /// Gets the representation for serialized TimeSpans.
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
        /// Gets the units for serialized TimeSpans.
        /// </summary>
        public TimeSpanUnits Units
        {
            get { return _units; }
            set
            {
                EnsureNotFrozen();
                _units = value;
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
                _representation = representationAttribute.Representation;
                return;
            }

            var optionsAttribute = attribute as BsonTimeSpanOptionsAttribute;
            if (optionsAttribute != null)
            {
                _representation = optionsAttribute.Representation;
                _units = optionsAttribute.Units;
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
            return new TimeSpanSerializationOptions(_representation, _units);
        }
    }
}
