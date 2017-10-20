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

namespace MongoDB.Driver.Core.Events
{
    /// <summary>
    /// A subscriber to events.
    /// </summary>
    public interface IEventSubscriber
    {
        /// <summary>
        /// Tries to get an event handler for an event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <returns><c>true</c> if this subscriber has provided an event handler; otherwise <c>false</c>.</returns>
        bool TryGetEventHandler<TEvent>(out Action<TEvent> handler);
    }
}
