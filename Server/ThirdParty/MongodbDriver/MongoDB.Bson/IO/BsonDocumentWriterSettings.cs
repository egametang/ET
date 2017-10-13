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
    /// Represents settings for a BsonDocumentWriter.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class BsonDocumentWriterSettings : BsonWriterSettings
    {
        // private static fields
        private static BsonDocumentWriterSettings __defaults = null; // delay creation to pick up the latest default values

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocumentWriterSettings class.
        /// </summary>
        public BsonDocumentWriterSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocumentWriterSettings class.
        /// </summary>
        /// <param name="guidRepresentation">The representation for Guids.</param>
        [Obsolete("Use the no-argument constructor instead and set the properties.")]
        public BsonDocumentWriterSettings(GuidRepresentation guidRepresentation)
            : base(guidRepresentation)
        {
        }

        // public static properties
        /// <summary>
        /// Gets or sets the default BsonDocumentWriter settings.
        /// </summary>
        public static BsonDocumentWriterSettings Defaults
        {
            get
            {
                if (__defaults == null)
                {
                    __defaults = new BsonDocumentWriterSettings();
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
        public new BsonDocumentWriterSettings Clone()
        {
            return (BsonDocumentWriterSettings)CloneImplementation();
        }

        // protected methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        protected override BsonWriterSettings CloneImplementation()
        {
            var clone = new BsonDocumentWriterSettings
            {
                GuidRepresentation = GuidRepresentation,
                MaxSerializationDepth = MaxSerializationDepth
            };
            return clone;
        }
    }
}
