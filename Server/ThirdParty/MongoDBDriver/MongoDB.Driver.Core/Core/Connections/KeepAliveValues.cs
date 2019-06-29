/* Copyright 2018-present MongoDB Inc.
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

namespace MongoDB.Driver.Core.Connections
{
    internal struct KeepAliveValues
    {
        public ulong OnOff { get; set; }
        public ulong KeepAliveTime { get; set; }
        public ulong KeepAliveInterval { get; set; }

        public byte[] ToBytes()
        {
            // set the tcp_keepalive struct at the following page for documentation of the buffer layout
            // https://msdn.microsoft.com/en-us/library/windows/desktop/dd877220(v=vs.85).aspx
            var bytes = new byte[24];
            Array.Copy(BitConverter.GetBytes(OnOff), 0, bytes, 0, 8);
            Array.Copy(BitConverter.GetBytes(KeepAliveTime), 0, bytes, 8, 8);
            Array.Copy(BitConverter.GetBytes(KeepAliveInterval), 0, bytes, 16, 8);
            return bytes;
        }
    }
}
