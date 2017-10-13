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
using System.Linq;
using System.Reflection;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Events
{
    /// <summary>
    /// Subscribes methods with a single argument to events 
    /// of that single argument's type.
    /// </summary>
    public sealed class ReflectionEventSubscriber : IEventSubscriber
    {
        private readonly Dictionary<Type, Delegate> _handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionEventSubscriber" /> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="methodName">Name of the method to match against.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        public ReflectionEventSubscriber(object instance, string methodName = "Handle", BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            Ensure.IsNotNull(instance, nameof(instance));
            Ensure.IsNotNullOrEmpty(methodName, nameof(methodName));

            _handlers = new Dictionary<Type, Delegate>();

            var methods = instance.GetType().GetTypeInfo().GetMethods(bindingFlags)
                .Where(x => x.Name == methodName &&
                    x.GetParameters().Length == 1 &&
                    x.ReturnType == typeof(void));

            foreach (var method in methods)
            {
                var eventType = method.GetParameters()[0].ParameterType;
                var delegateType = typeof(Action<>).MakeGenericType(eventType);
                var @delegate = method.CreateDelegate(delegateType, instance);
                _handlers.Add(eventType, @delegate);
            }
        }

        /// <inheritdoc />
        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            Delegate @delegate;
            if (_handlers.TryGetValue(typeof(TEvent), out @delegate))
            {
                handler = (Action<TEvent>)@delegate;
                return true;
            }

            handler = null;
            return false;
        }
    }
}
