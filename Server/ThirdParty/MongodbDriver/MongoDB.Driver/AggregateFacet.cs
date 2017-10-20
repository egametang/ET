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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents static methods for creating facets.
    /// </summary>
    public static class AggregateFacet
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AggregateFacet{TInput, TOutput}" /> class.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="name">The facet name.</param>
        /// <param name="pipeline">The facet pipeline.</param>
        /// <returns>
        /// A new instance of the <see cref="AggregateFacet{TInput, TOutput}" /> class
        /// </returns>
        public static AggregateFacet<TInput, TOutput> Create<TInput, TOutput>(string name, PipelineDefinition<TInput, TOutput> pipeline)
        {
            return new AggregateFacet<TInput, TOutput>(name, pipeline);
        }
    }

    /// <summary>
    /// Represents a facet to be passed to the Facet method.
    /// </summary>
    /// <typeparam name="TInput">The type of the input documents.</typeparam>
    public abstract class AggregateFacet<TInput>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateFacet{TInput}"/> class.
        /// </summary>
        /// <param name="name">The facet name.</param>
        protected AggregateFacet(string name)
        {
            Name = Ensure.IsNotNull(name, nameof(name));
        }

        /// <summary>
        /// Gets the facet name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the output serializer.
        /// </summary>
        public abstract IBsonSerializer OutputSerializer { get; }

        /// <summary>
        /// Gets the type of the output documents.
        /// </summary>
        public abstract Type OutputType { get; }

        /// <summary>
        /// Renders the facet pipeline.
        /// </summary>
        /// <param name="inputSerializer">The input serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>The rendered pipeline.</returns>
        public abstract BsonArray RenderPipeline(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry);
    }

    /// <summary>
    /// Represents a facet to be passed to the Facet method.
    /// </summary>
    /// <typeparam name="TInput">The type of the input documents.</typeparam>
    /// <typeparam name="TOutput">The type of the otuput documents.</typeparam>
    public class AggregateFacet<TInput, TOutput> : AggregateFacet<TInput>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateFacet{TInput, TOutput}"/> class.
        /// </summary>
        /// <param name="name">The facet name.</param>
        /// <param name="pipeline">The facet pipeline.</param>
        public AggregateFacet(string name, PipelineDefinition<TInput, TOutput> pipeline)
            : base(name)
        {
            Pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline));
        }

        /// <inheritdoc/>
        public override IBsonSerializer OutputSerializer => Pipeline.OutputSerializer;

        /// <inheritdoc/>
        public override Type OutputType => typeof(TOutput);

        /// <summary>
        /// Gets the facet pipeline.
        /// </summary>
        public PipelineDefinition<TInput, TOutput> Pipeline { get; private set; }

        /// <inheritdoc/>
        public override BsonArray RenderPipeline(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedPipeline = Pipeline.Render(inputSerializer, serializerRegistry);
            return new BsonArray(renderedPipeline.Documents);
        }
    }
}
