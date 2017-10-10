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

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// The verbosity of an explanation.
    /// </summary>
    public enum ExplainVerbosity
    {
        /// <summary>
        /// Runs the query planner and chooses the winning plan, but does not actually execute it.
        /// </summary>
        QueryPlanner,

        /// <summary>
        /// Runs the query optimizer, and then runs the winning plan to completion. In addition to the
        /// planner information, this makes execution stats available.
        /// </summary>
        ExecutionStats,

        /// <summary>
        /// Runs the query optimizer and chooses the winning plan, but then runs all generated plans
        /// to completion. This makes execution stats available for all of the query plans.
        /// </summary>
        AllPlansExecution
    }
}
