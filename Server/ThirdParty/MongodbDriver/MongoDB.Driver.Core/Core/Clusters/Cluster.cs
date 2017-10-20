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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Async;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters
{
    /// <summary>
    /// Represents a cluster.
    /// </summary>
    internal abstract class Cluster : ICluster
    {
        #region static
        // static fields
        private static readonly TimeSpan __minHeartbeatInterval = TimeSpan.FromMilliseconds(500);
        private static readonly Range<int> __supportedWireVersionRange = new Range<int>(0, 3);
        private static readonly IServerSelector __randomServerSelector = new RandomServerSelector();
        #endregion

        // fields
        private readonly ClusterId _clusterId;
        private ClusterDescription _description;
        private TaskCompletionSource<bool> _descriptionChangedTaskCompletionSource;
        private readonly object _descriptionLock = new object();
        private Timer _rapidHeartbeatTimer;
        private readonly object _serverSelectionWaitQueueLock = new object();
        private int _serverSelectionWaitQueueSize;
        private readonly IClusterableServerFactory _serverFactory;
        private readonly ClusterSettings _settings;
        private readonly InterlockedInt32 _state;

        private readonly Action<ClusterDescriptionChangedEvent> _descriptionChangedEventHandler;
        private readonly Action<ClusterSelectingServerEvent> _selectingServerEventHandler;
        private readonly Action<ClusterSelectedServerEvent> _selectedServerEventHandler;
        private readonly Action<ClusterSelectingServerFailedEvent> _selectingServerFailedEventHandler;

        // constructors
        protected Cluster(ClusterSettings settings, IClusterableServerFactory serverFactory, IEventSubscriber eventSubscriber)
        {
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _serverFactory = Ensure.IsNotNull(serverFactory, nameof(serverFactory));
            Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));
            _state = new InterlockedInt32(State.Initial);

            _clusterId = new ClusterId();
            _description = ClusterDescription.CreateInitial(_clusterId, _settings.ConnectionMode);
            _descriptionChangedTaskCompletionSource = new TaskCompletionSource<bool>();

            _rapidHeartbeatTimer = new Timer(RapidHeartbeatTimerCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            eventSubscriber.TryGetEventHandler(out _descriptionChangedEventHandler);
            eventSubscriber.TryGetEventHandler(out _selectingServerEventHandler);
            eventSubscriber.TryGetEventHandler(out _selectedServerEventHandler);
            eventSubscriber.TryGetEventHandler(out _selectingServerFailedEventHandler);
        }

        // events
        public event EventHandler<ClusterDescriptionChangedEventArgs> DescriptionChanged;

        // properties
        public ClusterId ClusterId
        {
            get { return _clusterId; }
        }

        public ClusterDescription Description
        {
            get
            {
                lock (_descriptionLock)
                {
                    return _description;
                }
            }
        }

        public ClusterSettings Settings
        {
            get { return _settings; }
        }

        // methods
        protected IClusterableServer CreateServer(EndPoint endPoint)
        {
            return _serverFactory.CreateServer(_clusterId, endPoint);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_state.TryChange(State.Disposed))
            {
                var newClusterDescription = new ClusterDescription(
                    _clusterId,
                    _description.ConnectionMode,
                    ClusterType.Unknown,
                    Enumerable.Empty<ServerDescription>());

                UpdateClusterDescription(newClusterDescription);

                _rapidHeartbeatTimer.Dispose();
            }
        }

        private void EnterServerSelectionWaitQueue()
        {
            lock (_serverSelectionWaitQueueLock)
            {
                if (_serverSelectionWaitQueueSize >= _settings.MaxServerSelectionWaitQueueSize)
                {
                    throw MongoWaitQueueFullException.ForServerSelection();
                }

                if (++_serverSelectionWaitQueueSize == 1)
                {
                    _rapidHeartbeatTimer.Change(TimeSpan.Zero, __minHeartbeatInterval);
                }
            }
        }

        private void ExitServerSelectionWaitQueue()
        {
            lock (_serverSelectionWaitQueueLock)
            {
                if (--_serverSelectionWaitQueueSize == 0)
                {
                    _rapidHeartbeatTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                }
            }
        }

        public virtual void Initialize()
        {
            ThrowIfDisposed();
            _state.TryChange(State.Initial, State.Open);
        }

        private void RapidHeartbeatTimerCallback(object args)
        {
            try
            {
                RequestHeartbeat();
            }
            catch
            {
                // TODO: Trace this
                // If we don't protect this call, we could
                // take down the app domain.
            }
        }

        protected abstract void RequestHeartbeat();

        protected void OnDescriptionChanged(ClusterDescription oldDescription, ClusterDescription newDescription)
        {
            if (_descriptionChangedEventHandler != null)
            {
                _descriptionChangedEventHandler(new ClusterDescriptionChangedEvent(oldDescription, newDescription));
            }

            var handler = DescriptionChanged;
            if (handler != null)
            {
                var args = new ClusterDescriptionChangedEventArgs(oldDescription, newDescription);
                handler(this, args);
            }
        }

        public IServer SelectServer(IServerSelector selector, CancellationToken cancellationToken)
        {
            ThrowIfDisposedOrNotOpen();
            Ensure.IsNotNull(selector, nameof(selector));

            using (var helper = new SelectServerHelper(this, selector))
            {
                try
                {
                    while (true)
                    {
                        var server = helper.SelectServer();
                        if (server != null)
                        {
                            return server;
                        }

                        helper.WaitingForDescriptionToChange();
                        WaitForDescriptionChanged(helper.Selector, helper.Description, helper.DescriptionChangedTask, helper.TimeoutRemaining, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    helper.HandleException(ex);
                    throw;
                }
            }
        }

        public async Task<IServer> SelectServerAsync(IServerSelector selector, CancellationToken cancellationToken)
        {
            ThrowIfDisposedOrNotOpen();
            Ensure.IsNotNull(selector, nameof(selector));

            using (var helper = new SelectServerHelper(this, selector))
            {
                try
                {
                    while (true)
                    {
                        var server = helper.SelectServer();
                        if (server != null)
                        {
                            return server;
                        }

                        helper.WaitingForDescriptionToChange();
                        await WaitForDescriptionChangedAsync(helper.Selector, helper.Description, helper.DescriptionChangedTask, helper.TimeoutRemaining, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    helper.HandleException(ex);
                    throw;
                }
            }
        }

        protected abstract bool TryGetServer(EndPoint endPoint, out IClusterableServer server);

        protected void UpdateClusterDescription(ClusterDescription newClusterDescription)
        {
            ClusterDescription oldClusterDescription = null;
            TaskCompletionSource<bool> oldDescriptionChangedTaskCompletionSource = null;

            lock (_descriptionLock)
            {
                oldClusterDescription = _description;
                _description = newClusterDescription;

                oldDescriptionChangedTaskCompletionSource = _descriptionChangedTaskCompletionSource;
                _descriptionChangedTaskCompletionSource = new TaskCompletionSource<bool>();
            }

            OnDescriptionChanged(oldClusterDescription, newClusterDescription);
            oldDescriptionChangedTaskCompletionSource.TrySetResult(true);
        }

        private string BuildTimeoutExceptionMessage(TimeSpan timeout, IServerSelector selector, ClusterDescription clusterDescription)
        {
            var ms = (int)Math.Round(timeout.TotalMilliseconds);
            return string.Format(
                "A timeout occured after {0}ms selecting a server using {1}. Client view of cluster state is {2}.",
                ms.ToString(),
                selector.ToString(),
                clusterDescription.ToString());
        }

        private void ThrowIfDisposed()
        {
            if (_state.Value == State.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void ThrowIfDisposedOrNotOpen()
        {
            if (_state.Value != State.Open)
            {
                ThrowIfDisposed();
                throw new InvalidOperationException("Server must be initialized.");
            }
        }

        private void WaitForDescriptionChanged(IServerSelector selector, ClusterDescription description, Task descriptionChangedTask, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using (var helper = new WaitForDescriptionChangedHelper(this, selector, description, descriptionChangedTask, timeout, cancellationToken))
            {
                var index = Task.WaitAny(helper.Tasks);
                helper.HandleCompletedTask(helper.Tasks[index]);
            }
        }

        private async Task WaitForDescriptionChangedAsync(IServerSelector selector, ClusterDescription description, Task descriptionChangedTask, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using (var helper = new WaitForDescriptionChangedHelper(this, selector, description, descriptionChangedTask, timeout, cancellationToken))
            {
                var completedTask  = await Task.WhenAny(helper.Tasks).ConfigureAwait(false);
                helper.HandleCompletedTask(completedTask);
            }
        }

        private void ThrowTimeoutException(IServerSelector selector, ClusterDescription description)
        {
            var message = BuildTimeoutExceptionMessage(_settings.ServerSelectionTimeout, selector, description);
            throw new TimeoutException(message);
        }

        // nested classes
        private class SelectServerHelper : IDisposable
        {
            private readonly Cluster _cluster;
            private ClusterDescription _description;
            private Task _descriptionChangedTask;
            private bool _serverSelectionWaitQueueEntered;
            private readonly IServerSelector _selector;
            private readonly Stopwatch _stopwatch;
            private readonly DateTime _timeoutAt;

            public SelectServerHelper(Cluster cluster, IServerSelector selector)
            {
                _cluster = cluster;
                _selector = DecorateSelector(selector);
                _stopwatch = Stopwatch.StartNew();
                _timeoutAt = DateTime.UtcNow + _cluster.Settings.ServerSelectionTimeout;
            }

            public ClusterDescription Description
            {
                get { return _description; }
            }

            public Task DescriptionChangedTask
            {
                get { return _descriptionChangedTask; }
            }

            public IServerSelector Selector
            {
                get { return _selector; }
            }

            public TimeSpan TimeoutRemaining
            {
                get { return _timeoutAt - DateTime.UtcNow; }
            }

            public void Dispose()
            {
                if (_serverSelectionWaitQueueEntered)
                {
                    _cluster.ExitServerSelectionWaitQueue();
                }
            }

            public void HandleException(Exception exception)
            {
                var selectingServerFailedEventHandler = _cluster._selectingServerFailedEventHandler;
                if (selectingServerFailedEventHandler != null)
                {
                    selectingServerFailedEventHandler(new ClusterSelectingServerFailedEvent(
                        _description,
                        _selector,
                        exception,
                        EventContext.OperationId));
                }
            }

            public IServer SelectServer()
            {
                lock (_cluster._descriptionLock)
                {
                    _descriptionChangedTask = _cluster._descriptionChangedTaskCompletionSource.Task;
                    _description = _cluster._description;
                }

                if (!_serverSelectionWaitQueueEntered)
                {
                    var selectingServerEventHandler = _cluster._selectingServerEventHandler;
                    if (selectingServerEventHandler != null)
                    {
                        // this is our first time through...
                        selectingServerEventHandler(new ClusterSelectingServerEvent(
                            _description,
                            _selector,
                            EventContext.OperationId));
                    }
                }

                ThrowIfIncompatible(_description);

                var connectedServers = _description.Servers.Where(s => s.State == ServerState.Connected);
                var selectedServers = _selector.SelectServers(_description, connectedServers).ToList();

                while (selectedServers.Count > 0)
                {
                    var server = selectedServers.Count == 1 ?
                        selectedServers[0] :
                        __randomServerSelector.SelectServers(_description, selectedServers).Single();

                    IClusterableServer selectedServer;
                    if (_cluster.TryGetServer(server.EndPoint, out selectedServer))
                    {
                        _stopwatch.Stop();
                        var selectedServerEventHandler = _cluster._selectedServerEventHandler;
                        if (selectedServerEventHandler != null)
                        {
                            selectedServerEventHandler(new ClusterSelectedServerEvent(
                                _description,
                                _selector,
                                server,
                                _stopwatch.Elapsed,
                                EventContext.OperationId));
                        }
                        return selectedServer;
                    }

                    selectedServers.Remove(server);
                }

                return null;
            }

            public void WaitingForDescriptionToChange()
            {
                if (!_serverSelectionWaitQueueEntered)
                {
                    _cluster.EnterServerSelectionWaitQueue();
                    _serverSelectionWaitQueueEntered = true;
                }

                var timeoutRemaining = _timeoutAt - DateTime.UtcNow;
                if (timeoutRemaining <= TimeSpan.Zero)
                {
                    _cluster.ThrowTimeoutException(_selector, _description);
                }
            }

            private IServerSelector DecorateSelector(IServerSelector selector)
            {
                var settings = _cluster.Settings;
                if (settings.PreServerSelector != null || settings.PostServerSelector != null)
                {
                    var allSelectors = new List<IServerSelector>();
                    if (settings.PreServerSelector != null)
                    {
                        allSelectors.Add(settings.PreServerSelector);
                    }

                    allSelectors.Add(selector);

                    if (settings.PostServerSelector != null)
                    {
                        allSelectors.Add(settings.PostServerSelector);
                    }

                    return new CompositeServerSelector(allSelectors);
                }

                return selector;
            }
            private void ThrowIfIncompatible(ClusterDescription description)
            {
                var isIncompatible = description.Servers
                    .Any(sd => sd.WireVersionRange != null && !sd.WireVersionRange.Overlaps(__supportedWireVersionRange));

                if (isIncompatible)
                {
                    throw new MongoIncompatibleDriverException(description);
                }
            }
        }

        private sealed class WaitForDescriptionChangedHelper : IDisposable
        {
            private readonly CancellationToken _cancellationToken;
            private readonly TaskCompletionSource<bool> _cancellationTaskCompletionSource;
            private readonly CancellationTokenRegistration _cancellationTokenRegistration;
            private readonly Cluster _cluster;
            private readonly ClusterDescription _description;
            private readonly Task _descriptionChangedTask;
            private readonly IServerSelector _selector;
            private readonly CancellationTokenSource _timeoutCancellationTokenSource;
            private readonly Task _timeoutTask;

            public  WaitForDescriptionChangedHelper(Cluster cluster, IServerSelector selector, ClusterDescription description, Task descriptionChangedTask , TimeSpan timeout, CancellationToken cancellationToken)
            {
                _cluster = cluster;
                _description = description;
                _selector = selector;
                _descriptionChangedTask = descriptionChangedTask;
                _cancellationToken = cancellationToken;
                _cancellationTaskCompletionSource = new TaskCompletionSource<bool>();
                _cancellationTokenRegistration = cancellationToken.Register(() => _cancellationTaskCompletionSource.TrySetCanceled());
                _timeoutCancellationTokenSource = new CancellationTokenSource();
                _timeoutTask = Task.Delay(timeout, _timeoutCancellationTokenSource.Token);
            }

            public Task[] Tasks
            {
                get
                {
                    return new Task[]
                    {
                        _descriptionChangedTask,
                        _timeoutTask,
                        _cancellationTaskCompletionSource.Task
                    };
                }
            }

            public void Dispose()
            {
                _cancellationTokenRegistration.Dispose();
                _timeoutCancellationTokenSource.Dispose();
            }

            public void HandleCompletedTask(Task completedTask)
            {
                if (completedTask == _timeoutTask)
                {
                    _cluster.ThrowTimeoutException(_selector, _description);
                }
                _timeoutCancellationTokenSource.Cancel();

                if (completedTask == _cancellationTaskCompletionSource.Task)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                }

                _descriptionChangedTask.GetAwaiter().GetResult(); // propagate exceptions
            }
        }

        private static class State
        {
            public const int Initial = 0;
            public const int Open = 1;
            public const int Disposed = 2;
        }
    }
}
