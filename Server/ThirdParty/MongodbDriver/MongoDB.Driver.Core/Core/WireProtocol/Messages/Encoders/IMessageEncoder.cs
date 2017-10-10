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
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders
{
    /// <summary>
    /// Represents a message encoder.
    /// </summary>
    public interface IMessageEncoder
    {
        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        MongoDBMessage ReadMessage();

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        void WriteMessage(MongoDBMessage message);
    }
}
