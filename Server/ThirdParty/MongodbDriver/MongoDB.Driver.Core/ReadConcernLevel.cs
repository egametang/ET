/* Copyright 2015 MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// The leve of the read concern.
    /// </summary>
    public enum ReadConcernLevel
    {
        /// <summary>
        /// Reads data committed locally.
        /// </summary>
        Local,
        /// <summary>
        /// Reads data committed to a majority of nodes.
        /// </summary>
        Majority,
        /// <summary>
        /// Avoids returning data from a "stale" primary 
        /// (one that has already been superseded by a new primary but doesn't know it yet). 
        /// It is important to note that readConcern level linearizable does not by itself 
        /// produce linearizable reads; they must be issued in conjunction with w:majority 
        /// writes to the same document(s) in order to be linearizable.
        /// </summary>
        Linearizable
    }
}
