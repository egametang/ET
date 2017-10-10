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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Events.Diagnostics
{
    internal static class TraceSourceEventHelper
    {
        public const int CommandIdBase = 100;
        public const int ConnectionIdBase = 200;
        public const int ConnectionPoolIdBase = 300;
        public const int ServerIdBase = 400;
        public const int ClusterIdBase = 500;

        public static string Label(ConnectionId id)
        {
            return string.Format("connection[{0}:{1}:{2}]", id.ServerId.ClusterId.Value.ToString(), Format(id.ServerId.EndPoint), Format(id));
        }

        public static string Label(ServerId serverId)
        {
            return string.Format("server[{0}:{1}]", serverId.ClusterId.Value.ToString(), Format(serverId.EndPoint));
        }

        public static string Label(ClusterId clusterId)
        {
            return string.Format("cluster[{0}]", clusterId.Value.ToString());
        }

        public static string Format(ConnectionId id)
        {
            if (id.ServerValue.HasValue)
            {
                return id.LocalValue.ToString() + "-" + id.ServerValue.Value.ToString();
            }
            return id.LocalValue.ToString();
        }

        public static string Format(ServerId serverId)
        {
            return Format(serverId.EndPoint);
        }

        public static string Format(EndPoint endPoint)
        {
            var dnsEndPoint = endPoint as DnsEndPoint;
            if (dnsEndPoint != null)
            {
                return string.Concat(
                    dnsEndPoint.Host,
                    ":",
                    dnsEndPoint.Port.ToString());
            }

            return endPoint.ToString();
        }
    }
}
