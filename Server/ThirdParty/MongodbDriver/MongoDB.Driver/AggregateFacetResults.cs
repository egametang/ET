/* Copyright 2016 MongoDB Inc.
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
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the results of a $facet stage with an arbitrary number of facets.
    /// </summary>
    public class AggregateFacetResults
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateFacetResults"/> class.
        /// </summary>
        /// <param name="facets">The facets.</param>
        public AggregateFacetResults(AggregateFacetResult[] facets)
        {
            Facets = Ensure.IsNotNull(facets, nameof(facets));
        }

        /// <summary>
        /// Gets the facets.
        /// </summary>
        public IReadOnlyList<AggregateFacetResult> Facets { get; private set; }
    }

    internal class AggregateFacetResultsSerializer : SerializerBase<AggregateFacetResults>
    {
        private readonly string[] _names;
        private readonly IBsonSerializer[] _serializers;

        public AggregateFacetResultsSerializer(IEnumerable<string> names, IEnumerable<IBsonSerializer> serializers)
        {
            _names = names.ToArray();
            _serializers = serializers.ToArray();
        }

        public override AggregateFacetResults Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var facets = new AggregateFacetResult[_names.Length];

            var reader = context.Reader;
            reader.ReadStartDocument();
            while (reader.ReadBsonType() != 0)
            {
                var name = reader.ReadName();
                var index = Array.IndexOf(_names, name);
                if (index != -1)
                {
                    var itemSerializer = _serializers[index];
                    var itemType = itemSerializer.ValueType;
                    var itemSerializerType = typeof(IBsonSerializer<>).MakeGenericType(itemType);
                    var arraySerializerType = typeof(ArraySerializer<>).MakeGenericType(itemType);
                    var arraySerializerConstructor = arraySerializerType.GetTypeInfo().GetConstructor(new[] { itemSerializerType });
                    var arraySerializer = (IBsonSerializer)arraySerializerConstructor.Invoke(new object[] { itemSerializer });
                    var output = (Array)arraySerializer.Deserialize(context);
                    var facetType = typeof(AggregateFacetResult<>).MakeGenericType(itemType);
                    var ienumerableItemType = typeof(IEnumerable<>).MakeGenericType(itemType);
                    var facetConstructor = facetType.GetTypeInfo().GetConstructor(new[] { typeof(string), ienumerableItemType });
                    var facet = (AggregateFacetResult)facetConstructor.Invoke(new object[] { name, output });
                    facets[index] = facet;
                }
                else
                {
                    throw new BsonSerializationException($"Unexpected field name '{name}' in $facet result.");
                }
            }
            reader.ReadEndDocument();

            var missingIndex = Array.IndexOf(facets, null);
            if (missingIndex != -1)
            {
                var missingName = _names[missingIndex];
                throw new BsonSerializationException($"Field name '{missingName}' in $facet result.");
            }


            return new AggregateFacetResults(facets);
        }
    }
}
