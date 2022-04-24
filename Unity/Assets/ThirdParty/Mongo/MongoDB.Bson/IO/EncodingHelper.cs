/* Copyright 2020-present MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a class that has some helper methods for <see cref="Encoding"/>.
    /// </summary>
    internal static class EncodingHelper
    {
        public readonly struct DisposableSegment : IDisposable
        {
            private IDisposable DisposableData { get; }
            public ArraySegment<byte> Segment { get; }

            public DisposableSegment(IDisposable disposableData, ArraySegment<byte> segment)
            {
                DisposableData = disposableData;
                Segment = segment;
            }

            public void Dispose()
            {
                DisposableData?.Dispose();
            }
        }

        private static readonly ArraySegment<byte> __emptySegment = new ArraySegment<byte>(new byte[0]);

        public static DisposableSegment GetBytesUsingThreadStaticBuffer(this Encoding encoding, string value)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var length = value.Length;
            if (length == 0)
            {
                return new DisposableSegment(null, __emptySegment);
            }

            var maxSize = encoding.GetMaxByteCount(length);
            var rentedBuffer = ThreadStaticBuffer.RentBuffer(maxSize);

            var size = encoding.GetBytes(value, 0, length, rentedBuffer.Bytes, 0);
            var segment = new ArraySegment<byte>(rentedBuffer.Bytes, 0, size);

            return new DisposableSegment(rentedBuffer, segment);
        }
    }
}
