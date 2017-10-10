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
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver
{
    /// <summary>
    /// Model for inserting a single document.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
#if NET45
    [Serializable]
#endif
    public sealed class InsertOneModel<TDocument> : WriteModel<TDocument>
    {
        // fields
        private readonly TDocument _document;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertOneModel{TDocument}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public InsertOneModel(TDocument document)
        {
            _document = document;
        }

        // properties
        /// <summary>
        /// Gets the document.
        /// </summary>
        public TDocument Document
        {
            get { return _document; }
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        public override WriteModelType ModelType
        {
            get { return WriteModelType.InsertOne; }
        }
    }
}
