/* Copyright 2018-present MongoDB Inc.
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
using System.Reflection;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for a class that implements <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TDictionary">The type of the dictionary.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> :
        DictionarySerializerBase<TDictionary, TKey, TValue>,
        IChildSerializerConfigurable,
        IDictionaryRepresentationConfigurable<ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>>
            where TDictionary : class, IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDictionaryInterfaceImplementerSerializer{TDictionary, TKey, TValue}"/> class.
        /// </summary>
        public ReadOnlyDictionaryInterfaceImplementerSerializer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDictionaryInterfaceImplementerSerializer{TDictionary, TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        public ReadOnlyDictionaryInterfaceImplementerSerializer(DictionaryRepresentation dictionaryRepresentation)
            : base(dictionaryRepresentation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDictionaryInterfaceImplementerSerializer{TDictionary, TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <param name="keySerializer">The key serializer.</param>
        /// <param name="valueSerializer">The value serializer.</param>
        public ReadOnlyDictionaryInterfaceImplementerSerializer(DictionaryRepresentation dictionaryRepresentation, IBsonSerializer<TKey> keySerializer, IBsonSerializer<TValue> valueSerializer)
            : base(dictionaryRepresentation, keySerializer, valueSerializer)
        {
        }

        // public methods
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified dictionary representation.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> WithDictionaryRepresentation(DictionaryRepresentation dictionaryRepresentation)
        {
            return dictionaryRepresentation == DictionaryRepresentation 
                ? this 
                : new ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>(dictionaryRepresentation, KeySerializer, ValueSerializer);
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified dictionary representation and key value serializers.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <param name="keySerializer">The key serializer.</param>
        /// <param name="valueSerializer">The value serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> WithDictionaryRepresentation(DictionaryRepresentation dictionaryRepresentation, IBsonSerializer<TKey> keySerializer, IBsonSerializer<TValue> valueSerializer)
        {
            return dictionaryRepresentation == DictionaryRepresentation && keySerializer == KeySerializer && valueSerializer == ValueSerializer
                ? this
                : new ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>(dictionaryRepresentation, keySerializer, valueSerializer);
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified key serializer.
        /// </summary>
        /// <param name="keySerializer">The key serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> WithKeySerializer(IBsonSerializer<TKey> keySerializer)
        {
            return keySerializer == KeySerializer 
                ? this 
                : new ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>(DictionaryRepresentation, keySerializer, ValueSerializer);
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified value serializer.
        /// </summary>
        /// <param name="valueSerializer">The value serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue> WithValueSerializer(IBsonSerializer<TValue> valueSerializer)
        {
            return valueSerializer == ValueSerializer 
                ? this 
                : new ReadOnlyDictionaryInterfaceImplementerSerializer<TDictionary, TKey, TValue>(DictionaryRepresentation, KeySerializer, valueSerializer);
        }

        // explicit interface implementations
        IBsonSerializer IChildSerializerConfigurable.ChildSerializer => ValueSerializer;
        
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
            return new Dictionary<TKey, TValue>();
        }

        /// <inheritdoc/>
        protected override TDictionary FinalizeAccumulator(ICollection<KeyValuePair<TKey, TValue>> accumulator)
        {
            try
            {
                return (TDictionary) Activator.CreateInstance(typeof(TDictionary), new object[] {accumulator});
            }
            catch (MissingMethodException exception)
            {
                throw new MissingMethodException(
                    $"No suitable constructor found for IReadOnlyDictionary type: '{typeof(TDictionary).FullName}'.",
                    exception);
            }
        }
    }
}
