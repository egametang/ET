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

using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents an abstract AggregateFacetResult with an arbitrary TOutput type.
    /// </summary>
    public abstract class AggregateFacetResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateFacetResult" /> class.
        /// </summary>
        /// <param name="name">The name of the facet.</param>
        internal AggregateFacetResult(string name)
        {
            Name = Ensure.IsNotNull(name, nameof(name));
        }

        /// <summary>
        /// Gets the name of the facet.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the output of the facet.
        /// </summary>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <returns>The output of the facet.</returns>
        public IReadOnlyList<TOutput> Output<TOutput>()
        {
            return ((AggregateFacetResult<TOutput>)this).Output;
        }
    }

    /// <summary>
    /// Represents the result of a single facet.
    /// </summary>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    public sealed class AggregateFacetResult<TOutput> : AggregateFacetResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateFacetResult{TOutput}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="output">The output.</param>
        public AggregateFacetResult(string name, IEnumerable<TOutput> output)
            : base(name)
        {
            Ensure.IsNotNull(output, nameof(output));

            var readOnlyList = output as IReadOnlyList<TOutput>;
            if (readOnlyList != null)
            {
                Output = readOnlyList;
            }
            else
            {
                Output = output.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        public IReadOnlyList<TOutput> Output { get; set; }
    }
}
