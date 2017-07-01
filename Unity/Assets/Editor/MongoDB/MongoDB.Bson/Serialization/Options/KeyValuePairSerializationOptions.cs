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
    /// Represents serialization options for a KeyValuePair.
    /// </summary>
    public class KeyValuePairSerializationOptions : BsonBaseSerializationOptions
    {
        // private static fields
        private static KeyValuePairSerializationOptions __defaults = (KeyValuePairSerializationOptions)new KeyValuePairSerializationOptions().Freeze();

        // private fields
        private BsonType _representation = BsonType.Document;
        private IBsonSerializationOptions _keySerializationOptions;
        private IBsonSerializationOptions _valueSerializationOptions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the KeyValuePairSerializationOptions class.
        /// </summary>
        public KeyValuePairSerializationOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the KeyValuePairSerializationOptions class.
        /// </summary>
        /// <param name="representation">The representation to use for the KeyValuePair.</param>
        public KeyValuePairSerializationOptions(BsonType representation)
        {
            _representation = representation;
        }

        /// <summary>
        /// Initializes a new instance of the KeyValuePairSerializationOptions class.
        /// </summary>
        /// <param name="representation">The representation to use for the KeyValuePair.</param>
        /// <param name="keySerializationOptions">The serialization options for the key.</param>
        /// <param name="valueSerializationOptions">The serialization options for the value.</param>
        public KeyValuePairSerializationOptions(BsonType representation, IBsonSerializationOptions keySerializationOptions, IBsonSerializationOptions valueSerializationOptions)
        {
            _representation = representation;
            _keySerializationOptions = keySerializationOptions;
            _valueSerializationOptions = valueSerializationOptions;
        }

        // public static properties
        /// <summary>
        /// Gets or sets the default KeyValuePair serialization options.
        /// </summary>
        public static KeyValuePairSerializationOptions Defaults
        {
            get { return __defaults; }
            set {
                if (value.IsFrozen)
                {
                    __defaults = value;
                }
                else
                {
                    __defaults = (KeyValuePairSerializationOptions)value.Freeze();
                }
            }
        }

        // public properties
        /// <summary>
        /// Gets or sets the serialization options for the key.
        /// </summary>
        public IBsonSerializationOptions KeySerializationOptions
        {
            get { return _keySerializationOptions; }
            set
            {
                EnsureNotFrozen();
                _keySerializationOptions = value;
            }
        }

        /// <summary>
        /// Gets the representation to use for the KeyValuePair.
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
        /// Gets or sets the serialization options for the value.
        /// </summary>
        public IBsonSerializationOptions ValueSerializationOptions
        {
            get { return _valueSerializationOptions; }
            set
            {
                EnsureNotFrozen();
                _valueSerializationOptions = value;
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
            }
        }

        /// <summary>
        /// Clones the serialization options.
        /// </summary>
        /// <returns>A cloned copy of the serialization options.</returns>
        public override IBsonSerializationOptions Clone()
        {
            return new KeyValuePairSerializationOptions(_representation, _keySerializationOptions, _valueSerializationOptions);
        }

        /// <summary>
        /// Freezes the serialization options.
        /// </summary>
        /// <returns>The frozen serialization options.</returns>
        public override IBsonSerializationOptions Freeze()
        {
            if (!IsFrozen)
            {
                if (_keySerializationOptions != null)
                {
                    _keySerializationOptions.Freeze();
                }
                if (_valueSerializationOptions != null)
                {
                    _valueSerializationOptions.Freeze();
                }
            }
            return base.Freeze();
        }
    }
}
