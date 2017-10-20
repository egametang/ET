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

namespace MongoDB.Driver.Core.Events
{
    /// <summary>
    /// Subscriber for a single type of event.
    /// </summary>
    /// <typeparam name="TSingleEvent">The type of the single event.</typeparam>
    public sealed class SingleEventSubscriber<TSingleEvent> : IEventSubscriber
    {
        private readonly Delegate _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleEventSubscriber{TSingleEvent}"/> class.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public SingleEventSubscriber(Action<TSingleEvent> handler)
        {
            _handler = Ensure.IsNotNull(handler, nameof(handler));
        }

        /// <inheritdoc />
        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            if (typeof(TEvent) == typeof(TSingleEvent))
            {
                handler = (Action<TEvent>)_handler;
                return true;
            }

            handler = null;
            return false;
        }
    }
}
