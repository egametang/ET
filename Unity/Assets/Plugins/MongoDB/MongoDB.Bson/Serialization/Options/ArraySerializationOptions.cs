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
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization.Options
{
    /// <summary>
    /// Represents serialization options for an Array value.
    /// </summary>
    public class ArraySerializationOptions : BsonBaseSerializationOptions
    {
        // private fields
        private IBsonSerializationOptions _itemSerializationOptions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the ArraySerializationOptions class.
        /// </summary>
        public ArraySerializationOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ArraySerializationOptions class.
        /// </summary>
        /// <param name="itemSerializationOptions">The serialization options to use for items in the array.</param>
        public ArraySerializationOptions(IBsonSerializationOptions itemSerializationOptions)
        {
            _itemSerializationOptions = itemSerializationOptions;
        }

        // public properties
        /// <summary>
        /// Gets or sets the serialization options for the items in the array.
        /// </summary>
        public IBsonSerializationOptions ItemSerializationOptions
        {
            get { return _itemSerializationOptions; }
            set
            {
                EnsureNotFrozen();
                _itemSerializationOptions = value;
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
            var arraySerializer = serializer as IBsonArraySerializer;
            if (arraySerializer == null)
            {
                var message = string.Format(
                        "A serialization options attribute of type {0} cannot be used when the serializer is of type {1}.",
                        BsonUtils.GetFriendlyTypeName(attribute.GetType()),
                        BsonUtils.GetFriendlyTypeName(serializer.GetType()));
                throw new NotSupportedException(message);
            }

            var itemSerializer = arraySerializer.GetItemSerializationInfo().Serializer;
            if (_itemSerializationOptions == null)
            {
                var itemDefaultSerializationOptions = itemSerializer.GetDefaultSerializationOptions();

                // special case for legacy collections: allow BsonRepresentation on object
                if (itemDefaultSerializationOptions == null &&
                    (serializer.GetType() == typeof(EnumerableSerializer) || serializer.GetType() == typeof(QueueSerializer) || serializer.GetType() == typeof(StackSerializer)) &&
                    attribute.GetType() == typeof(BsonRepresentationAttribute))
                {
                    itemDefaultSerializationOptions = new RepresentationSerializationOptions(BsonType.Null); // will be modified later by ApplyAttribute
                }

                if (itemDefaultSerializationOptions == null)
                {
                    var message = string.Format(
                        "A serialization options attribute of type {0} cannot be used when the serializer is of type {1} and the item serializer is of type {2}.",
                        BsonUtils.GetFriendlyTypeName(attribute.GetType()),
                        BsonUtils.GetFriendlyTypeName(serializer.GetType()),
                        BsonUtils.GetFriendlyTypeName(itemSerializer.GetType()));
                    throw new NotSupportedException(message);
                }

                _itemSerializationOptions = itemDefaultSerializationOptions.Clone();
            }
            _itemSerializationOptions.ApplyAttribute(itemSerializer, attribute);
        }

        /// <summary>
        /// Clones the serialization options.
        /// </summary>
        /// <returns>A cloned copy of the serialization options.</returns>
        public override IBsonSerializationOptions Clone()
        {
            return new ArraySerializationOptions(_itemSerializationOptions);
        }

        /// <summary>
        /// Freezes the serialization options.
        /// </summary>
        /// <returns>The frozen serialization options.</returns>
        public override IBsonSerializationOptions Freeze()
        {
            if (!IsFrozen)
            {
                if (_itemSerializationOptions != null)
                {
                    _itemSerializationOptions.Freeze();
                }
            }
            return base.Freeze();
        }
    }
}
