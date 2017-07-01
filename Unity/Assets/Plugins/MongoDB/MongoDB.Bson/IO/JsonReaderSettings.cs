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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents settings for a JsonReader.
    /// </summary>
    [Serializable]
    public class JsonReaderSettings : BsonReaderSettings
    {
        // private static fields
        private static JsonReaderSettings __defaults = null; // delay creation to pick up the latest default values

        // private fields
        private bool _closeInput = false;

        // constructors
        /// <summary>
        /// Initializes a new instance of the JsonReaderSettings class.
        /// </summary>
        public JsonReaderSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonReaderSettings class.
        /// </summary>
        /// <param name="closeInput">Whether to close the input stream when the reader is closed.</param>
        /// <param name="guidRepresentation">The representation for Guids.</param>
        [Obsolete("Use the no-argument constructor instead and set the properties.")]
        public JsonReaderSettings(bool closeInput, GuidRepresentation guidRepresentation)
            : base(guidRepresentation)
        {
            _closeInput = closeInput;
        }

        // public static properties
        /// <summary>
        /// Gets or sets the default settings for a JsonReader.
        /// </summary>
        public static JsonReaderSettings Defaults
        {
            get
            {
                if (__defaults == null)
                {
                    __defaults = new JsonReaderSettings();
                }
                return __defaults;
            }
            set { __defaults = value; }
        }

        // public properties
        /// <summary>
        /// Gets or sets whether to close the input stream when the reader is closed.
        /// </summary>
        public bool CloseInput
        {
            get { return _closeInput; }
            set
            {
                if (IsFrozen) { throw new InvalidOperationException("JsonReaderSettings is frozen."); }
                _closeInput = value;
            }
        }

        // public methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        public new JsonReaderSettings Clone()
        {
            return (JsonReaderSettings)CloneImplementation();
        }

        // protected methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        protected override BsonReaderSettings CloneImplementation()
        {
            var clone = new JsonReaderSettings
            {
                CloseInput = _closeInput,
                GuidRepresentation = GuidRepresentation
            };
            return clone;
        }
    }
}
