/* Copyright 2010-2016 MongoDB Inc.
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
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using MongoDB.Bson.IO;

namespace MongoDB.Driver
{
    /// <summary>
    /// The address of a MongoDB server.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoServerAddress : IEquatable<MongoServerAddress>
    {
        // private fields
        private string _host;
        private int _port;

        // constructors
        /// <summary>
        /// Initializes a new instance of MongoServerAddress.
        /// </summary>
        /// <param name="host">The server's host name.</param>
        public MongoServerAddress(string host)
        {
            _host = host;
            _port = 27017;
        }

        /// <summary>
        /// Initializes a new instance of MongoServerAddress.
        /// </summary>
        /// <param name="host">The server's host name.</param>
        /// <param name="port">The server's port number.</param>
        public MongoServerAddress(string host, int port)
        {
            _host = host;
            _port = port;
        }

        // factory methods
        /// <summary>
        /// Parses a string representation of a server address.
        /// </summary>
        /// <param name="value">The string representation of a server address.</param>
        /// <returns>A new instance of MongoServerAddress initialized with values parsed from the string.</returns>
        public static MongoServerAddress Parse(string value)
        {
            MongoServerAddress address;
            if (TryParse(value, out address))
            {
                return address;
            }
            else
            {
                var message = string.Format("'{0}' is not a valid server address.", value);
                throw new FormatException(message);
            }
        }

        /// <summary>
        /// Tries to parse a string representation of a server address.
        /// </summary>
        /// <param name="value">The string representation of a server address.</param>
        /// <param name="address">The server address (set to null if TryParse fails).</param>
        /// <returns>True if the string is parsed succesfully.</returns>
        public static bool TryParse(string value, out MongoServerAddress address)
        {
            // don't throw ArgumentNullException if value is null
            if (value != null)
            {
                Match match = Regex.Match(value, @"^(?<host>(\[[^]]+\]|[^:\[\]]+))(:(?<port>\d+))?$");
                if (match.Success)
                {
                    string host = match.Groups["host"].Value;
                    string portString = match.Groups["port"].Value;
                    int port = (portString == "") ? 27017 : JsonConvert.ToInt32(portString);
                    address = new MongoServerAddress(host, port);
                    return true;
                }
            }

            address = null;
            return false;
        }

        // public properties
        /// <summary>
        /// Gets the server's host name.
        /// </summary>
        public string Host
        {
            get { return _host; }
        }

        /// <summary>
        /// Gets the server's port number.
        /// </summary>
        public int Port
        {
            get { return _port; }
        }

        // public operators
        /// <summary>
        /// Compares two server addresses.
        /// </summary>
        /// <param name="lhs">The first address.</param>
        /// <param name="rhs">The other address.</param>
        /// <returns>True if the two addresses are equal (or both are null).</returns>
        public static bool operator ==(MongoServerAddress lhs, MongoServerAddress rhs)
        {
            return object.Equals(lhs, rhs);
        }

        /// <summary>
        /// Compares two server addresses.
        /// </summary>
        /// <param name="lhs">The first address.</param>
        /// <param name="rhs">The other address.</param>
        /// <returns>True if the two addresses are not equal (or one is null and the other is not).</returns>
        public static bool operator !=(MongoServerAddress lhs, MongoServerAddress rhs)
        {
            return !(lhs == rhs);
        }

        // public methods
        /// <summary>
        /// Compares two server addresses.
        /// </summary>
        /// <param name="rhs">The other server address.</param>
        /// <returns>True if the two server addresses are equal.</returns>
        public bool Equals(MongoServerAddress rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            return _host.Equals(rhs._host, StringComparison.OrdinalIgnoreCase) && _port == rhs._port;
        }

        /// <summary>
        /// Compares two server addresses.
        /// </summary>
        /// <param name="obj">The other server address.</param>
        /// <returns>True if the two server addresses are equal.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MongoServerAddress); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + _host.ToLowerInvariant().GetHashCode();
            hash = 37 * hash + _port.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns a string representation of the server address.
        /// </summary>
        /// <returns>A string representation of the server address.</returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", _host, _port);
        }
    }
}
