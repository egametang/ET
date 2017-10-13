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
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs after a server's description has changed.
    /// </summary>
    public struct ServerDescriptionChangedEvent
    {
        private readonly ServerDescription _oldDescription;
        private readonly ServerDescription _newDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDescriptionChangedEvent"/> struct.
        /// </summary>
        /// <param name="oldDescription">The old description.</param>
        /// <param name="newDescription">The new description.</param>
        public ServerDescriptionChangedEvent(ServerDescription oldDescription, ServerDescription newDescription)
        {
            _oldDescription = oldDescription;
            _newDescription = newDescription;
        }

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        public ClusterId ClusterId
        {
            get { return _newDescription.ServerId.ClusterId; }
        }

        /// <summary>
        /// Gets the new description.
        /// </summary>
        public ServerDescription NewDescription
        {
            get { return _newDescription; }
        }

        /// <summary>
        /// Gets the old description.
        /// </summary>
        public ServerDescription OldDescription
        {
            get { return _oldDescription; }
        }

        /// <summary>
        /// Gets the server identifier.
        /// </summary>
        public ServerId ServerId
        {
            get { return _newDescription.ServerId; }
        }
    }
}
