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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents a serializer for a class map.
    /// </summary>
    public class BsonClassMapSerializer : IBsonSerializer, IBsonIdProvider, IBsonDocumentSerializer
    {
        // private fields
        private BsonClassMap _classMap;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonClassMapSerializer class.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public BsonClassMapSerializer(BsonClassMap classMap)
        {
            _classMap = classMap;
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            VerifyNominalType(nominalType);
            if (bsonReader.GetCurrentBsonType() == Bson.BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }
            else
            {
                var discriminatorConvention = _classMap.GetDiscriminatorConvention();
                var actualType = discriminatorConvention.GetActualType(bsonReader, nominalType);
                if (actualType != nominalType)
                {
                    var serializer = BsonSerializer.LookupSerializer(actualType);
                    if (serializer != this)
                    {
                        return serializer.Deserialize(bsonReader, nominalType, actualType, options);
                    }
                }

                return Deserialize(bsonReader, nominalType, actualType, options);
            }
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            VerifyNominalType(nominalType);
            var bsonType = bsonReader.GetCurrentBsonType();
            if (bsonType == Bson.BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }
            else
            {
                if (actualType != _classMap.ClassType)
                {
                    var message = string.Format("BsonClassMapSerializer.Deserialize for type {0} was called with actualType {1}.",
                        BsonUtils.GetFriendlyTypeName(_classMap.ClassType), BsonUtils.GetFriendlyTypeName(actualType));
                    throw new BsonSerializationException(message);
                }

                if (actualType.IsValueType)
                {
                    var message = string.Format("Value class {0} cannot be deserialized.", actualType.FullName);
                    throw new BsonSerializationException(message);
                }

                if (_classMap.IsAnonymous)
                {
                    throw new InvalidOperationException("An anonymous class cannot be deserialized.");
                }

                if (bsonType != BsonType.Document)
                {
                    var message = string.Format(
                        "Expected a nested document representing the serialized form of a {0} value, but found a value of type {1} instead.",
                        actualType.FullName, bsonType);
                    throw new Exception(message);
                }

                Dictionary<string, object> values = null;
                object obj = null;
                ISupportInitialize supportsInitialization = null;
                if (_classMap.HasCreatorMaps)
                {
                    // for creator-based deserialization we first gather the values in a dictionary and then call a matching creator
                    values = new Dictionary<string, object>();
                }
                else
                {
                    // for mutable classes we deserialize the values directly into the result object
                    obj = _classMap.CreateInstance();

                    supportsInitialization = obj as ISupportInitialize;
                    if (supportsInitialization != null)
                    {
                        supportsInitialization.BeginInit();
                    }
                }

                var discriminatorConvention = _classMap.GetDiscriminatorConvention();
                var allMemberMaps = _classMap.AllMemberMaps;
                var extraElementsMemberMapIndex = _classMap.ExtraElementsMemberMapIndex;
                var memberMapBitArray = FastMemberMapHelper.GetBitArray(allMemberMaps.Count);

                bsonReader.ReadStartDocument();
                var elementTrie = _classMap.ElementTrie;
                bool memberMapFound;
                int memberMapIndex;
                while (bsonReader.ReadBsonType(elementTrie, out memberMapFound, out memberMapIndex) != BsonType.EndOfDocument)
                {
                    var elementName = bsonReader.ReadName();
                    if (memberMapFound)
                    {
                        var memberMap = allMemberMaps[memberMapIndex];
                        if (memberMapIndex != extraElementsMemberMapIndex)
                        {
                            if (obj != null)
                            {
                                if (memberMap.IsReadOnly)
                                {
                                    bsonReader.SkipValue();
                                }
                                else
                                {
                                    var value = DeserializeMemberValue(bsonReader, memberMap);
                                    memberMap.Setter(obj, value);
                                }
                            }
                            else
                            {
                                var value = DeserializeMemberValue(bsonReader, memberMap);
                                values[elementName] = value;
                            }
                        }
                        else
                        {
                            DeserializeExtraElement(bsonReader, obj, elementName, memberMap);
                        }
                        memberMapBitArray[memberMapIndex >> 5] |= 1U << (memberMapIndex & 31);
                    }
                    else
                    {
                        if (elementName == discriminatorConvention.ElementName)
                        {
                            bsonReader.SkipValue(); // skip over discriminator
                            continue;
                        }

                        if (extraElementsMemberMapIndex >= 0)
                        {
                            DeserializeExtraElement(bsonReader, obj, elementName, _classMap.ExtraElementsMemberMap);
                            memberMapBitArray[extraElementsMemberMapIndex >> 5] |= 1U << (extraElementsMemberMapIndex & 31);
                        }
                        else if (_classMap.IgnoreExtraElements)
                        {
                            bsonReader.SkipValue();
                        }
                        else
                        {
                            var message = string.Format(
                                "Element '{0}' does not match any field or property of class {1}.",
                                elementName, _classMap.ClassType.FullName);
                            throw new Exception(message);
                        }
                    }
                }
                bsonReader.ReadEndDocument();

                // check any members left over that we didn't have elements for (in blocks of 32 elements at a time)
                for (var bitArrayIndex = 0; bitArrayIndex < memberMapBitArray.Length; ++bitArrayIndex)
                {
                    memberMapIndex = bitArrayIndex << 5;
                    var memberMapBlock = ~memberMapBitArray[bitArrayIndex]; // notice that bits are flipped so 1's are now the missing elements

                    // work through this memberMapBlock of 32 elements
                    while (true)
                    {
                        // examine missing elements (memberMapBlock is shifted right as we work through the block)
                        for (; (memberMapBlock & 1) != 0; ++memberMapIndex, memberMapBlock >>= 1)
                        {
                            var memberMap = allMemberMaps[memberMapIndex];
                            if (memberMap.IsReadOnly)
                            {
                                continue;
                            }

                            if (memberMap.IsRequired)
                            {
                                var fieldOrProperty = (memberMap.MemberInfo.MemberType == MemberTypes.Field) ? "field" : "property";
                                var message = string.Format(
                                    "Required element '{0}' for {1} '{2}' of class {3} is missing.",
                                    memberMap.ElementName, fieldOrProperty, memberMap.MemberName, _classMap.ClassType.FullName);
                                throw new Exception(message);
                            }

                            if (obj != null)
                            {
                                memberMap.ApplyDefaultValue(obj);
                            }
                            else if (memberMap.IsDefaultValueSpecified && !memberMap.IsReadOnly)
                            {
                                values[memberMap.ElementName] = memberMap.DefaultValue;
                            }
                        }

                        if (memberMapBlock == 0)
                        {
                            break;
                        }

                        // skip ahead to the next missing element
                        var leastSignificantBit = FastMemberMapHelper.GetLeastSignificantBit(memberMapBlock);
                        memberMapIndex += leastSignificantBit;
                        memberMapBlock >>= leastSignificantBit;
                    }
                }

                if (obj != null)
                {
                    if (supportsInitialization != null)
                    {
                        supportsInitialization.EndInit();
                    }

                    return obj;
                }
                else
                {
                    return CreateInstanceUsingCreator(values);
                }

            }
        }

        /// <summary>
        /// Get the default serialization options for this serializer.
        /// </summary>
        /// <returns>The default serialization options for this serializer.</returns>
        public IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            return null;
        }

        /// <summary>
        /// Gets the document Id.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="id">The Id.</param>
        /// <param name="idNominalType">The nominal type of the Id.</param>
        /// <param name="idGenerator">The IdGenerator for the Id type.</param>
        /// <returns>True if the document has an Id.</returns>
        public bool GetDocumentId(
            object document,
            out object id,
            out Type idNominalType,
            out IIdGenerator idGenerator)
        {
            var idMemberMap = _classMap.IdMemberMap;
            if (idMemberMap != null)
            {
                id = idMemberMap.Getter(document);
                idNominalType = idMemberMap.MemberType;
                idGenerator = idMemberMap.IdGenerator;
                return true;
            }
            else
            {
                id = null;
                idNominalType = null;
                idGenerator = null;
                return false;
            }
        }

        /// <summary>
        /// Gets the serialization info for a member.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <returns>The serialization info for the member.</returns>
        public BsonSerializationInfo GetMemberSerializationInfo(string memberName)
        {
            foreach (var memberMap in _classMap.AllMemberMaps)
            {
                if (memberMap.MemberName == memberName)
                {
                    var elementName = memberMap.ElementName;
                    var serializer = memberMap.GetSerializer(memberMap.MemberType);
                    var nominalType = memberMap.MemberType;
                    var serializationOptions = memberMap.SerializationOptions;
                    return new BsonSerializationInfo(elementName, serializer, nominalType, serializationOptions);
                }
            }

            var message = string.Format(
                "Class {0} does not have a member called {1}.",
                BsonUtils.GetFriendlyTypeName(_classMap.ClassType),
                memberName);
            throw new ArgumentOutOfRangeException("memberName", message);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public void Serialize(
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
                // Nullable types are weird because they get boxed as their underlying value type
                // we can best handle that by switching the nominalType to the underlying value type
                // (so VerifyNominalType doesn't fail and we don't get an unnecessary discriminator)
                if (nominalType.IsGenericType && nominalType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    nominalType = nominalType.GetGenericArguments()[0];
                }

                VerifyNominalType(nominalType);
                var actualType = (value == null) ? nominalType : value.GetType();
                if (actualType != _classMap.ClassType)
                {
                    var message = string.Format("BsonClassMapSerializer.Serialize for type {0} was called with actualType {1}.",
                        BsonUtils.GetFriendlyTypeName(_classMap.ClassType), BsonUtils.GetFriendlyTypeName(actualType));
                    throw new BsonSerializationException(message);
                }

                var documentSerializationOptions = (options ?? DocumentSerializationOptions.Defaults) as DocumentSerializationOptions;
                if (documentSerializationOptions == null)
                {
                    var message = string.Format(
                        "Serializer BsonClassMapSerializer expected serialization options of type {0}, not {1}.",
                        BsonUtils.GetFriendlyTypeName(typeof(DocumentSerializationOptions)),
                        BsonUtils.GetFriendlyTypeName(options.GetType()));
                    throw new BsonSerializationException(message);
                }

                bsonWriter.WriteStartDocument();
                BsonMemberMap idMemberMap = null;
                if (documentSerializationOptions.SerializeIdFirst)
                {
                    idMemberMap = _classMap.IdMemberMap;
                    if (idMemberMap != null)
                    {
                        SerializeMember(bsonWriter, value, idMemberMap);
                    }
                }

                if (actualType != nominalType || _classMap.DiscriminatorIsRequired || _classMap.HasRootClass)
                {
                    // never write out a discriminator for an anonymous class
                    if (!_classMap.IsAnonymous)
                    {
                        var discriminatorConvention = _classMap.GetDiscriminatorConvention();
                        var discriminator = discriminatorConvention.GetDiscriminator(nominalType, actualType);
                        if (discriminator != null)
                        {
                            bsonWriter.WriteName(discriminatorConvention.ElementName);
                            BsonValueSerializer.Instance.Serialize(bsonWriter, typeof(BsonValue), discriminator, null);
                        }
                    }
                }

                var allMemberMaps = _classMap.AllMemberMaps;
                var extraElementsMemberMapIndex = _classMap.ExtraElementsMemberMapIndex;

                for (var memberMapIndex = 0; memberMapIndex < allMemberMaps.Count; ++memberMapIndex)
                {
                    var memberMap = allMemberMaps[memberMapIndex];
                    // note: if serializeIdFirst is false then idMemberMap will be null (so no property will be skipped)
                    if (memberMap != idMemberMap)
                    {
                        if (memberMapIndex != extraElementsMemberMapIndex)
                        {
                            SerializeMember(bsonWriter, value, memberMap);
                        }
                        else
                        {
                            SerializeExtraElements(bsonWriter, value, memberMap);
                        }
                    }
                }
                bsonWriter.WriteEndDocument();
            }
        }

        /// <summary>
        /// Sets the document Id.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="id">The Id.</param>
        public void SetDocumentId(object document, object id)
        {
            var documentType = document.GetType();
            if (documentType.IsValueType)
            {
                var message = string.Format("SetDocumentId cannot be used with value type {0}.", documentType.FullName);
                throw new BsonSerializationException(message);
            }

            var idMemberMap = _classMap.IdMemberMap;
            if (idMemberMap != null)
            {
                idMemberMap.Setter(document, id);
            }
            else
            {
                var message = string.Format("Class {0} has no Id member.", document.GetType().FullName);
                throw new InvalidOperationException(message);
            }
        }

        // private methods
        private BsonCreatorMap ChooseBestCreator(Dictionary<string, object> values)
        {
            // there's only one selector for now, but there might be more in the future (possibly even user provided)
            var selector = new MostArgumentsCreatorSelector();
            var creatorMap = selector.SelectCreator(_classMap, values);

            if (creatorMap == null)
            {
                throw new BsonSerializationException("No matching creator found.");
            }

            return creatorMap;
        }

        private object CreateInstanceUsingCreator(Dictionary<string, object> values)
        {
            var creatorMap = ChooseBestCreator(values);
            var obj = creatorMap.CreateInstance(values); // removes values consumed

            var supportsInitialization = obj as ISupportInitialize;
            if (supportsInitialization != null)
            {
                supportsInitialization.BeginInit();
            }

            // process any left over values that weren't passed to the creator
            foreach (var keyValuePair in values)
            {
                var elementName = keyValuePair.Key;
                var value = keyValuePair.Value;

                var memberMap = _classMap.GetMemberMapForElement(elementName);
                if (!memberMap.IsReadOnly)
                {
                    memberMap.Setter.Invoke(obj, value);
                }
            }

            if (supportsInitialization != null)
            {
                supportsInitialization.EndInit();
            }

            return obj;
        }

        private void DeserializeExtraElement(
            BsonReader bsonReader,
            object obj,
            string elementName,
            BsonMemberMap extraElementsMemberMap)
        {
            if (extraElementsMemberMap.MemberType == typeof(BsonDocument))
            {
                var extraElements = (BsonDocument)extraElementsMemberMap.Getter(obj);
                if (extraElements == null)
                {
                    extraElements = new BsonDocument();
                    extraElementsMemberMap.Setter(obj, extraElements);
                }
                var bsonValue = (BsonValue)BsonValueSerializer.Instance.Deserialize(bsonReader, typeof(BsonValue), null);
                extraElements[elementName] = bsonValue;
            }
            else
            {
                var extraElements = (IDictionary<string, object>)extraElementsMemberMap.Getter(obj);
                if (extraElements == null)
                {
                    if (extraElementsMemberMap.MemberType == typeof(IDictionary<string, object>))
                    {
                        extraElements = new Dictionary<string, object>();
                    }
                    else
                    {
                        extraElements = (IDictionary<string, object>)Activator.CreateInstance(extraElementsMemberMap.MemberType);
                    }
                    extraElementsMemberMap.Setter(obj, extraElements);
                }
                var bsonValue = (BsonValue)BsonValueSerializer.Instance.Deserialize(bsonReader, typeof(BsonValue), null);
                extraElements[elementName] = BsonTypeMapper.MapToDotNetValue(bsonValue);
            }
        }

        private object DeserializeMemberValue(BsonReader bsonReader, BsonMemberMap memberMap)
        {
            try
            {
                var nominalType = memberMap.MemberType;

                var bsonType = bsonReader.GetCurrentBsonType();
                if (bsonType == BsonType.Null && nominalType.IsInterface)
                {
                    bsonReader.ReadNull();
                    return null;
                }

                Type actualType;
                if (bsonType == BsonType.Null)
                {
                    actualType = nominalType;
                }
                else
                {
                    var discriminatorConvention = memberMap.GetDiscriminatorConvention();
                    actualType = discriminatorConvention.GetActualType(bsonReader, nominalType); // returns nominalType if no discriminator found
                }

                var serializer = memberMap.GetSerializer(actualType);
                return serializer.Deserialize(bsonReader, nominalType, actualType, memberMap.SerializationOptions);
            }
            catch (Exception ex)
            {
                var message = string.Format(
                    "An error occurred while deserializing the {0} {1} of class {2}: {3}", // terminating period provided by nested message
                    memberMap.MemberName, (memberMap.MemberInfo.MemberType == MemberTypes.Field) ? "field" : "property", memberMap.ClassMap.ClassType.FullName, ex.Message);
                throw new Exception(message, ex);
            }
        }

        private void SerializeExtraElements(BsonWriter bsonWriter, object obj, BsonMemberMap extraElementsMemberMap)
        {
            var extraElements = extraElementsMemberMap.Getter(obj);
            if (extraElements != null)
            {
                if (extraElementsMemberMap.MemberType == typeof(BsonDocument))
                {
                    var bsonDocument = (BsonDocument)extraElements;
                    foreach (var element in bsonDocument)
                    {
                        bsonWriter.WriteName(element.Name);
                        BsonValueSerializer.Instance.Serialize(bsonWriter, typeof(BsonValue), element.Value, null);
                    }
                }
                else
                {
                    var dictionary = (IDictionary<string, object>)extraElements;
                    foreach (var key in dictionary.Keys)
                    {
                        bsonWriter.WriteName(key);
                        var value = dictionary[key];
                        if (value == null)
                        {
                            bsonWriter.WriteNull();
                        }
                        else
                        {
                            var bsonValue = BsonTypeMapper.MapToBsonValue(dictionary[key]);
                            BsonValueSerializer.Instance.Serialize(bsonWriter, typeof(BsonValue), bsonValue, null);
                        }
                    }
                }
            }
        }

        private void SerializeMember(BsonWriter bsonWriter, object obj, BsonMemberMap memberMap)
        {
            var value = memberMap.Getter(obj);

            if (!memberMap.ShouldSerialize(obj, value))
            {
                return; // don't serialize member
            }

            bsonWriter.WriteName(memberMap.ElementName);
            var nominalType = memberMap.MemberType;
            if (value == null && nominalType.IsInterface)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = (value == null) ? nominalType : value.GetType();
                var serializer = memberMap.GetSerializer(actualType);
                serializer.Serialize(bsonWriter, nominalType, value, memberMap.SerializationOptions);
            }
        }

        private void VerifyNominalType(Type nominalType)
        {
            if (!(nominalType.IsClass || (nominalType.IsValueType && !nominalType.IsPrimitive) || nominalType.IsInterface) ||
                typeof(Array).IsAssignableFrom(nominalType))
            {
                string message = string.Format("BsonClassMapSerializer cannot be used with type {0}.", nominalType.FullName);
                throw new BsonSerializationException(message);
            }
        }

        // nested classes
        // helper class that implements member map bit array helper functions
        private static class FastMemberMapHelper
        {
            public static uint[] GetBitArray(int memberCount)
            {
                var bitArrayOffset = memberCount & 31;
                var bitArrayLength = memberCount >> 5;
                if (bitArrayOffset == 0)
                {
                    return new uint[bitArrayLength];
                }
                var bitArray = new uint[bitArrayLength + 1];
                bitArray[bitArrayLength] = ~0U << bitArrayOffset; // set unused bits to 1
                return bitArray;
            }

            // see http://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightBinSearch
            // also returns 31 if no bits are set; caller must check this case
            public static int GetLeastSignificantBit(uint bitBlock)
            {
                var leastSignificantBit = 1;
                if ((bitBlock & 65535) == 0)
                {
                    bitBlock >>= 16;
                    leastSignificantBit |= 16;
                }
                if ((bitBlock & 255) == 0)
                {
                    bitBlock >>= 8;
                    leastSignificantBit |= 8;
                }
                if ((bitBlock & 15) == 0)
                {
                    bitBlock >>= 4;
                    leastSignificantBit |= 4;
                }
                if ((bitBlock & 3) == 0)
                {
                    bitBlock >>= 2;
                    leastSignificantBit |= 2;
                }
                return leastSignificantBit - (int)(bitBlock & 1);
            }
        }
    }
}
