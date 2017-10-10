/* Copyright 2010-2015 MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver
{
    /// <summary>
    /// Model for creating an index.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class CreateIndexModel<TDocument>
    {
        private readonly IndexKeysDefinition<TDocument> _keys;
        private readonly CreateIndexOptions<TDocument> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIndexModel{TDocument}"/> class.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="options">The options.</param>
        public CreateIndexModel(IndexKeysDefinition<TDocument> keys, CreateIndexOptions options = null)
        {
            _keys = Ensure.IsNotNull(keys, nameof(keys));
            _options = CreateIndexOptions<TDocument>.CoercedFrom(options);
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        public IndexKeysDefinition<TDocument> Keys
        {
            get { return _keys; }
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        public CreateIndexOptions<TDocument> Options
        {
            get { return _options; }
        }
    }
}
