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
using System.Diagnostics;
using System.Reflection;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Events.Diagnostics
{
    /// <summary>
    /// An event subscriber that writes command events to a trace source.
    /// </summary>
    public sealed class TraceSourceCommandEventSubscriber : IEventSubscriber
    {
        private readonly TraceSource _traceSource;
        private readonly ReflectionEventSubscriber _subscriber;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceSourceCommandEventSubscriber"/> class.
        /// </summary>
        /// <param name="traceSource">The trace source.</param>
        public TraceSourceCommandEventSubscriber(TraceSource traceSource)
        {
            _traceSource = Ensure.IsNotNull(traceSource, nameof(traceSource));
            _subscriber = new ReflectionEventSubscriber(this, bindingFlags: BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <inheritdoc />
        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            return _subscriber.TryGetEventHandler(out handler);
        }

        private void Handle(CommandStartedEvent @event)
        {
            if (_traceSource.Switch.ShouldTrace(TraceEventType.Verbose))
            {
                Debug(TraceSourceEventHelper.CommandIdBase, "{0}-{1}: sending {2}.", TraceSourceEventHelper.Label(@event.ConnectionId), @event.RequestId, @event.Command);
            }
            else
            {
                Info(TraceSourceEventHelper.CommandIdBase, "{0}-{1}: sending {2}.", TraceSourceEventHelper.Label(@event.ConnectionId), @event.RequestId, @event.CommandName);
            }
        }

        private void Handle(CommandSucceededEvent @event)
        {
            if (_traceSource.Switch.ShouldTrace(TraceEventType.Verbose))
            {
                Debug(TraceSourceEventHelper.CommandIdBase + 1, "{0}-{1}: {2} succeeded: {3}.", TraceSourceEventHelper.Label(@event.ConnectionId), @event.RequestId, @event.CommandName, @event.Reply);
            }
            else
            {
                Info(TraceSourceEventHelper.CommandIdBase + 1, "{0}-{1}: {2} succeeded.", TraceSourceEventHelper.Label(@event.ConnectionId), @event.RequestId, @event.CommandName);
            }
        }

        private void Handle(CommandFailedEvent @event)
        {
            Error(TraceSourceEventHelper.CommandIdBase + 2, @event.Failure, "{0}-{1}: {2} failed.", TraceSourceEventHelper.Label(@event.ConnectionId), @event.RequestId, @event.CommandName);
        }

        private void Debug(int id, string message, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, id, message, args);
        }

        private void Info(int id, string message, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Information, id, message, args);
        }

        private void Error(int id, Exception ex, string message, params object[] args)
        {
            _traceSource.TraceEvent(
                TraceEventType.Error,
                id,
                message + Environment.NewLine + ex.ToString(),
                args);
        }
    }
}
