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
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for IPEndPoints.
    /// </summary>
    public class IPEndPointSerializer : ClassSerializerBase<IPEndPoint>
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the IPEndPointSerializer class.
        /// </summary>
        public IPEndPointSerializer()
        {
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override IPEndPoint DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            EnsureBsonTypeEquals(bsonReader, BsonType.String);

            var stringValue = bsonReader.ReadString();
            var match = Regex.Match(stringValue, @"^(?<address>(.+|\[.*\]))\:(?<port>\d+)$");
            if (match.Success)
            {
                IPAddress address;
                if (IPAddress.TryParse(match.Groups["address"].Value, out address))
                {
                    int port;
                    if (int.TryParse(match.Groups["port"].Value, out port))
                    {
                        return new IPEndPoint(address, port);
                    }
                }
            }

            var message = string.Format("Invalid IPEndPoint value '{0}'.", stringValue);
            throw new FormatException(message);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, IPEndPoint value)
        {
            var bsonWriter = context.Writer;

            string stringValue;
            if (value.AddressFamily == AddressFamily.InterNetwork)
            {
                stringValue = string.Format("{0}:{1}", value.Address, value.Port); // IPv4
            }
            else
            {
                stringValue = string.Format("[{0}]:{1}", value.Address, value.Port); // IPv6
            }
            bsonWriter.WriteString(stringValue);
        }
    }
}
