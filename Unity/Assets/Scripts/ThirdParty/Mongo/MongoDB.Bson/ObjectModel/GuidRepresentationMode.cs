/* Copyright 2019-present MongoDB Inc.
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

namespace MongoDB.Bson
{
    /// <summary>
    /// Whether to handle GuidRepresentation using the v2.x mode of the v3.x mode.
    /// See the reference documentation for details.
    /// </summary>
    public enum GuidRepresentationMode
    {
        /// <summary>
        /// Handle GuidRepresentation using the v2.x mode.
        /// </summary>
        V2,

        /// <summary>
        /// Handle GuidRepresentation using the v3.x mode.
        /// </summary>
        V3
    }
}
