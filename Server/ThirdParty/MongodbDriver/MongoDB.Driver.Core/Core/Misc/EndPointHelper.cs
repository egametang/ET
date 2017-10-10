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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// Represents helper methods for EndPoints.
    /// </summary>
    public static class EndPointHelper
    {
        // static fields
        private static IEqualityComparer<EndPoint> __endPointEqualityComparer = new EndPointEqualityComparerImpl();

        // static properties
        /// <summary>
        /// Gets an end point equality comparer.
        /// </summary>
        /// <value>
        /// An end point equality comparer.
        /// </value>
        public static IEqualityComparer<EndPoint> EndPointEqualityComparer
        {
            get { return __endPointEqualityComparer; }
        }

        // static methods
        /// <summary>
        /// Determines whether a list of end points contains a specific end point.
        /// </summary>
        /// <param name="endPoints">The list of end points.</param>
        /// <param name="endPoint">The specific end point to search for.</param>
        /// <returns>True if the list of end points contains the specific end point.</returns>
        public static bool Contains(IEnumerable<EndPoint> endPoints, EndPoint endPoint)
        {
            return endPoints.Contains(endPoint, __endPointEqualityComparer);
        }

        /// <summary>
        /// Compares two end points.
        /// </summary>
        /// <param name="a">The first end point.</param>
        /// <param name="b">The second end point.</param>
        /// <returns>True if both end points are equal, or if both are null.</returns>
        public static bool Equals(EndPoint a, EndPoint b)
        {
            return __endPointEqualityComparer.Equals(a, b);
        }

        /// <summary>
        /// Creates an end point from object data saved during serialization.
        /// </summary>
        /// <param name="info">The object data.</param>
        /// <returns>An end point.</returns>
        public static EndPoint FromObjectData(List<object> info)
        {
            if (info == null)
            {
                return null;
            }

            var type = (string)info[0];
            switch (type)
            {
                case "DnsEndPoint": return new DnsEndPoint((string)info[1], (int)info[2], (AddressFamily)(int)info[3]);
                case "IPEndPoint": return new IPEndPoint((IPAddress)info[1], (int)info[2]);
                default: throw new MongoInternalException("Unexpected EndPoint type.");
            }
        }

#if NET45
        /// <summary>
        /// Gets the object data required to serialize an end point.
        /// </summary>
        /// <param name="value">The end point.</param>
        /// <returns>The object data.</returns>
        public static List<object> GetObjectData(EndPoint value)
        {
            var dnsEndPoint = value as DnsEndPoint;
            if (dnsEndPoint != null)
            {
                return new List<object>
                {
                    "DnsEndPoint",
                    dnsEndPoint.Host,
                    dnsEndPoint.Port,
                    (int)dnsEndPoint.AddressFamily
                };
            }

            var ipEndPoint = value as IPEndPoint;
            if (ipEndPoint != null)
            {
                return new List<object>
                {
                    "IPEndPoint",
                    ipEndPoint.Address,
                    ipEndPoint.Port
                };
            }

            return null;
        }
#endif

        /// <summary>
        /// Compares two sequences of end points.
        /// </summary>
        /// <param name="a">The first sequence of end points.</param>
        /// <param name="b">The second sequence of end points.</param>
        /// <returns>True if both sequences contain the same end points in the same order, or if both sequences are null.</returns>
        public static bool SequenceEquals(IEnumerable<EndPoint> a, IEnumerable<EndPoint> b)
        {
            return a.SequenceEqual(b, __endPointEqualityComparer);
        }

        /// <summary>
        /// Parses the string representation of an end point.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>An end point.</returns>
        public static EndPoint Parse(string value)
        {
            Ensure.IsNotNull(value, nameof(value));

            EndPoint endPoint;
            if (!TryParse(value, out endPoint))
            {
                var message = string.Format("'{0}' is not a valid end point.", value);
                throw new ArgumentException(message, "value");
            }

            return endPoint;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents the end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents the end point.
        /// </returns>
        public static string ToString(EndPoint endPoint)
        {
            var dnsEndPoint = endPoint as DnsEndPoint;
            if (dnsEndPoint != null)
            {
                return string.Format("{0}:{1}", dnsEndPoint.Host, dnsEndPoint.Port);
            }

            return endPoint.ToString();
        }

        /// <summary>
        /// Tries to parse the string representation of an end point.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if the string representation was parsed successfully.</returns>
        public static bool TryParse(string value, out EndPoint result)
        {
            result = null;

            if (value != null)
            {
                value = value.ToLowerInvariant();
                var match = Regex.Match(value, @"^(?<address>\[[^]]+\])(:(?<port>\d+))?$");
                if (match.Success)
                {
                    var addressString = match.Groups["address"].Value;
                    var portString = match.Groups["port"].Value;
                    var port = 27017;
                    if (portString.Length != 0 && !int.TryParse(portString, out port))
                    {
                        return false;
                    }

                    if (!IsValidPort(port))
                    {
                        return false;
                    }

                    IPAddress address;
                    if (IPAddress.TryParse(addressString, out address))
                    {
                        result = new IPEndPoint(address, port);
                        return true;
                    }

                    return false;
                }

                match = Regex.Match(value, @"^(?<host>[^:]+)(:(?<port>\d+))?$");
                if (match.Success)
                {
                    var host = match.Groups["host"].Value;
                    var portString = match.Groups["port"].Value;
                    var port = 27017;
                    if (portString.Length != 0 && !int.TryParse(portString, out port))
                    {
                        return false;
                    }

                    if (!IsValidPort(port))
                    {
                        return false;
                    }

                    IPAddress address;
                    if (IPAddress.TryParse(host, out address))
                    {
                        result = new IPEndPoint(address, port);
                        return true;
                    }

                    try
                    {
                        result = new DnsEndPoint(host, port);
                        return true;
                    }
                    catch (ArgumentException)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static bool IsValidPort(int port)
        {
            return port > 0 && port <= ushort.MaxValue;
        }

        // nested classes
        private class EndPointEqualityComparerImpl : IEqualityComparer<EndPoint>
        {
            public bool Equals(EndPoint x, EndPoint y)
            {
                if (x == null && y == null)
                {
                    return true;
                }
                else if (x == null || y == null)
                {
                    return false;
                }

                // mono has a bug in DnsEndPoint.Equals, so if the types aren't
                // equal, it will throw a null reference exception.
                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Equals(y);
            }

            public int GetHashCode(EndPoint obj)
            {
                return obj.GetHashCode();
            }
        }

    }
}
