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
using System.Diagnostics;

namespace MongoDB.Driver.Core.Events.Diagnostics.PerformanceCounters
{
    internal class ConnectionPerformanceRecorder
    {
        // private fields
        private readonly PerformanceCounterPackage _appPackage;
        private readonly PerformanceCounterPackage _serverPackage;
        private Stopwatch _stopwatch;

        // constructors
        public ConnectionPerformanceRecorder(PerformanceCounterPackage appPackage, PerformanceCounterPackage serverPackage)
        {
            _appPackage = appPackage;
            _serverPackage = serverPackage;
        }

        // public methods
        public void Opened()
        {
            _stopwatch = Stopwatch.StartNew();

            _appPackage.CurrentNumberOfOpenConnections.Increment();
            _serverPackage.CurrentNumberOfOpenConnections.Increment();
        }

        public void Closed()
        {
            _stopwatch.Stop();

            var elapsed = _stopwatch.Elapsed.Seconds;
            _appPackage.AverageConnectionLifeTime.IncrementBy(elapsed);
            _appPackage.AverageConnectionLifeTimeBase.IncrementBy(100);

            _serverPackage.AverageConnectionLifeTime.IncrementBy(elapsed);
            _serverPackage.AverageConnectionLifeTimeBase.IncrementBy(100);

            _appPackage.CurrentNumberOfOpenConnections.Decrement();
            _serverPackage.CurrentNumberOfOpenConnections.Decrement();
        }

        public void MessageReceived(int requestId, int sizeInBytes)
        {
            _appPackage.NumberOfMessagesReceivedPerSecond.Increment();
            _serverPackage.NumberOfMessagesReceivedPerSecond.Increment();

            _appPackage.AverageSizeOfReceivedMessagesInBytes.IncrementBy(sizeInBytes);
            _appPackage.AverageSizeOfReceivedMessagesInBytesBase.IncrementBy(100);
            _serverPackage.AverageSizeOfReceivedMessagesInBytes.IncrementBy(sizeInBytes);
            _serverPackage.AverageSizeOfReceivedMessagesInBytesBase.IncrementBy(100);

            _appPackage.NumberOfBytesReceivedPerSecond.IncrementBy(sizeInBytes);
            _serverPackage.NumberOfBytesReceivedPerSecond.IncrementBy(sizeInBytes);
        }

        public void PacketSent(int numMessages, int sizeInBytes)
        {
            _appPackage.NumberOfMessagesSentPerSecond.IncrementBy(numMessages);
            _serverPackage.NumberOfMessagesSentPerSecond.IncrementBy(numMessages);

            _appPackage.AverageSizeOfSentMessagesInBytes.IncrementBy(sizeInBytes / 2);
            _appPackage.AverageSizeOfSentMessagesInBytesBase.IncrementBy(100);
            _serverPackage.AverageSizeOfSentMessagesInBytes.IncrementBy(sizeInBytes / 2);
            _serverPackage.AverageSizeOfSentMessagesInBytesBase.IncrementBy(100);

            _appPackage.NumberOfBytesSentPerSecond.IncrementBy(sizeInBytes);
            _serverPackage.NumberOfBytesSentPerSecond.IncrementBy(sizeInBytes);
        }
    }
}
#endif
