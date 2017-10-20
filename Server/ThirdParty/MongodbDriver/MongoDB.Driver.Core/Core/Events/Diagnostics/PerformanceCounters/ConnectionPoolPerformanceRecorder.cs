/* Copyright 2013-2016 MongoDB Inc.
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

#if NET45
namespace MongoDB.Driver.Core.Events.Diagnostics.PerformanceCounters
{
    internal class ConnectionPoolPerformanceRecorder
    {
        // private fields
        private readonly PerformanceCounterPackage _appPackage;
        private readonly int _maxSize;
        private readonly PerformanceCounterPackage _serverPackage;

        // constructors
        public ConnectionPoolPerformanceRecorder(int maxSize, PerformanceCounterPackage appPackage, PerformanceCounterPackage serverPackage)
        {
            _maxSize = maxSize;
            _appPackage = appPackage;
            _serverPackage = serverPackage;
        }

        // public methods
        public void Opened()
        {
            _appPackage.CurrentNumberOfOpenConnectionPools.Increment();
            _serverPackage.CurrentNumberOfActiveConnections.Increment();

            _appPackage.ConnectionPoolUtilizationPercentageBase.IncrementBy(_maxSize);
            _serverPackage.ConnectionPoolUtilizationPercentageBase.IncrementBy(_maxSize);
        }

        public void Closed()
        {
            _appPackage.CurrentNumberOfOpenConnectionPools.Decrement();
            _serverPackage.CurrentNumberOfOpenConnectionPools.Decrement();

            _appPackage.ConnectionPoolUtilizationPercentageBase.IncrementBy(-_maxSize);
            _serverPackage.ConnectionPoolUtilizationPercentageBase.IncrementBy(-_maxSize);
        }

        public void WaitQueueEntered()
        {
            _appPackage.CurrentWaitQueueSize.Increment();
            _serverPackage.CurrentWaitQueueSize.Increment();
        }

        public void WaitQueueExited()
        {
            _appPackage.CurrentWaitQueueSize.Decrement();
            _serverPackage.CurrentWaitQueueSize.Decrement();
        }

        public void ConnectionAdded()
        {
            _appPackage.CurrentNumberOfAvailableConnections.Increment();
            _serverPackage.CurrentNumberOfAvailableConnections.Increment();

            _appPackage.ConnectionPoolUtilizationPercentage.Increment();
            _serverPackage.ConnectionPoolUtilizationPercentage.Increment();
        }

        public void ConnectionCheckedIn()
        {
            _appPackage.CurrentNumberOfAvailableConnections.Increment();
            _serverPackage.CurrentNumberOfAvailableConnections.Increment();
            _appPackage.CurrentNumberOfActiveConnections.Decrement();
            _serverPackage.CurrentNumberOfActiveConnections.Decrement();

            _appPackage.NumberOfConnectionsCheckedInPerSecond.Increment();
            _serverPackage.NumberOfConnectionsCheckedInPerSecond.Increment();
        }

        public void ConnectionCheckedOut()
        {
            _appPackage.CurrentNumberOfAvailableConnections.Decrement();
            _serverPackage.CurrentNumberOfAvailableConnections.Decrement();
            _appPackage.CurrentNumberOfActiveConnections.Increment();
            _serverPackage.CurrentNumberOfActiveConnections.Increment();

            _appPackage.NumberOfConnectionsCheckedOutPerSecond.Increment();
            _serverPackage.NumberOfConnectionsCheckedOutPerSecond.Increment();
        }

        public void ConnectionRemoved()
        {
            _appPackage.CurrentNumberOfAvailableConnections.Decrement();
            _serverPackage.CurrentNumberOfAvailableConnections.Decrement();

            _appPackage.ConnectionPoolUtilizationPercentage.Decrement();
            _serverPackage.ConnectionPoolUtilizationPercentage.Decrement();
        }
    }
}
#endif
