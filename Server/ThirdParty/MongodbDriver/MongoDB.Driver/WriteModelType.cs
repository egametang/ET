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

namespace MongoDB.Driver
{
    /// <summary>
    /// The type of a write model.
    /// </summary>
    public enum WriteModelType
    {
        /// <summary>
        /// A model to insert a single document.
        /// </summary>
        InsertOne,
        /// <summary>
        /// A model to delete a single document.
        /// </summary>
        DeleteOne,
        /// <summary>
        /// A model to delete multiple documents.
        /// </summary>
        DeleteMany,
        /// <summary>
        /// A model to replace a single document.
        /// </summary>
        ReplaceOne,
        /// <summary>
        /// A model to update a single document.
        /// </summary>
        UpdateOne,
        /// <summary>
        /// A model to update many documents.
        /// </summary>
        UpdateMany
    }
}
