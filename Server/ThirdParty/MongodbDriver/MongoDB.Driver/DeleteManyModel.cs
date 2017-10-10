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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver
{
    /// <summary>
    /// Model for deleting many documents.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
#if NET45
    [Serializable]
#endif
    public sealed class DeleteManyModel<TDocument> : WriteModel<TDocument>
    {
        // fields
        private Collation _collation;
        private readonly FilterDefinition<TDocument> _filter;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteManyModel{TDocument}"/> class.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public DeleteManyModel(FilterDefinition<TDocument> filter)
        {
            _filter = Ensure.IsNotNull(filter, nameof(filter));
        }

        // properties
        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        public FilterDefinition<TDocument> Filter
        {
            get { return _filter; }
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        public override WriteModelType ModelType
        {
            get { return WriteModelType.DeleteMany; }
        }
    }
}
