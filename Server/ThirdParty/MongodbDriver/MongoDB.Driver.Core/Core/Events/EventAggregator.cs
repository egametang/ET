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
using System.Collections.Generic;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Events
{
    internal class EventAggregator : IEventSubscriber
    {
        private readonly List<IEventSubscriber> _subscribers;

        public EventAggregator()
        {
            _subscribers = new List<IEventSubscriber>();
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            Subscribe(new SingleEventSubscriber<TEvent>(handler));
        }

        public void Subscribe(IEventSubscriber subscriber)
        {
            Ensure.IsNotNull(subscriber, nameof(subscriber));

            _subscribers.Add(subscriber);
        }

        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            handler = null;
            foreach (var subscriber in _subscribers)
            {
                Action<TEvent> local;
                if (subscriber.TryGetEventHandler<TEvent>(out local))
                {
                    if (handler == null)
                    {
                        handler = local;
                    }
                    else
                    {
                        handler = (Action<TEvent>)Delegate.Combine(handler, local);
                    }
                }
            }

            return handler != null;
        }
    }
}