/* Copyright 2017-present MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// The operation type of a change stream result.
    /// </summary>
    public enum ChangeStreamOperationType
    {
        /// <summary>
        /// An insert operation type.
        /// </summary>
        Insert,
        /// <summary>
        /// An update operation type.
        /// </summary>
        Update,
        /// <summary>
        /// A replace operation type.
        /// </summary>
        Replace,
        /// <summary>
        /// A delete operation type.
        /// </summary>
        Delete,
        /// <summary>
        /// An invalidate operation type.
        /// </summary>
        Invalidate
    }
}
