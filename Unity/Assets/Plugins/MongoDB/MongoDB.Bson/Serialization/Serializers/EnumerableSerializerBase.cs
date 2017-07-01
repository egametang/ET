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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a base serializer for enumerable values.
    /// </summary>
    public abstract class EnumerableSerializerBase : BsonBaseSerializer, IBsonArraySerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the EnumerableSerializerBase class.
        /// </summary>
        public EnumerableSerializerBase(IBsonSerializationOptions defaultSerializationOptions)
            : base(defaultSerializationOptions)
        {
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            var arraySerializationOptions = EnsureSerializationOptions<ArraySerializationOptions>(options);
            var itemSerializationOptions = arraySerializationOptions.ItemSerializationOptions;

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;

                case BsonType.Array:
                    var instance = CreateInstance(actualType);
                    var itemDiscriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(object));
                    Type lastItemType = null;
                    IBsonSerializer lastItemSerializer = null;

                    bsonReader.ReadStartArray();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var itemType = itemDiscriminatorConvention.GetActualType(bsonReader, typeof(object));
                        IBsonSerializer itemSerializer;
                        if (itemType == lastItemType)
                        {
                            itemSerializer = lastItemSerializer;
                        }
                        else
                        {
                            itemSerializer = BsonSerializer.LookupSerializer(itemType);
                            lastItemType = itemType;
                            lastItemSerializer = itemSerializer;
                        }
                        var item = itemSerializer.Deserialize(bsonReader, typeof(object), itemType, itemSerializationOptions);
                        AddItem(instance, item);
                    }
                    bsonReader.ReadEndArray();

                    return FinalizeResult(instance, actualType);

                case BsonType.Document:
                    bsonReader.ReadStartDocument();
                    bsonReader.ReadString("_t"); // skip over discriminator
                    bsonReader.ReadName("_v");
                    var value = Deserialize(bsonReader, actualType, actualType, options);
                    bsonReader.ReadEndDocument();
                    return value;

                default:
                    var message = string.Format("Can't deserialize a {0} from BsonType {1}.", nominalType.FullName, bsonType);
                    throw new Exception(message);
            }
        }

        /// <summary>
        /// Gets the serialization info for individual items of an enumerable type.
        /// </summary>
        /// <returns>The serialization info for the items.</returns>
        public BsonSerializationInfo GetItemSerializationInfo()
        {
            string elementName = null;
            var serializer = BsonSerializer.LookupSerializer(typeof(object));
            var nominalType = typeof(object);
            IBsonSerializationOptions serializationOptions = null;
            return new BsonSerializationInfo(elementName, serializer, nominalType, serializationOptions);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                var discriminator = GetDiscriminator(nominalType, actualType);
                if (discriminator != null)
                {
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteString("_t", discriminator);
                    bsonWriter.WriteName("_v");
                    Serialize(bsonWriter, actualType, value, options);
                    bsonWriter.WriteEndDocument();
                    return;
                }

                var arraySerializationOptions = EnsureSerializationOptions<ArraySerializationOptions>(options);
                var itemSerializationOptions = arraySerializationOptions.ItemSerializationOptions;
                Type lastItemType = null;
                IBsonSerializer lastItemSerializer = null;

                bsonWriter.WriteStartArray();
                foreach (var item in EnumerateItemsInSerializationOrder(value))
                {
                    var itemType = (item == null) ? typeof(object) : item.GetType();
                    IBsonSerializer itemSerializer;
                    if (itemType == lastItemType)
                    {
                        itemSerializer = lastItemSerializer;
                    }
                    else
                    {
                        itemSerializer = BsonSerializer.LookupSerializer(itemType);
                        lastItemType = itemType;
                        lastItemSerializer = itemSerializer;
                    }
                    itemSerializer.Serialize(bsonWriter, typeof(object), item, itemSerializationOptions);
                }
                bsonWriter.WriteEndArray();
            }
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="item">The item.</param>
        protected abstract void AddItem(object instance, object item);

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The instance.</returns>
        protected abstract object CreateInstance(Type actualType);

        /// <summary>
        /// Enumerates the items.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The items.</returns>
        protected abstract IEnumerable EnumerateItemsInSerializationOrder(object instance);

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The result.</returns>
        protected abstract object FinalizeResult(object instance, Type actualType);

        /// <summary>
        /// Gets the discriminator.
        /// </summary>
        /// <param name="nominalType">Type nominal type.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The discriminator (or null if no discriminator is needed).</returns>
        protected virtual string GetDiscriminator(Type nominalType, Type actualType)
        {
            if (nominalType == typeof(object))
            {
                return TypeNameDiscriminator.GetDiscriminator(actualType);
            }

            return null;
        }
    }

    /// <summary>
    /// Represents a serializer for enumerable values.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public abstract class EnumerableSerializerBase<T> : BsonBaseSerializer, IBsonArraySerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the EnumerableSerializer class.
        /// </summary>
        public EnumerableSerializerBase(IBsonSerializationOptions defaultSerializationOptions)
            : base(defaultSerializationOptions)
        {
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType);

            var arraySerializationOptions = EnsureSerializationOptions<ArraySerializationOptions>(options);
            var itemSerializationOptions = arraySerializationOptions.ItemSerializationOptions;

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;

                case BsonType.Array:
                    var instance = CreateInstance(actualType);
                    var itemNominalType = typeof(T);
                    var itemNominalTypeSerializer = BsonSerializer.LookupSerializer(itemNominalType);

                    bsonReader.ReadStartArray();
                    if (itemNominalType.IsValueType)
                    {
                        while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                        {
                            var item = (T)itemNominalTypeSerializer.Deserialize(bsonReader, itemNominalType, itemNominalType, itemSerializationOptions);
                            AddItem(instance, item);
                        }
                    }
                    else
                    {
                        var discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(T));
                        Type lastItemType = null;
                        IBsonSerializer lastItemSerializer = null;

                        while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                        {
                            var itemType = discriminatorConvention.GetActualType(bsonReader, typeof(T));
                            IBsonSerializer itemSerializer;
                            if (itemType == itemNominalType)
                            {
                                itemSerializer = itemNominalTypeSerializer;
                            }
                            else if (itemType == lastItemType)
                            {
                                itemSerializer = lastItemSerializer;
                            }
                            else
                            {
                                itemSerializer = BsonSerializer.LookupSerializer(itemType);
                                lastItemType = itemType;
                                lastItemSerializer = itemSerializer;
                            }
                            var item = (T)itemSerializer.Deserialize(bsonReader, itemNominalType, itemType, itemSerializationOptions);
                            AddItem(instance, item);
                        }
                    }
                    bsonReader.ReadEndArray();

                    return FinalizeResult(instance, actualType);

                case BsonType.Document:
                    bsonReader.ReadStartDocument();
                    bsonReader.ReadString("_t"); // skip over discriminator
                    bsonReader.ReadName("_v");
                    var value = Deserialize(bsonReader, actualType, actualType, options);
                    bsonReader.ReadEndDocument();
                    return value;

                default:
                    var message = string.Format("Can't deserialize a {0} from BsonType {1}.", actualType.FullName, bsonType);
                    throw new Exception(message);
            }
        }

        /// <summary>
        /// Gets the serialization info for individual items of an enumerable type.
        /// </summary>
        /// <returns>The serialization info for the items.</returns>
        public BsonSerializationInfo GetItemSerializationInfo()
        {
            string elementName = null;
            var serializer = BsonSerializer.LookupSerializer(typeof(T));
            var nominalType = typeof(T);
            IBsonSerializationOptions serializationOptions = null;
            return new BsonSerializationInfo(elementName, serializer, nominalType, serializationOptions);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                var discriminator = GetDiscriminator(nominalType, actualType);
                if (discriminator != null)
                {
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteString("_t", discriminator);
                    bsonWriter.WriteName("_v");
                    Serialize(bsonWriter, actualType, value, options);
                    bsonWriter.WriteEndDocument();
                    return;
                }

                var arraySerializationOptions = EnsureSerializationOptions<ArraySerializationOptions>(options);
                var itemSerializationOptions = arraySerializationOptions.ItemSerializationOptions;
                var itemNominalType = typeof(T);
                var itemNominalTypeSerializer = BsonSerializer.LookupSerializer(itemNominalType);

                bsonWriter.WriteStartArray();
                if (itemNominalType.IsValueType)
                {
                    foreach (var item in EnumerateItemsInSerializationOrder(value))
                    {
                        itemNominalTypeSerializer.Serialize(bsonWriter, itemNominalType, item, itemSerializationOptions);
                    }
                }
                else
                {
                    Type lastItemType = null;
                    IBsonSerializer lastItemSerializer = null;

                    foreach (var item in EnumerateItemsInSerializationOrder(value))
                    {
                        var itemType = (item == null) ? itemNominalType : item.GetType();
                        IBsonSerializer itemSerializer;
                        if (itemType == itemNominalType)
                        {
                            itemSerializer = itemNominalTypeSerializer;
                        }
                        else if (itemType == lastItemType)
                        {
                            itemSerializer = lastItemSerializer;
                        }
                        else
                        {
                            itemSerializer = BsonSerializer.LookupSerializer(itemType);
                            lastItemType = itemType;
                            lastItemSerializer = itemSerializer;
                        }
                        itemSerializer.Serialize(bsonWriter, itemNominalType, item, itemSerializationOptions);
                    }
                }
                bsonWriter.WriteEndArray();
            }
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="item">The item.</param>
        protected abstract void AddItem(object instance, T item);

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The instance.</returns>
        protected abstract object CreateInstance(Type actualType);

        /// <summary>
        /// Enumerates the items.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The items.</returns>
        protected abstract IEnumerable<T> EnumerateItemsInSerializationOrder(object instance);

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The result.</returns>
        protected abstract object FinalizeResult(object instance, Type actualType);

        /// <summary>
        /// Gets the discriminator.
        /// </summary>
        /// <param name="nominalType">Type nominal type.</param>
        /// <param name="actualType">The actual type.</param>
        /// <returns>The discriminator (or null if no discriminator is needed).</returns>
        protected virtual string GetDiscriminator(Type nominalType, Type actualType)
        {
            if (nominalType == typeof(object))
            {
                return TypeNameDiscriminator.GetDiscriminator(actualType);
            }

            return null;
        }

        /// <summary>
        /// Verifies the types.
        /// </summary>
        /// <param name="nominalType">Type nominal type.</param>
        /// <param name="actualType">The actual type.</param>
        protected virtual void VerifyTypes(Type nominalType, Type actualType)
        {
        }
    }
}

