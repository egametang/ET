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
using System.Text;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents settings for a BsonBinaryWriter.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class BsonBinaryWriterSettings : BsonWriterSettings
    {
        // private static fields
        private static BsonBinaryWriterSettings __defaults = null; // delay creation to pick up the latest default values

        // private fields
        private UTF8Encoding _encoding = Utf8Encodings.Strict;
        private bool _fixOldBinarySubTypeOnOutput = true;
        private int _maxDocumentSize = BsonDefaults.MaxDocumentSize;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBinaryWriterSettings class.
        /// </summary>
        public BsonBinaryWriterSettings()
        {
        }

        // public static properties
        /// <summary>
        /// Gets or sets the default BsonBinaryWriter settings.
        /// </summary>
        public static BsonBinaryWriterSettings Defaults
        {
            get
            {
                if (__defaults == null)
                {
                    __defaults = new BsonBinaryWriterSettings();
                }
                return __defaults;
            }
            set { __defaults = value; }
        }

        // public properties
        /// <summary>
        /// Gets or sets the Encoding.
        /// </summary>
        public UTF8Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (IsFrozen) { throw new InvalidOperationException("BsonBinaryWriterSettings is frozen."); }
                _encoding = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to fix the old binary data subtype on output.
        /// </summary>
        public bool FixOldBinarySubTypeOnOutput
        {
            get { return _fixOldBinarySubTypeOnOutput; }
            set
            {
                if (IsFrozen) { throw new InvalidOperationException("BsonBinaryWriterSettings is frozen."); }
                _fixOldBinarySubTypeOnOutput = value;
            }
        }

        /// <summary>
        /// Gets or sets the max document size.
        /// </summary>
        public int MaxDocumentSize
        {
            get { return _maxDocumentSize; }
            set
            {
                if (IsFrozen) { throw new InvalidOperationException("BsonBinaryWriterSettings is frozen."); }
                _maxDocumentSize = value;
            }
        }

        // public methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        public new BsonBinaryWriterSettings Clone()
        {
            return (BsonBinaryWriterSettings)CloneImplementation();
        }

        // protected methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        protected override BsonWriterSettings CloneImplementation()
        {
            var clone = new BsonBinaryWriterSettings
            {
                Encoding = _encoding,
                FixOldBinarySubTypeOnOutput = _fixOldBinarySubTypeOnOutput,
                GuidRepresentation = GuidRepresentation,
                MaxDocumentSize = _maxDocumentSize,
                MaxSerializationDepth = MaxSerializationDepth
            };
            return clone;
        }
    }
}
