/* Copyright 2015-present MongoDB Inc.
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

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.GridFS
{
    internal static class StreamExtensions
    {
        public static void ReadBytes(this Stream stream, byte[] destination, int offset, int count, CancellationToken cancellationToken)
        {
            while (count > 0)
            {
                var bytesRead = stream.Read(destination, offset, count); // TODO: honor cancellationToken?
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException();
                }
                offset += bytesRead;
                count -= bytesRead;
            }
        }

        public static async Task ReadBytesAsync(this Stream stream, byte[] destination, int offset, int count, CancellationToken cancellationToken)
        {
            while (count > 0)
            {
                var bytesRead = await stream.ReadAsync(destination, offset, count, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException();
                }
                offset += bytesRead;
                count -= bytesRead;
            }
        }
    }
}
