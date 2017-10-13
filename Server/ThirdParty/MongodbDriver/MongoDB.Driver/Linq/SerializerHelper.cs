/* Copyright 2015-2017 MongoDB Inc.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Linq
{
    internal static class SerializerHelper
    {
        public static IBsonSerializer CreateArraySerializer(IBsonSerializer serializer)
        {
            return (IBsonSerializer)Activator.CreateInstance(
                typeof(ArraySerializer<>).MakeGenericType(serializer.ValueType),
                serializer);
        }

        public static IBsonSerializer CreateEnumerableSerializer(IBsonSerializer itemSerializer)
        {
            var listSerializer = CreateListSerializer(itemSerializer);

            return (IBsonSerializer)Activator.CreateInstance(
                typeof(ImpliedImplementationInterfaceSerializer<,>).MakeGenericType(
                    typeof(IEnumerable<>).MakeGenericType(itemSerializer.ValueType),
                    listSerializer.ValueType),
                listSerializer);
        }

        public static IBsonSerializer CreateHashSetSerializer(IBsonSerializer itemSerializer)
        {
            var implementationType = typeof(HashSet<>).MakeGenericType(itemSerializer.ValueType);
            return (IBsonSerializer)Activator.CreateInstance(
                typeof(EnumerableInterfaceImplementerSerializer<,>).MakeGenericType(
                    implementationType,
                    itemSerializer.ValueType),
                itemSerializer);
        }

        public static IBsonSerializer CreateListSerializer(IBsonSerializer itemSerializer)
        {
            var implementationType = typeof(List<>).MakeGenericType(itemSerializer.ValueType);
            return (IBsonSerializer)Activator.CreateInstance(
                typeof(EnumerableInterfaceImplementerSerializer<>).MakeGenericType(
                    implementationType),
                itemSerializer);
        }

        public static IBsonSerializer RecursiveConfigureChildSerializer(IChildSerializerConfigurable configurable, IBsonSerializer childSerializer)
        {
            var childConfigurable = configurable.ChildSerializer as IChildSerializerConfigurable;
            if (childConfigurable != null)
            {
                childSerializer = RecursiveConfigureChildSerializer(childConfigurable, childSerializer);
            }

            return configurable.WithChildSerializer(childSerializer);
        }

        public static IBsonSerializer GetDeepestChildSerializer(IChildSerializerConfigurable childConfigurableSerializer)
        {
            IBsonSerializer deepestChildSerializer = null;

            while (childConfigurableSerializer != null)
            {
                deepestChildSerializer = childConfigurableSerializer.ChildSerializer;
                childConfigurableSerializer = deepestChildSerializer as IChildSerializerConfigurable;
            }

            return deepestChildSerializer;
        }
    }
}
