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
using System.Text;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents settings for a BsonBinaryReader.
    /// </summary>
    [Serializable]
    public class BsonBinaryReaderSettings : BsonReaderSettings
    {
        // private static fields
        private static BsonBinaryReaderSettings __defaults = null; // delay creation to pick up the latest default values

        // private fields
        private bool _closeInput = false;
        private UTF8Encoding _encoding = new UTF8Encoding(false, true);
        private bool _fixOldBinarySubTypeOnInput = true;
        private bool _fixOldDateTimeMaxValueOnInput = true;
        private int _maxDocumentSize = BsonDefaults.MaxDocumentSize;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBinaryReaderSettings class.
        /// </summary>
        public BsonBinaryReaderSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonBinaryReaderSettings class.
        /// </summary>
        /// <param name="closeInput">Whether to close the input stream when the reader is closed.</param>
        /// <param name="fixOldBinarySubTypeOnInput">Whether to fix occurrences of the old binary subtype on input.</param>
        /// <param name="fixOldDateTimeMaxValueOnInput">Whether to fix occurrences of the old representation of DateTime.MaxValue on input.</param>
        /// <param name="guidRepresentation">The representation for Guids.</param>
        /// <param name="maxDocumentSize">The max document size.</param>
        [Obsolete("Use the no-argument constructor instead and set the properties.")]
        public BsonBinaryReaderSettings(
            bool closeInput,
            bool fixOldBinarySubTypeOnInput,
            bool fixOldDateTimeMaxValueOnInput,
            GuidRepresentation guidRepresentation,
            int maxDocumentSize)
            : base(guidRepresentation)
        {
            _closeInput = closeInput;
            _fixOldBinarySubTypeOnInput = fixOldBinarySubTypeOnInput;
            _fixOldDateTimeMaxValueOnInput = fixOldDateTimeMaxValueOnInput;
            _maxDocumentSize = maxDocumentSize;
        }

        // public static properties
        /// <summary>
        /// Gets or sets the default settings for a BsonBinaryReader.
        /// </summary>
        public static BsonBinaryReaderSettings Defaults
        {
            get
            {
                if (__defaults == null)
                {
                    __defaults = new BsonBinaryReaderSettings();
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
                if (IsFrozen) { throw new InvalidOperationException("BsonBinaryReaderSettings is frozen."); }
                _closeInput = value;
            }
        }

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
                if (IsFrozen) { throw new InvalidOperationException("BsonBinaryReaderSettings is frozen."); }
                _encoding = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to fix occurrences of the old binary subtype on input. 
        /// </summary>
        public bool FixOldBinarySubTypeOnInput
        {
            get { return _fixOldBinarySubTypeOnInput; }
            set
            {
                if (IsFrozen) { throw new InvalidOperationException("BsonBinaryReaderSettings is frozen."); }
                _fixOldBinarySubTypeOnInput = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to fix occurrences of the old representation of DateTime.MaxValue on input. 
        /// </summary>
        public bool FixOldDateTimeMaxValueOnInput
        {
            get { return _fixOldDateTimeMaxValueOnInput; }
            set
            {
                if (IsFrozen) { throw new InvalidOperationException("BsonBinaryReaderSettings is frozen."); }
                _fixOldDateTimeMaxValueOnInput = value;
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
                if (IsFrozen) { throw new InvalidOperationException("BsonBinaryReaderSettings is frozen."); }
                _maxDocumentSize = value;
            }
        }

        // public methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        public new BsonBinaryReaderSettings Clone()
        {
            return (BsonBinaryReaderSettings)CloneImplementation();
        }

        // protected methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        protected override BsonReaderSettings CloneImplementation()
        {
            var clone = new BsonBinaryReaderSettings
            {
                CloseInput = _closeInput,
                Encoding = _encoding,
                FixOldBinarySubTypeOnInput = _fixOldBinarySubTypeOnInput,
                FixOldDateTimeMaxValueOnInput = _fixOldDateTimeMaxValueOnInput,
                GuidRepresentation = GuidRepresentation,
                MaxDocumentSize = _maxDocumentSize
            };
            return clone;
        }
    }
}
