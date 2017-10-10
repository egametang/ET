/* Copyright 2013-2014 MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Servers
{
    internal sealed class HeartbeatDelay : IDisposable
    {
        // fields
        private readonly DateTime _earlyHeartbeatAt;
        private int _earlyHeartbeatHasBeenRequested;
        private readonly TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();
        private readonly Timer _timer;

        // constructors
        public HeartbeatDelay(TimeSpan heartbeatInterval, TimeSpan minHeartbeatInterval)
        {
            if (minHeartbeatInterval > heartbeatInterval) { minHeartbeatInterval = heartbeatInterval; }
            _timer = new Timer(TimerCallback, null, heartbeatInterval, Timeout.InfiniteTimeSpan);
            _earlyHeartbeatAt = DateTime.UtcNow + minHeartbeatInterval;
        }

        // properties
        public Task Task
        {
            get { return _taskCompletionSource.Task; }
        }

        // methods
        public void Dispose()
        {
            _timer.Dispose();
        }

        public void RequestHeartbeat()
        {
            if (Interlocked.CompareExchange(ref _earlyHeartbeatHasBeenRequested, 1, 0) == 0)
            {
                var earlyHeartbeatDelay = _earlyHeartbeatAt - DateTime.UtcNow;
                if (earlyHeartbeatDelay <= TimeSpan.Zero)
                {
                    _timer.Dispose();
                    _taskCompletionSource.TrySetResult(true);
                }
                else
                {
                    _timer.Change(earlyHeartbeatDelay, Timeout.InfiniteTimeSpan);
                }
            }
        }

        private void TimerCallback(object state)
        {
            _taskCompletionSource.TrySetResult(true);
        }
    }
}
