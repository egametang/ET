/* Copyright 2013-2015 MongoDB Inc.
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

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents the map-reduce output mode.
    /// </summary>
    public enum MapReduceOutputMode
    {
        /// <summary>
        /// The output of the map-reduce operation replaces the output collection.
        /// </summary>
        Replace = 0,

        /// <summary>
        /// The output of the map-reduce operation is merged with the output collection.
        /// If an existing document has the same key as the new result, overwrite the existing document.
        /// </summary>
        Merge,

        /// <summary>
        /// The output of the map-reduce operation is merged with the output collection.
        /// If an existing document has the same key as the new result, apply the reduce function to both
        /// the new and the existing documents and overwrite the existing document with the result.
        /// </summary>
        Reduce
    }
}
