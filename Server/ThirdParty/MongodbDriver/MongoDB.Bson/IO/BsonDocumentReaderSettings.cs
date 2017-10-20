/* Copyright 2010-2016 MongoDB Inc.
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
    /// Represents settings for a BsonDocumentReader.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class BsonDocumentReaderSettings : BsonReaderSettings
    {
        // private static fields
        private static BsonDocumentReaderSettings __defaults = null; // delay creation to pick up the latest default values

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocumentReaderSettings class.
        /// </summary>
        public BsonDocumentReaderSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocumentReaderSettings class.
        /// </summary>
        /// <param name="guidRepresentation">The representation for Guids.</param>
        [Obsolete("Use the no-argument constructor instead and set the properties.")]
        public BsonDocumentReaderSettings(GuidRepresentation guidRepresentation)
            : base(guidRepresentation)
        {
        }

        // public static properties
        /// <summary>
        /// Gets or sets the default settings for a BsonDocumentReader.
        /// </summary>
        public static BsonDocumentReaderSettings Defaults
        {
            get
            {
                if (__defaults == null)
                {
                    __defaults = new BsonDocumentReaderSettings();
                }
                return __defaults;
            }
            set { __defaults = value; }
        }

        // public methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        public new BsonDocumentReaderSettings Clone()
        {
            return (BsonDocumentReaderSettings)CloneImplementation();
        }

        // protected methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        protected override BsonReaderSettings CloneImplementation()
        {
            var clone = new BsonDocumentReaderSettings
            {
                GuidRepresentation = GuidRepresentation
            };
            return clone;
        }
    }
}
