﻿/* Copyright 2010-present MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents settings for a BsonReader.
    /// </summary>
    [Serializable]
    public abstract class BsonReaderSettings
    {
        // private fields
#pragma warning disable 618
        private GuidRepresentation _guidRepresentation = BsonDefaults.GuidRepresentationMode == GuidRepresentationMode.V2 ? BsonDefaults.GuidRepresentation : GuidRepresentation.Unspecified;
#pragma warning restore 618
        private bool _isFrozen;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonReaderSettings class.
        /// </summary>
        protected BsonReaderSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonReaderSettings class.
        /// </summary>
        /// <param name="guidRepresentation">The representation for Guids.</param>
        [Obsolete("Configure serializers instead.")]
        protected BsonReaderSettings(GuidRepresentation guidRepresentation)
        {
            if (BsonDefaults.GuidRepresentationMode != GuidRepresentationMode.V2)
            {
                throw new InvalidOperationException("BsonReaderSettings constructor with GuidRepresentation can only be used when GuidRepresentationMode is V2.");
            }
            _guidRepresentation = guidRepresentation;
        }

        // public properties
        /// <summary>
        /// Gets or sets the representation for Guids.
        /// </summary>
        [Obsolete("Configure serializers instead.")]
        public GuidRepresentation GuidRepresentation
        {
            get
            {
                if (BsonDefaults.GuidRepresentationMode != GuidRepresentationMode.V2)
                {
                    throw new InvalidOperationException("BsonReaderSettings.GuidRepresentation can only be used when GuidRepresentationMode is V2.");
                }
                return _guidRepresentation;
            }
            set
            {
                if (_isFrozen) { ThrowFrozenException(); }
                if (BsonDefaults.GuidRepresentationMode != GuidRepresentationMode.V2)
                {
                    throw new InvalidOperationException("BsonReaderSettings.GuidRepresentation can only be used when GuidRepresentationMode is V2.");
                }
                _guidRepresentation = value;
            }
        }

        /// <summary>
        /// Gets whether the settings are frozen.
        /// </summary>
        public bool IsFrozen
        {
            get { return _isFrozen; }
        }

        // public methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        public BsonReaderSettings Clone()
        {
            return CloneImplementation();
        }

        /// <summary>
        /// Freezes the settings.
        /// </summary>
        /// <returns>The frozen settings.</returns>
        public BsonReaderSettings Freeze()
        {
            _isFrozen = true;
            return this;
        }

        /// <summary>
        /// Returns a frozen copy of the settings.
        /// </summary>
        /// <returns>A frozen copy of the settings.</returns>
        public BsonReaderSettings FrozenCopy()
        {
            if (_isFrozen)
            {
                return this;
            }
            else
            {
                return Clone().Freeze();
            }
        }

        // protected methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        protected abstract BsonReaderSettings CloneImplementation();

        /// <summary>
        /// Throws an InvalidOperationException when an attempt is made to change a setting after the settings are frozen.
        /// </summary>
        protected void ThrowFrozenException()
        {
            var message = string.Format("{0} is frozen.", this.GetType().Name);
            throw new InvalidOperationException(message);
        }
    }
}
