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

namespace MongoDB.Driver.Core.Clusters
{
    /// <summary>
    /// Represents the data for the event that fires when a cluster description changes.
    /// </summary>
    public class ClusterDescriptionChangedEventArgs : EventArgs
    {
        // fields
        private readonly ClusterDescription _oldClusterDescription;
        private readonly ClusterDescription _newClusterDescription;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterDescriptionChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldClusterDescription">The old cluster description.</param>
        /// <param name="newClusterDescription">The new cluster description.</param>
        public ClusterDescriptionChangedEventArgs(ClusterDescription oldClusterDescription, ClusterDescription newClusterDescription)
        {
            _oldClusterDescription = Ensure.IsNotNull(oldClusterDescription, nameof(oldClusterDescription));
            _newClusterDescription = Ensure.IsNotNull(newClusterDescription, nameof(newClusterDescription));
        }

        // properties
        /// <summary>
        /// Gets the old cluster description.
        /// </summary>
        /// <value>
        /// The old cluster description.
        /// </value>
        public ClusterDescription OldClusterDescription
        {
            get { return _oldClusterDescription; }
        }

        /// <summary>
        /// Gets the new cluster description.
        /// </summary>
        /// <value>
        /// The new cluster description.
        /// </value>
        public ClusterDescription NewClusterDescription
        {
            get { return _newClusterDescription; }
        }
    }
}
