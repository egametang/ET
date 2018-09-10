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
using System.Collections.Generic;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents how duplicate names should be handled.
    /// </summary>
    public enum DuplicateNameHandling
    {
        /// <summary>
        /// Overwrite original value with new value.
        /// </summary>
        Overwrite,
        /// <summary>
        /// Ignore duplicate name and keep original value.
        /// </summary>
        Ignore,
        /// <summary>
        /// Throw an exception.
        /// </summary>
        ThrowException
    }

    /// <summary>
    /// Represents options used by the BsonTypeMapper.
    /// </summary>
    public class BsonTypeMapperOptions
    {
        // private static fields
        private static BsonTypeMapperOptions __defaults = new BsonTypeMapperOptions();

        // private fields
        private bool _isFrozen;
        private DuplicateNameHandling _duplicateNameHandling; // default value zero means Overwrite
        private Type _mapBsonArrayTo = typeof(List<object>);
        private Type _mapBsonDocumentTo = typeof(Dictionary<string, object>);
        private bool _mapOldBinaryToByteArray;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonTypeMapperOptions class.
        /// </summary>
        public BsonTypeMapperOptions()
        {
        }

        // public static properties
        /// <summary>
        /// Gets or sets the default BsonTypeMapperOptions.
        /// </summary>
        public static BsonTypeMapperOptions Defaults
        {
            get { return __defaults; }
            set {
                if (value.IsFrozen)
                {
                    __defaults = value;
                }
                else
                {
                    __defaults = value.Clone().Freeze();
                }
            }
        }

        // public properties
        /// <summary>
        /// Gets or sets how duplicate names should be handled.
        /// </summary>
        public DuplicateNameHandling DuplicateNameHandling
        {
            get { return _duplicateNameHandling; }
            set {
                if (_isFrozen) { throw new InvalidOperationException("BsonTypeMapperOptions is frozen."); }
                _duplicateNameHandling = value;
            }
        }

        /// <summary>
        /// Gets whether the BsonTypeMapperOptions is frozen.
        /// </summary>
        public bool IsFrozen
        {
            get { return _isFrozen; }
        }

        /// <summary>
        /// Gets or sets the type that a BsonArray should be mapped to.
        /// </summary>
        public Type MapBsonArrayTo
        {
            get { return _mapBsonArrayTo; }
            set {
                if (_isFrozen) { throw new InvalidOperationException("BsonTypeMapperOptions is frozen."); }
                _mapBsonArrayTo = value;
            }
        }

        /// <summary>
        /// Gets or sets the type that a BsonDocument should be mapped to.
        /// </summary>
        public Type MapBsonDocumentTo
        {
            get { return _mapBsonDocumentTo; }
            set {
                if (_isFrozen) { throw new InvalidOperationException("BsonTypeMapperOptions is frozen."); }
                _mapBsonDocumentTo = value;
            }
        }

        /// <summary>
        /// Gets or sets whether binary sub type OldBinary should be mapped to byte[] the way sub type Binary is.
        /// </summary>
        public bool MapOldBinaryToByteArray
        {
            get { return _mapOldBinaryToByteArray; }
            set {
                if (_isFrozen) { throw new InvalidOperationException("BsonTypeMapperOptions is frozen."); }
                _mapOldBinaryToByteArray = value;
            }
        }

        // public methods
        /// <summary>
        /// Clones the BsonTypeMapperOptions.
        /// </summary>
        /// <returns>The cloned BsonTypeMapperOptions.</returns>
        public BsonTypeMapperOptions Clone()
        {
            return new BsonTypeMapperOptions
            {
                DuplicateNameHandling = _duplicateNameHandling,
                MapBsonArrayTo = _mapBsonArrayTo,
                MapBsonDocumentTo = _mapBsonDocumentTo,
                MapOldBinaryToByteArray = _mapOldBinaryToByteArray
            };
        }

        /// <summary>
        /// Freezes the BsonTypeMapperOptions.
        /// </summary>
        /// <returns>The frozen BsonTypeMapperOptions.</returns>
        public BsonTypeMapperOptions Freeze()
        {
            if (!_isFrozen)
            {
                _isFrozen = true;
            }
            return this;
        }
    }
}
