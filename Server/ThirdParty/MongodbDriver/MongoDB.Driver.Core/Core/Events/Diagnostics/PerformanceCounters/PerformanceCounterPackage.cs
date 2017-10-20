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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MongoDB.Driver.Core.Events.Diagnostics.PerformanceCounters
{
    internal sealed class PerformanceCounterPackage : IDisposable
    {
        // private constants
        private const string CategoryName = ".NET Driver for MongoDB";

        // private fields
        private volatile bool _disposed;

        // constructors
        public PerformanceCounterPackage(string instanceName)
        {
            Initialize(instanceName);
        }

        // public properties
        [PerformanceCounter(
            "Average Connection LifeTime (sec)",
            "The average lifetime of a connection in seconds.",
            PerformanceCounterType.RawFraction)]
        public PerformanceCounter AverageConnectionLifeTime { get; private set; }

        [PerformanceCounter(
            "Average Connection LifeTime (sec) Base",
            "The average lifetime of a connection in seconds base.",
            PerformanceCounterType.RawBase)]
        public PerformanceCounter AverageConnectionLifeTimeBase { get; private set; }

        [PerformanceCounter(
            "Average Size Of Received Messages In Bytes",
            "The average size of a received message in bytes.",
            PerformanceCounterType.RawFraction)]
        public PerformanceCounter AverageSizeOfReceivedMessagesInBytes { get; private set; }

        [PerformanceCounter(
            "Average Size Of Received Messages In Bytes Base",
            "The average size of a received message in bytes base.",
            PerformanceCounterType.RawBase)]
        public PerformanceCounter AverageSizeOfReceivedMessagesInBytesBase { get; private set; }

        [PerformanceCounter(
            "Average Size Of Sent Messages In Bytes",
            "The average size of a sent message in bytes.",
            PerformanceCounterType.RawFraction)]
        public PerformanceCounter AverageSizeOfSentMessagesInBytes { get; private set; }

        [PerformanceCounter(
            "Average Size Of Sent Messages In Bytes Base",
            "The average size of a sent message in bytes base.",
            PerformanceCounterType.RawBase)]
        public PerformanceCounter AverageSizeOfSentMessagesInBytesBase { get; private set; }

        [PerformanceCounter(
            "Connection Pool Utilization Percentage",
            "The utilization percentage of the connection pool.",
            PerformanceCounterType.RawFraction)]
        public PerformanceCounter ConnectionPoolUtilizationPercentage { get; private set; }

        [PerformanceCounter(
            "Connection Pool Utilization Percentage Base",
            "The utilization percentagle of the connection pool base.",
            PerformanceCounterType.RawBase)]
        public PerformanceCounter ConnectionPoolUtilizationPercentageBase { get; private set; }

        [PerformanceCounter(
            "Current Number Of Active Connections",
            "The current number of connections in use.",
            PerformanceCounterType.NumberOfItems32)]
        public PerformanceCounter CurrentNumberOfActiveConnections { get; private set; }

        [PerformanceCounter(
            "Current Number Of Available Connections",
            "The current number of open and available connections.",
            PerformanceCounterType.NumberOfItems32)]
        public PerformanceCounter CurrentNumberOfAvailableConnections { get; private set; }

        [PerformanceCounter(
            "Current Number Of Open Connection Pools.",
            "The current number of open connection pools.",
            PerformanceCounterType.NumberOfItems32)]
        public PerformanceCounter CurrentNumberOfOpenConnectionPools { get; private set; }

        [PerformanceCounter(
            "Current Number Of Open Connections",
            "The current number of open connections.",
            PerformanceCounterType.NumberOfItems32)]
        public PerformanceCounter CurrentNumberOfOpenConnections { get; private set; }

        [PerformanceCounter(
            "Current Wait Queue Size",
            "The current number of consumers waiting for a connection.",
            PerformanceCounterType.NumberOfItems32)]
        public PerformanceCounter CurrentWaitQueueSize { get; private set; }

        [PerformanceCounter(
            "Number of Connections Checked In / sec",
            "The number of connections checked in of a connection pool per second.",
            PerformanceCounterType.RateOfCountsPerSecond32)]
        public PerformanceCounter NumberOfConnectionsCheckedInPerSecond { get; private set; }

        [PerformanceCounter(
            "Number of Connections Checked Out / sec",
            "The number of connections checked out of a connection pool per second.",
            PerformanceCounterType.RateOfCountsPerSecond32)]
        public PerformanceCounter NumberOfConnectionsCheckedOutPerSecond { get; private set; }

        [PerformanceCounter(
            "Number Of Bytes Received / sec",
            "The number of bytes received per second.",
            PerformanceCounterType.RateOfCountsPerSecond32)]
        public PerformanceCounter NumberOfBytesReceivedPerSecond { get; private set; }

        [PerformanceCounter(
            "Number Of Bytes Sent / sec",
            "The number of bytes sent per second.",
            PerformanceCounterType.RateOfCountsPerSecond32)]
        public PerformanceCounter NumberOfBytesSentPerSecond { get; private set; }

        [PerformanceCounter(
            "Number Of Messages Received / sec",
            "The number of messages received per second.",
            PerformanceCounterType.RateOfCountsPerSecond32)]
        public PerformanceCounter NumberOfMessagesReceivedPerSecond { get; private set; }

        [PerformanceCounter(
            "Number Of Messages Sent / sec",
            "The number of messages sent per second.",
            PerformanceCounterType.RateOfCountsPerSecond32)]
        public PerformanceCounter NumberOfMessagesSentPerSecond { get; private set; }

        // public static methods
        public static void Install()
        {
            if (PerformanceCounterCategory.Exists(CategoryName))
            {
                return;
            }

            var properties = GetCounterProperties();
            var collection = new CounterCreationDataCollection();
            foreach (var property in properties)
            {
                var attribute = GetCounterAttribute(property);
                collection.Add(new CounterCreationData(attribute.Name, attribute.Help, attribute.Type));
            }

            PerformanceCounterCategory.Create(
                CategoryName,
                "Stats for the .NET MongoDB Driver.",
                PerformanceCounterCategoryType.MultiInstance,
                collection);
        }

        // private static methods
        private static PerformanceCounterAttribute GetCounterAttribute(PropertyInfo info)
        {
            return info.GetCustomAttributes(typeof(PerformanceCounterAttribute), false)
                .OfType<PerformanceCounterAttribute>()
                .Single();
        }

        private static IEnumerable<PropertyInfo> GetCounterProperties()
        {
            return typeof(PerformanceCounterPackage)
                .GetProperties()
                .Where(x => x.PropertyType == typeof(PerformanceCounter));
        }

        // public methods
        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var property in GetCounterProperties())
                {
                    var counter = (PerformanceCounter)property.GetValue(this, null);
                    try
                    {
                        counter.RemoveInstance();
                    }
                    catch (NotImplementedException)
                    {
                        // apparently this happens on Mono...
                    }
                    counter.Dispose();
                    property.SetValue(this, null, null);
                }
            }

            _disposed = true;
        }

        private void Initialize(string instanceName)
        {
            foreach (var property in GetCounterProperties())
            {
                var attribute = GetCounterAttribute(property);

                var counter = new PerformanceCounter(
                    CategoryName,
                    attribute.Name,
                    instanceName,
                    false);

                counter.RawValue = 0; // initialize it to it's default value...

                property.SetValue(this, counter, null);
            }
        }
    }
}
#endif
