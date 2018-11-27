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
using System.Linq;
using System.Reflection;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Represents the object discriminator convention.
    /// </summary>
    public class ObjectDiscriminatorConvention : IDiscriminatorConvention
    {
        // private static fields
        private static ObjectDiscriminatorConvention __instance = new ObjectDiscriminatorConvention("_t");

        // private fields
        private string _elementName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the ObjectDiscriminatorConvention class.
        /// </summary>
        /// <param name="elementName">The element name.</param>
        public ObjectDiscriminatorConvention(string elementName)
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
        /// Gets an instance of the ObjectDiscriminatorConvention.
        /// </summary>
        public static ObjectDiscriminatorConvention Instance
        {
            get { return __instance; }
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
        public Type GetActualType(IBsonReader bsonReader, Type nominalType)
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
                    case BsonType.Decimal128: primitiveType = typeof(Decimal128); break;
                    case BsonType.Double: primitiveType = typeof(double); break;
                    case BsonType.Int32: primitiveType = typeof(int); break;
                    case BsonType.Int64: primitiveType = typeof(long); break;
                    case BsonType.ObjectId: primitiveType = typeof(ObjectId); break;
                    case BsonType.String: primitiveType = typeof(string); break;
                }

                // Type.IsAssignableFrom is extremely expensive, always perform a direct type check before calling Type.IsAssignableFrom
                if (primitiveType != null && (primitiveType == nominalType || nominalType.GetTypeInfo().IsAssignableFrom(primitiveType)))
                {
                    return primitiveType;
                }
            }

            if (bsonType == BsonType.Document)
            {
                var bookmark = bsonReader.GetBookmark();
                bsonReader.ReadStartDocument();
                var actualType = nominalType;
                if (bsonReader.FindElement(_elementName))
                {
                    var context = BsonDeserializationContext.CreateRoot(bsonReader);
                    var discriminator = BsonValueSerializer.Instance.Deserialize(context);
                    if (discriminator.IsBsonArray)
                    {
                        discriminator = discriminator.AsBsonArray.Last(); // last item is leaf class discriminator
                    }
                    actualType = BsonSerializer.LookupActualType(nominalType, discriminator);
                }
                bsonReader.ReturnToBookmark(bookmark);
                return actualType;
            }

            return nominalType;
        }

        /// <summary>
        /// Gets the discriminator value for an actual type.
        /// </summary>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The discriminator value.</returns>
        public BsonValue GetDiscriminator(Type nominalType, Type actualType)
        {
            return TypeNameDiscriminator.GetDiscriminator(actualType);
        }
    }
}
