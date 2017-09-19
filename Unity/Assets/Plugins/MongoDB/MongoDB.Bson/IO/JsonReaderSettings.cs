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
    /// Represents settings for a JsonReader.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class JsonReaderSettings : BsonReaderSettings
    {
        // private static fields
        private static JsonReaderSettings __defaults = null; // delay creation to pick up the latest default values

        // constructors
        /// <summary>
        /// Initializes a new instance of the JsonReaderSettings class.
        /// </summary>
        public JsonReaderSettings()
        {
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
                GuidRepresentation = GuidRepresentation
            };
            return clone;
        }
    }
}
