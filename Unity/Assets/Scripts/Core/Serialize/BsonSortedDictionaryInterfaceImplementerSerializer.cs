/* Copyright 2010-present MongoDB Inc.
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
using System.Collections.ObjectModel;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace ET
{
    /// <summary>
    /// Represents a serializer for a class that implements IDictionary.
    /// </summary>
    /// <typeparam name="TDictionary">The type of the dictionary.</typeparam>
    public class BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary> :
        BsonSortedDictionarySerializerBase<TDictionary>,
        IChildSerializerConfigurable,
        IDictionaryRepresentationConfigurable
            where TDictionary : class, IDictionary, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryInterfaceImplementerSerializer{TDictionary}"/> class.
        /// </summary>
        public BsonSortedDictionaryInterfaceImplementerSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryInterfaceImplementerSerializer{TDictionary}"/> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        public BsonSortedDictionaryInterfaceImplementerSerializer(DictionaryRepresentation dictionaryRepresentation)
            : base(dictionaryRepresentation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryInterfaceImplementerSerializer{TDictionary}"/> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <param name="keySerializer">The key serializer.</param>
        /// <param name="valueSerializer">The value serializer.</param>
        public BsonSortedDictionaryInterfaceImplementerSerializer(DictionaryRepresentation dictionaryRepresentation, IBsonSerializer keySerializer, IBsonSerializer valueSerializer)
            : base(dictionaryRepresentation, keySerializer, valueSerializer)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified dictionary representation.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary> WithDictionaryRepresentation(DictionaryRepresentation dictionaryRepresentation)
        {
            if (dictionaryRepresentation == DictionaryRepresentation)
            {
                return this;
            }
            else
            {
                return new BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary>(dictionaryRepresentation, KeySerializer, ValueSerializer);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified dictionary representation and key value serializers.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <param name="keySerializer">The key serializer.</param>
        /// <param name="valueSerializer">The value serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary> WithDictionaryRepresentation(DictionaryRepresentation dictionaryRepresentation, IBsonSerializer keySerializer, IBsonSerializer valueSerializer)
        {
            if (dictionaryRepresentation == DictionaryRepresentation && keySerializer == KeySerializer && valueSerializer == ValueSerializer)
            {
                return this;
            }
            else
            {
                return new BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary>(dictionaryRepresentation, keySerializer, valueSerializer);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified key serializer.
        /// </summary>
        /// <param name="keySerializer">The key serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary> WithKeySerializer(IBsonSerializer keySerializer)
        {
            if (keySerializer == KeySerializer)
            {
                return this;
            }
            else
            {
                return new BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary>(DictionaryRepresentation, keySerializer, ValueSerializer);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified value serializer.
        /// </summary>
        /// <param name="valueSerializer">The value serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary> WithValueSerializer(IBsonSerializer valueSerializer)
        {
            if (valueSerializer == ValueSerializer)
            {
                return this;
            }
            else
            {
                return new BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary>(DictionaryRepresentation, KeySerializer, valueSerializer);
            }
        }

        // protected methods
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <returns>The instance.</returns>
        protected override TDictionary CreateInstance()
        {
            return new TDictionary();
        }

        // explicit interface implementations
        IBsonSerializer IChildSerializerConfigurable.ChildSerializer
        {
            get { return ValueSerializer; }
        }

        IBsonSerializer IChildSerializerConfigurable.WithChildSerializer(IBsonSerializer childSerializer)
        {
            return WithValueSerializer(childSerializer);
        }

        IBsonSerializer IDictionaryRepresentationConfigurable.WithDictionaryRepresentation(DictionaryRepresentation dictionaryRepresentation)
        {
            return WithDictionaryRepresentation(dictionaryRepresentation);
        }
    }

    /// <summary>
    /// Represents a serializer for a class that implements <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TDictionary">The type of the dictionary.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> :
        SortedDictionarySerializerBase<TDictionary, TKey, TValue>,
        IChildSerializerConfigurable,
        IDictionaryRepresentationConfigurable<BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>>
            where TDictionary : class, IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryInterfaceImplementerSerializer{TDictionary, TKey, TValue}"/> class.
        /// </summary>
        public BsonSortedDictionaryInterfaceImplementerSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryInterfaceImplementerSerializer{TDictionary, TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        public BsonSortedDictionaryInterfaceImplementerSerializer(DictionaryRepresentation dictionaryRepresentation)
            : base(dictionaryRepresentation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryInterfaceImplementerSerializer{TDictionary, TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <param name="keySerializer">The key serializer.</param>
        /// <param name="valueSerializer">The value serializer.</param>
        public BsonSortedDictionaryInterfaceImplementerSerializer(DictionaryRepresentation dictionaryRepresentation, IBsonSerializer<TKey> keySerializer, IBsonSerializer<TValue> valueSerializer)
            : base(dictionaryRepresentation, keySerializer, valueSerializer)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified dictionary representation.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> WithDictionaryRepresentation(DictionaryRepresentation dictionaryRepresentation)
        {
            if (dictionaryRepresentation == DictionaryRepresentation)
            {
                return this;
            }
            else
            {
                return new BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>(dictionaryRepresentation, KeySerializer, ValueSerializer);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified dictionary representation and key value serializers.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <param name="keySerializer">The key serializer.</param>
        /// <param name="valueSerializer">The value serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> WithDictionaryRepresentation(DictionaryRepresentation dictionaryRepresentation, IBsonSerializer<TKey> keySerializer, IBsonSerializer<TValue> valueSerializer)
        {
            if (dictionaryRepresentation == DictionaryRepresentation && keySerializer == KeySerializer && valueSerializer == ValueSerializer)
            {
                return this;
            }
            else
            {
                return new BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>(dictionaryRepresentation, keySerializer, valueSerializer);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified key serializer.
        /// </summary>
        /// <param name="keySerializer">The key serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> WithKeySerializer(IBsonSerializer<TKey> keySerializer)
        {
            if (keySerializer == KeySerializer)
            {
                return this;
            }
            else
            {
                return new BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>(DictionaryRepresentation, keySerializer, ValueSerializer);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified value serializer.
        /// </summary>
        /// <param name="valueSerializer">The value serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> WithValueSerializer(IBsonSerializer<TValue> valueSerializer)
        {
            if (valueSerializer == ValueSerializer)
            {
                return this;
            }
            else
            {
                return new BsonSortedDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>(DictionaryRepresentation, KeySerializer, valueSerializer);
            }
        }

        // explicit interface implementations
        IBsonSerializer IChildSerializerConfigurable.ChildSerializer
        {
            get { return ValueSerializer; }
        }

        IBsonSerializer IChildSerializerConfigurable.WithChildSerializer(IBsonSerializer childSerializer)
        {
            return WithValueSerializer((IBsonSerializer<TValue>)childSerializer);
        }

        IBsonSerializer IDictionaryRepresentationConfigurable.WithDictionaryRepresentation(DictionaryRepresentation dictionaryRepresentation)
        {
            return WithDictionaryRepresentation(dictionaryRepresentation);
        }

        /// <inheritdoc/>
        protected override ICollection<KeyValuePair<TKey, TValue>> CreateAccumulator()
        {
            return Activator.CreateInstance<TDictionary>();
        }

    }
}
