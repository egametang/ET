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

using System;

namespace MongoDB.Driver.Core.Servers
{
    /// <summary>
    /// Monitors a server for state changes.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal interface IServerMonitor : IDisposable
    {
        ServerDescription Description { get; }

        /// <summary>
        /// Occurs when the server description changes.
        /// </summary>
        event EventHandler<ServerDescriptionChangedEventArgs> DescriptionChanged;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Instructs the monitor to refresh its description immediately.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Requests a heartbeat as soon as possible.
        /// </summary>
        void RequestHeartbeat();
    }
}
