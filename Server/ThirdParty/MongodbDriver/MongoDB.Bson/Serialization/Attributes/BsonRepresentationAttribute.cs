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
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Attributes
{
    /// <summary>
    /// Specifies the external representation and related options for this field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class BsonRepresentationAttribute : BsonSerializationOptionsAttribute
    {
        // private fields
        private BsonType _representation;
        private bool _allowOverflow;
        private bool _allowTruncation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonRepresentationAttribute class.
        /// </summary>
        /// <param name="representation">The external representation.</param>
        public BsonRepresentationAttribute(BsonType representation)
        {
            _representation = representation;
        }

        // public properties
        /// <summary>
        /// Gets the external representation.
        /// </summary>
        public BsonType Representation
        {
            get { return _representation; }
        }

        /// <summary>
        /// Gets or sets whether to allow overflow.
        /// </summary>
        public bool AllowOverflow
        {
            get { return _allowOverflow; }
            set { _allowOverflow = value; }
        }

        /// <summary>
        /// Gets or sets whether to allow truncation.
        /// </summary>
        public bool AllowTruncation
        {
            get { return _allowTruncation; }
            set { _allowTruncation = value; }
        }

        // protected methods
        /// <summary>
        /// Reconfigures the specified serializer by applying this attribute to it.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A reconfigured serializer.</returns>
        protected override IBsonSerializer Apply(IBsonSerializer serializer)
        {
            var representationConfigurable = serializer as IRepresentationConfigurable;
            if (representationConfigurable != null)
            {
                var reconfiguredSerializer = representationConfigurable.WithRepresentation(_representation);

                var converterConfigurable = reconfiguredSerializer as IRepresentationConverterConfigurable;
                if (converterConfigurable != null)
                {
                    var converter = new RepresentationConverter(_allowOverflow, _allowTruncation);
                    reconfiguredSerializer = converterConfigurable.WithConverter(converter);
                }

                return reconfiguredSerializer;
            }

            // for backward compatibility representations of Array and Document are mapped to DictionaryRepresentations if possible
            var dictionaryRepresentationConfigurable = serializer as IDictionaryRepresentationConfigurable;
            if (dictionaryRepresentationConfigurable != null)
            {
                if (_representation == BsonType.Array || _representation == BsonType.Document)
                {
                    var dictionaryRepresentation = (_representation == BsonType.Array) ? DictionaryRepresentation.ArrayOfArrays: DictionaryRepresentation.Document;
                    return dictionaryRepresentationConfigurable.WithDictionaryRepresentation(dictionaryRepresentation);
                }
            }

            return base.Apply(serializer);
        }
    }
}
