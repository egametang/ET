/* Copyright 2016 MongoDB Inc.
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

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// Represents the commands that write accept write concern concern feature.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Misc.Feature" />
    public class CommandsThatWriteAcceptWriteConcernFeature : Feature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandsThatWriteAcceptWriteConcernFeature"/> class.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="firstSupportedVersion">The first server version that supports the feature.</param>
        public CommandsThatWriteAcceptWriteConcernFeature(string name, SemanticVersion firstSupportedVersion)
            : base(name, firstSupportedVersion)
        {
        }

        /// <summary>
        /// Returns true if the write concern value supplied is one that should be sent to the server and the server version supports the commands that write accept write concern feature.
        /// </summary>
        /// <param name="serverVersion">The server version.</param>
        /// <param name="value">The write concern value.</param>
        /// <returns>Whether the write concern should be sent to the server.</returns>
        public bool ShouldSendWriteConcern(SemanticVersion serverVersion, WriteConcern value)
        {
            return value != null && !value.IsServerDefault && base.IsSupported(serverVersion);
        }
    }
}
