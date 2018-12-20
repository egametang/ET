/* Copyright 2018–present MongoDB Inc.
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
    /// An event subscriber that writes SDAM events to a trace source.
    /// </summary>
    public sealed class TraceSourceSdamEventSubscriber : IEventSubscriber
    {
        private readonly TraceSource _traceSource;
        private readonly ReflectionEventSubscriber _subscriber;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceSourceSdamEventSubscriber"/> class.
        /// </summary>
        /// <param name="traceSource">The trace source.</param>
        public TraceSourceSdamEventSubscriber(TraceSource traceSource)
        {
            _traceSource = Ensure.IsNotNull(traceSource, nameof(traceSource));
            _subscriber = new ReflectionEventSubscriber(this, bindingFlags: BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <inheritdoc />
        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            return _subscriber.TryGetEventHandler(out handler);
        }

        // Clusters
        private void Handle(ClusterOpeningEvent @event)
        {
            Info(TraceSourceEventHelper.ClusterIdBase, "{0}: opening.", TraceSourceEventHelper.Label(@event.ClusterId));
        }

        private void Handle(ClusterOpenedEvent @event)
        {
            Debug(TraceSourceEventHelper.ClusterIdBase + 1, "{0}: opened in {1}ms.", TraceSourceEventHelper.Label(@event.ClusterId), @event.Duration.TotalMilliseconds);
        }

        private void Handle(ClusterClosingEvent @event)
        {
            Debug(TraceSourceEventHelper.ClusterIdBase + 2, "{0}: closing.", TraceSourceEventHelper.Label(@event.ClusterId));
        }

        private void Handle(ClusterClosedEvent @event)
        {
            Info(TraceSourceEventHelper.ClusterIdBase + 3, "{0}: closed in {1}ms.", TraceSourceEventHelper.Label(@event.ClusterId), @event.Duration.TotalMilliseconds);
        }

        private void Handle(ClusterAddingServerEvent @event)
        {
            Info(TraceSourceEventHelper.ClusterIdBase + 4, "{0}: adding server at endpoint {1}.", TraceSourceEventHelper.Label(@event.ClusterId), TraceSourceEventHelper.Format(@event.EndPoint));
        }

        private void Handle(ClusterAddedServerEvent @event)
        {
            Debug(TraceSourceEventHelper.ClusterIdBase + 5, "{0}: added server {1} in {2}ms.", TraceSourceEventHelper.Label(@event.ServerId.ClusterId), TraceSourceEventHelper.Format(@event.ServerId), @event.Duration.TotalMilliseconds);
        }

        private void Handle(ClusterRemovingServerEvent @event)
        {
            Debug(TraceSourceEventHelper.ClusterIdBase + 6, "{0}: removing server {1}. Reason: {2}", TraceSourceEventHelper.Label(@event.ServerId.ClusterId), TraceSourceEventHelper.Format(@event.ServerId), @event.Reason);
        }

        private void Handle(ClusterRemovedServerEvent @event)
        {
            Info(TraceSourceEventHelper.ClusterIdBase + 7, "{0}: removed server {1} in {2}ms. Reason: {3}", TraceSourceEventHelper.Label(@event.ServerId.ClusterId), TraceSourceEventHelper.Format(@event.ServerId), @event.Duration.TotalMilliseconds, @event.Reason);
        }

        private void Handle(ClusterDescriptionChangedEvent @event)
        {
            Info(TraceSourceEventHelper.ClusterIdBase + 8, "{0}: {1}", TraceSourceEventHelper.Label(@event.OldDescription.ClusterId), @event.NewDescription);
        }
        
        private void Handle(SdamInformationEvent @event)
        {
            Info(TraceSourceEventHelper.ClusterIdBase + 9, "{0}", @event);
        }

        // Servers
        private void Handle(ServerOpeningEvent @event)
        {
            Info(TraceSourceEventHelper.ServerIdBase, "{0}: opening.", TraceSourceEventHelper.Label(@event.ServerId));
        }

        private void Handle(ServerOpenedEvent @event)
        {
            Debug(TraceSourceEventHelper.ServerIdBase + 1, "{0}: opened in {1}ms.", TraceSourceEventHelper.Label(@event.ServerId), @event.Duration.TotalMilliseconds);
        }

        private void Handle(ServerClosingEvent @event)
        {
            Debug(TraceSourceEventHelper.ServerIdBase + 2, "{0}: closing.", TraceSourceEventHelper.Label(@event.ServerId));
        }

        private void Handle(ServerClosedEvent @event)
        {
            Info(TraceSourceEventHelper.ServerIdBase + 3, "{0}: closed in {1}ms.", TraceSourceEventHelper.Label(@event.ServerId), @event.Duration.TotalMilliseconds);
        }

        private void Handle(ServerHeartbeatStartedEvent @event)
        {
            Debug(TraceSourceEventHelper.ServerIdBase + 4, "{0}: sending heartbeat.", TraceSourceEventHelper.Label(@event.ConnectionId));
        }

        private void Handle(ServerHeartbeatSucceededEvent @event)
        {
            Debug(TraceSourceEventHelper.ServerIdBase + 5, "{0}: sent heartbeat in {1}ms.", TraceSourceEventHelper.Label(@event.ConnectionId), @event.Duration.TotalMilliseconds);
        }

        private void Handle(ServerHeartbeatFailedEvent @event)
        {
            Error(TraceSourceEventHelper.ServerIdBase + 6, @event.Exception, "{0}: error sending heartbeat.", TraceSourceEventHelper.Label(@event.ConnectionId));
        }

        private void Handle(ServerDescriptionChangedEvent @event)
        {
            Debug(TraceSourceEventHelper.ServerIdBase + 7, "{0}: {1}", TraceSourceEventHelper.Label(@event.OldDescription.ServerId), @event.NewDescription);
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
