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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Servers
{
    /// <summary>
    /// Represents the arguments to the event that occurs when the server description changes.
    /// </summary>
    public class ServerDescriptionChangedEventArgs : EventArgs
    {
        // fields
        private readonly ServerDescription _oldServerDescription;
        private readonly ServerDescription _newServerDescription;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDescriptionChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldServerDescription">The old server description.</param>
        /// <param name="newServerDescription">The new server description.</param>
        public ServerDescriptionChangedEventArgs(ServerDescription oldServerDescription, ServerDescription newServerDescription)
        {
            _oldServerDescription = Ensure.IsNotNull(oldServerDescription, nameof(oldServerDescription));
            _newServerDescription = Ensure.IsNotNull(newServerDescription, nameof(newServerDescription));
        }

        // properties
        /// <summary>
        /// Gets the old server description.
        /// </summary>
        /// <value>
        /// The old server description.
        /// </value>
        public ServerDescription OldServerDescription
        {
            get { return _oldServerDescription; }
        }

        /// <summary>
        /// Gets the new server description.
        /// </summary>
        /// <value>
        /// The new server description.
        /// </value>
        public ServerDescription NewServerDescription
        {
            get { return _newServerDescription; }
        }
    }
}
