/* Copyright 2010-2015 MongoDB Inc.
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
using System.Text;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace MongoDB.Driver
{
    /// <summary>
    /// Default values for various Mongo settings.
    /// </summary>
    public static class MongoDefaults
    {
        // private static fields
        private static bool __assignIdOnInsert = true;
        private static string __authenticationMechanism = null;
        private static TimeSpan __connectTimeout = TimeSpan.FromSeconds(30);
        private static TimeSpan __localThreshold = TimeSpan.FromMilliseconds(15);
        private static TimeSpan __maxConnectionIdleTime = TimeSpan.FromMinutes(10);
        private static TimeSpan __maxConnectionLifeTime = TimeSpan.FromMinutes(30);
        private static int __maxBatchCount = 1000;
        private static int __maxConnectionPoolSize = 100;
        private static int __maxMessageLength = 16000000; // 16MB (not 16 MiB!)
        private static int __minConnectionPoolSize = 0;
        private static TimeSpan __operationTimeout = TimeSpan.FromSeconds(30);
        private static UTF8Encoding __readEncoding = Utf8Encodings.Strict;
        private static TimeSpan __serverSelectionTimeout = TimeSpan.FromSeconds(30);
        private static TimeSpan __socketTimeout = TimeSpan.Zero; // use operating system default (presumably infinite)
        private static int __tcpReceiveBufferSize = 64 * 1024; // 64KiB (note: larger than 2MiB fails on Mac using Mono)
        private static int __tcpSendBufferSize = 64 * 1024; // 64KiB (TODO: what is the optimum value for the buffers?)
        private static double __waitQueueMultiple = 5.0; // default wait queue multiple is 5.0
        private static int __waitQueueSize = 0; // use multiple by default
        private static TimeSpan __waitQueueTimeout = TimeSpan.FromMinutes(2); // default wait queue timeout is 2 minutes
        private static UTF8Encoding __writeEncoding = Utf8Encodings.Strict;
        private static int __maxDocumentSize = 4 * 1024 * 1024; // 4 MiB. Original MongoDB max document size

        // public static properties
        /// <summary>
        /// Gets or sets whether the driver should assign a value to empty Ids on Insert.
        /// </summary>
        public static bool AssignIdOnInsert
        {
            get { return __assignIdOnInsert; }
            set { __assignIdOnInsert = value; }
        }

        /// <summary>
        /// Gets or sets the default authentication mechanism.
        /// </summary>
        public static string AuthenticationMechanism
        {
            get { return __authenticationMechanism; }
            set { __authenticationMechanism = value; }
        }

        /// <summary>
        /// Gets the actual wait queue size (either WaitQueueSize or WaitQueueMultiple x MaxConnectionPoolSize).
        /// </summary>
        public static int ComputedWaitQueueSize
        {
            get
            {
                if (__waitQueueMultiple == 0.0)
                {
                    return __waitQueueSize;
                }
                else
                {
                    return (int)(__waitQueueMultiple * __maxConnectionPoolSize);
                }
            }
        }

        /// <summary>
        /// Gets or sets the connect timeout.
        /// </summary>
        public static TimeSpan ConnectTimeout
        {
            get { return __connectTimeout; }
            set { __connectTimeout = value; }
        }

        /// <summary>
        /// Gets or sets the representation to use for Guids (this is an alias for BsonDefaults.GuidRepresentation).
        /// </summary>
        public static GuidRepresentation GuidRepresentation
        {
            get { return BsonDefaults.GuidRepresentation; }
            set { BsonDefaults.GuidRepresentation = value; }
        }

        /// <summary>
        /// Gets or sets the default local threshold.
        /// </summary>
        public static TimeSpan LocalThreshold
        {
            get { return __localThreshold; }
            set { __localThreshold = value; }
        }

        /// <summary>
        /// Gets or sets the maximum batch count.
        /// </summary>
        public static int MaxBatchCount
        {
            get { return __maxBatchCount; }
            set { __maxBatchCount = value; }
        }

        /// <summary>
        /// Gets or sets the max connection idle time.
        /// </summary>
        public static TimeSpan MaxConnectionIdleTime
        {
            get { return __maxConnectionIdleTime; }
            set { __maxConnectionIdleTime = value; }
        }

        /// <summary>
        /// Gets or sets the max connection life time.
        /// </summary>
        public static TimeSpan MaxConnectionLifeTime
        {
            get { return __maxConnectionLifeTime; }
            set { __maxConnectionLifeTime = value; }
        }

        /// <summary>
        /// Gets or sets the max connection pool size.
        /// </summary>
        public static int MaxConnectionPoolSize
        {
            get { return __maxConnectionPoolSize; }
            set { __maxConnectionPoolSize = value; }
        }

        /// <summary>
        /// Gets or sets the max document size
        /// </summary>
        public static int MaxDocumentSize
        {
            get { return __maxDocumentSize; }
            set { __maxDocumentSize = value; }
        }

        /// <summary>
        /// Gets or sets the max message length.
        /// </summary>
        public static int MaxMessageLength
        {
            get { return __maxMessageLength; }
            set { __maxMessageLength = value; }
        }

        /// <summary>
        /// Gets or sets the min connection pool size.
        /// </summary>
        public static int MinConnectionPoolSize
        {
            get { return __minConnectionPoolSize; }
            set { __minConnectionPoolSize = value; }
        }

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        public static TimeSpan OperationTimeout
        {
            get { return __operationTimeout; }
            set { __operationTimeout = value; }
        }

        /// <summary>
        /// Gets or sets the Read Encoding.
        /// </summary>
        public static UTF8Encoding ReadEncoding
        {
            get { return __readEncoding; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                __readEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets the server selection timeout.
        /// </summary>
        public static TimeSpan ServerSelectionTimeout
        {
            get { return __serverSelectionTimeout; }
            set { __serverSelectionTimeout = value; }
        }

        /// <summary>
        /// Gets or sets the socket timeout.
        /// </summary>
        public static TimeSpan SocketTimeout
        {
            get { return __socketTimeout; }
            set { __socketTimeout = value; }
        }

        /// <summary>
        /// Gets or sets the TCP receive buffer size.
        /// </summary>
        public static int TcpReceiveBufferSize
        {
            get { return __tcpReceiveBufferSize; }
            set { __tcpReceiveBufferSize = value; }
        }

        /// <summary>
        /// Gets or sets the TCP send buffer size.
        /// </summary>
        public static int TcpSendBufferSize
        {
            get { return __tcpSendBufferSize; }
            set { __tcpSendBufferSize = value; }
        }

        /// <summary>
        /// Gets or sets the wait queue multiple (the actual wait queue size will be WaitQueueMultiple x MaxConnectionPoolSize, see also WaitQueueSize).
        /// </summary>
        public static double WaitQueueMultiple
        {
            get { return __waitQueueMultiple; }
            set
            {
                __waitQueueMultiple = value;
                __waitQueueSize = 0;
            }
        }

        /// <summary>
        /// Gets or sets the wait queue size (see also WaitQueueMultiple).
        /// </summary>
        public static int WaitQueueSize
        {
            get { return __waitQueueSize; }
            set
            {
                __waitQueueMultiple = 0.0;
                __waitQueueSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait queue timeout.
        /// </summary>
        public static TimeSpan WaitQueueTimeout
        {
            get { return __waitQueueTimeout; }
            set { __waitQueueTimeout = value; }
        }

        /// <summary>
        /// Gets or sets the Write Encoding.
        /// </summary>
        public static UTF8Encoding WriteEncoding
        {
            get { return __writeEncoding; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                __writeEncoding = value;
            }
        }
    }
}