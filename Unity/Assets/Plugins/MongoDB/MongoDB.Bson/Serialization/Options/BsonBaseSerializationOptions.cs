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

namespace MongoDB.Bson.Serialization.Options
{
    /// <summary>
    /// Abstract base class for serialization options.
    /// </summary>
    public abstract class BsonBaseSerializationOptions : IBsonSerializationOptions
    {
        // private fields
        private bool _isFrozen;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBaseSerializationOptions class.
        /// </summary>
        public BsonBaseSerializationOptions()
        {
        }

        // public properties
        /// <summary>
        /// Gets whether the serialization options are frozen.
        /// </summary>
        public bool IsFrozen
        {
            get { return _isFrozen; }
        }

        // public methods
        /// <summary>
        /// Apply an attribute to these serialization options and modify the options accordingly.
        /// </summary>
        /// <param name="serializer">The serializer that these serialization options are for.</param>
        /// <param name="attribute">The serialization options attribute.</param>
        public virtual void ApplyAttribute(IBsonSerializer serializer, Attribute attribute)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Clones the serialization options.
        /// </summary>
        /// <returns>A cloned copy of the serialization options.</returns>
        public virtual IBsonSerializationOptions Clone()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Freezes the serialization options.
        /// </summary>
        /// <returns>The frozen serialization options.</returns>
        public virtual IBsonSerializationOptions Freeze()
        {
            if (!_isFrozen)
            {
                _isFrozen = true;
            }
            return this;
        }

        // protected methods
        /// <summary>
        /// Ensures that this instance is not frozen.
        /// </summary>
        protected void EnsureNotFrozen()
        {
            if (_isFrozen)
            {
                var message = string.Format("{0} is frozen.", BsonUtils.GetFriendlyTypeName(this.GetType()));
                throw new InvalidOperationException(message);
            }
        }
    }
}
