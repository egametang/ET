/* Copyright 2010-2015 MongoDB Inc.
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
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// The settings used to access a database.
    /// </summary>
    public class MongoDatabaseSettings
    {
        // private fields
        private Setting<GuidRepresentation> _guidRepresentation;
        private Setting<ReadConcern> _readConcern;
        private Setting<UTF8Encoding> _readEncoding;
        private Setting<ReadPreference> _readPreference;
        private Setting<WriteConcern> _writeConcern;
        private Setting<UTF8Encoding> _writeEncoding;

        // the following fields are set when Freeze is called
        private bool _isFrozen;
        private int _frozenHashCode;
        private string _frozenStringRepresentation;

        // constructors
        /// <summary>
        /// Creates a new instance of MongoDatabaseSettings.
        /// </summary>
        public MongoDatabaseSettings()
        {
        }

        // public properties
        /// <summary>
        /// Gets or sets the representation to use for Guids.
        /// </summary>
        public GuidRepresentation GuidRepresentation
        {
            get { return _guidRepresentation.Value; }
            set
            {
                if (_isFrozen) { throw new InvalidOperationException("MongoDatabaseSettings is frozen."); }
                _guidRepresentation.Value = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the settings have been frozen to prevent further changes.
        /// </summary>
        public bool IsFrozen
        {
            get { return _isFrozen; }
        }

        /// <summary>
        /// Gets or sets the read concern.
        /// </summary>
        public ReadConcern ReadConcern
        {
            get { return _readConcern.Value; }
            set
            {
                if (_isFrozen) { throw new InvalidOperationException("MongoDatabaseSettings is frozen."); }
                _readConcern.Value = Ensure.IsNotNull(value, nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the Read Encoding.
        /// </summary>
        public UTF8Encoding ReadEncoding
        {
            get { return _readEncoding.Value; }
            set
            {
                if (_isFrozen) { throw new InvalidOperationException("MongoDatabaseSettings is frozen."); }
                _readEncoding.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the read preference.
        /// </summary>
        public ReadPreference ReadPreference
        {
            get { return _readPreference.Value; }
            set
            {
                if (_isFrozen) { throw new InvalidOperationException("MongoDatabaseSettings is frozen."); }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _readPreference.Value = value;
            }
        }

        /// <summary>
        /// Gets the serializer registry.
        /// </summary>
        public IBsonSerializerRegistry SerializerRegistry
        {
            get { return BsonSerializer.SerializerRegistry; }
        }

        /// <summary>
        /// Gets or sets the WriteConcern to use.
        /// </summary>
        public WriteConcern WriteConcern
        {
            get { return _writeConcern.Value; }
            set
            {
                if (_isFrozen) { throw new InvalidOperationException("MongoDatabaseSettings is frozen."); }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _writeConcern.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the Write Encoding.
        /// </summary>
        public UTF8Encoding WriteEncoding
        {
            get { return _writeEncoding.Value; }
            set
            {
                if (_isFrozen) { throw new InvalidOperationException("MongoDatabaseSettings is frozen."); }
                _writeEncoding.Value = value;
            }
        }

        // public methods
        /// <summary>
        /// Creates a clone of the settings.
        /// </summary>
        /// <returns>A clone of the settings.</returns>
        public MongoDatabaseSettings Clone()
        {
            var clone = new MongoDatabaseSettings();
            clone._guidRepresentation = _guidRepresentation.Clone();
            clone._readConcern = _readConcern.Clone();
            clone._readEncoding = _readEncoding.Clone();
            clone._readPreference = _readPreference.Clone();
            clone._writeConcern = _writeConcern.Clone();
            clone._writeEncoding = _writeEncoding.Clone();
            return clone;
        }

        /// <summary>
        /// Compares two MongoDatabaseSettings instances.
        /// </summary>
        /// <param name="obj">The other instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            var rhs = obj as MongoDatabaseSettings;
            if (rhs == null)
            {
                return false;
            }
            else
            {
                if (_isFrozen && rhs._isFrozen)
                {
                    return _frozenStringRepresentation == rhs._frozenStringRepresentation;
                }
                else
                {
                    return
                        _guidRepresentation.Value == rhs._guidRepresentation.Value &&
                        _readConcern.Value == rhs._readConcern.Value &&
                        object.Equals(_readEncoding, rhs._readEncoding) &&
                        object.Equals(_readPreference.Value, rhs._readPreference.Value) &&
                        _writeConcern.Value == rhs._writeConcern.Value &&
                        object.Equals(_writeEncoding, rhs._writeEncoding);
                }
            }
        }

        /// <summary>
        /// Freezes the settings.
        /// </summary>
        /// <returns>The frozen settings.</returns>
        public MongoDatabaseSettings Freeze()
        {
            if (!_isFrozen)
            {
                _frozenHashCode = GetHashCode();
                _frozenStringRepresentation = ToString();
                _isFrozen = true;
            }
            return this;
        }

        /// <summary>
        /// Returns a frozen copy of the settings.
        /// </summary>
        /// <returns>A frozen copy of the settings.</returns>
        public MongoDatabaseSettings FrozenCopy()
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

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            if (_isFrozen)
            {
                return _frozenHashCode;
            }

            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + _guidRepresentation.Value.GetHashCode();
            hash = 37 * hash + ((_readConcern.Value == null) ? 0 : _readConcern.GetHashCode());
            hash = 37 * hash + ((_readEncoding.Value == null) ? 0 : _readEncoding.GetHashCode());
            hash = 37 * hash + ((_readPreference.Value == null) ? 0 : _readPreference.Value.GetHashCode());
            hash = 37 * hash + ((_writeConcern.Value == null) ? 0 : _writeConcern.Value.GetHashCode());
            hash = 37 * hash + ((_writeEncoding.Value == null) ? 0 : _writeEncoding.GetHashCode());
            return hash;
        }

        /// <summary>
        /// Returns a string representation of the settings.
        /// </summary>
        /// <returns>A string representation of the settings.</returns>
        public override string ToString()
        {
            if (_isFrozen)
            {
                return _frozenStringRepresentation;
            }

            var parts = new List<string>();
            parts.Add(string.Format("GuidRepresentation={0}", _guidRepresentation.Value));
            parts.Add(string.Format("ReadConcern={0}", _readConcern.Value));
            if (_readEncoding.HasBeenSet)
            {
                parts.Add(string.Format("ReadEncoding={0}", (_readEncoding.Value == null) ? "null" : "UTF8Encoding"));
            }
            parts.Add(string.Format("ReadPreference={0}", _readPreference.Value));
            parts.Add(string.Format("WriteConcern={0}", _writeConcern.Value));
            if (_writeEncoding.HasBeenSet)
            {
                parts.Add(string.Format("WriteEncoding={0}", (_writeEncoding.Value == null) ? "null" : "UTF8Encoding"));
            }
            return string.Join(";", parts.ToArray());
        }

        // internal methods
        internal void ApplyDefaultValues(IInheritableMongoClientSettings clientSettings)
        {
            if (!_guidRepresentation.HasBeenSet)
            {
                GuidRepresentation = clientSettings.GuidRepresentation;
            }
            if (!_readConcern.HasBeenSet)
            {
                ReadConcern = clientSettings.ReadConcern;
            }
            if (!_readEncoding.HasBeenSet)
            {
                ReadEncoding = clientSettings.ReadEncoding;
            }
            if (!_readPreference.HasBeenSet)
            {
                ReadPreference = clientSettings.ReadPreference;
            }
            if (!_writeConcern.HasBeenSet)
            {
                WriteConcern = clientSettings.WriteConcern;
            }
            if (!_writeEncoding.HasBeenSet)
            {
                WriteEncoding = clientSettings.WriteEncoding;
            }
        }
    }
}
