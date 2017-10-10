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

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents args common to all serializers.
    /// </summary>
    public struct BsonSerializationArgs
    {
        // private fields
        private Type _nominalType;
        private bool _serializeAsNominalType;
        private bool _serializeIdFirst;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonSerializationArgs"/> struct.
        /// </summary>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="serializeAsNominalType">Whether to serialize as the nominal type.</param>
        /// <param name="serializeIdFirst">Whether to serialize the id first.</param>
        public BsonSerializationArgs(
            Type nominalType,
            bool serializeAsNominalType,
            bool serializeIdFirst)
        {
            _nominalType = nominalType;
            _serializeAsNominalType = serializeAsNominalType;
            _serializeIdFirst = serializeIdFirst;
        }

        // public properties
        /// <summary>
        /// Gets or sets the nominal type.
        /// </summary>
        /// <value>
        /// The nominal type.
        /// </value>
        public Type NominalType
        {
            get { return _nominalType; }
            set { _nominalType = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to serialize the value as if it were an instance of the nominal type.
        /// </summary>
        public bool SerializeAsNominalType
        {
            get { return _serializeAsNominalType; }
            set { _serializeAsNominalType = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to serialize the id first.
        /// </summary>
        public bool SerializeIdFirst
        {
            get { return _serializeIdFirst; }
            set { _serializeIdFirst = value; }
        }
    }
}