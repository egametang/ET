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

using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Operations.ElementNameValidators;
namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents the update type.
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// The update type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// This update uses an update specification to update an existing document.
        /// </summary>
        Update,

        /// <summary>
        /// This update completely replaces an existing document  with a new one.
        /// </summary>
        Replacement
    }
}
