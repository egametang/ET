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
using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Represents the standard discriminator conventions (see ScalarDiscriminatorConvention and HierarchicalDiscriminatorConvention).
    /// </summary>
    public abstract class StandardDiscriminatorConvention : IDiscriminatorConvention
    {
        // private static fields
        private static ScalarDiscriminatorConvention __scalar = new ScalarDiscriminatorConvention("_t");
        private static HierarchicalDiscriminatorConvention __hierarchical = new HierarchicalDiscriminatorConvention("_t");

        // private fields
        private string _elementName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the StandardDiscriminatorConvention class.
        /// </summary>
        /// <param name="elementName">The element name.</param>
        protected StandardDiscriminatorConvention(string elementName)
        {
            if (elementName == null)
            {
                throw new ArgumentNullException("elementName");
            }
            if (elementName.IndexOf('\0') != -1)
            {
                throw new ArgumentException("Element names cannot contain nulls.", "elementName");
            }
            _elementName = elementName;
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the ScalarDiscriminatorConvention.
        /// </summary>
        public static ScalarDiscriminatorConvention Scalar
        {
            get { return __scalar; }
        }

        /// <summary>
        /// Gets an instance of the HierarchicalDiscriminatorConvention.
        /// </summary>
        public static HierarchicalDiscriminatorConvention Hierarchical
        {
            get { return __hierarchical; }
        }

        // public properties
        /// <summary>
        /// Gets the discriminator element name.
        /// </summary>
        public string ElementName
        {
            get { return _elementName; }
        }

        // public methods
        /// <summary>
        /// Gets the actual type of an object by reading the discriminator from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The reader.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <returns>The actual type.</returns>
        public Type GetActualType(BsonReader bsonReader, Type nominalType)
        {
            // the BsonReader is sitting at the value whose actual type needs to be found
            var bsonType = bsonReader.GetCurrentBsonType();
            if (bsonReader.State == BsonReaderState.Value)
            {
                Type primitiveType = null;
                switch (bsonType)
                {
                    case BsonType.Boolean: primitiveType = typeof(bool); break;
                    case BsonType.Binary:
                        var bookmark = bsonReader.GetBookmark();
                        var binaryData = bsonReader.ReadBinaryData();
                        var subType = binaryData.SubType;
                        if (subType == BsonBinarySubType.UuidStandard || subType == BsonBinarySubType.UuidLegacy)
                        {
                            primitiveType = typeof(Guid);
                        }
                        bsonReader.ReturnToBookmark(bookmark);
                        break;
                    case BsonType.DateTime: primitiveType = typeof(DateTime); break;
                    case BsonType.Double: primitiveType = typeof(double); break;
                    case BsonType.Int32: primitiveType = typeof(int); break;
                    case BsonType.Int64: primitiveType = typeof(long); break;
                    case BsonType.ObjectId: primitiveType = typeof(ObjectId); break;
                    case BsonType.String: primitiveType = typeof(string); break;
                }

                // Type.IsAssignableFrom is extremely expensive, always perform a direct type check before calling Type.IsAssignableFrom
                if (primitiveType != null && (primitiveType == nominalType || nominalType.IsAssignableFrom(primitiveType)))
                {
                    return primitiveType;
                }
            }

            if (bsonType == BsonType.Document)
            {
                // ensure KnownTypes of nominalType are registered (so IsTypeDiscriminated returns correct answer)
                BsonSerializer.EnsureKnownTypesAreRegistered(nominalType);

                // we can skip looking for a discriminator if nominalType has no discriminated sub types
                if (BsonSerializer.IsTypeDiscriminated(nominalType))
                {
                    var bookmark = bsonReader.GetBookmark();
                    bsonReader.ReadStartDocument();
                    var actualType = nominalType;
                    if (bsonReader.FindElement(_elementName))
                    {
                        var discriminator = (BsonValue)BsonValueSerializer.Instance.Deserialize(bsonReader, typeof(BsonValue), null);
                        if (discriminator.IsBsonArray)
                        {
                            discriminator = discriminator.AsBsonArray.Last(); // last item is leaf class discriminator
                        }
                        actualType = BsonSerializer.LookupActualType(nominalType, discriminator);
                    }
                    bsonReader.ReturnToBookmark(bookmark);
                    return actualType;
                }
            }

            return nominalType;
        }

        /// <summary>
        /// Gets the discriminator value for an actual type.
        /// </summary>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The discriminator value.</returns>
        public abstract BsonValue GetDiscriminator(Type nominalType, Type actualType);
    }
}
