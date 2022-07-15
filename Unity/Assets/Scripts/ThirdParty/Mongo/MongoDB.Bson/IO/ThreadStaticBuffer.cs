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
using System.Threading;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a class that provides reusable buffer per thread.
    /// Use this technique ONLY when:
    ///     1. Buffer is not shared across multiple threads.
    ///     2. No nested methods invocations use the same buffer.
    /// Advised to limit the usage scope to a single method.
    /// </summary>
    internal static class ThreadStaticBuffer
    {
        public readonly struct RentedBuffer : IDisposable
        {
            private readonly int _ownerThreadId;
            private readonly byte[] _bytes;

            public RentedBuffer(int ownerThreadId, byte[] bytes)
            {
                _ownerThreadId = ownerThreadId;
                _bytes = bytes;
            }

            public void Dispose()
            {
                if (_ownerThreadId != __threadId)
                {
                    throw new InvalidOperationException("Attempt to return thread static buffer from the wrong thread.");
                }

                if (!__isBufferRented)
                {
                    throw new InvalidOperationException("Attempt to return a thread static buffer that is not in use.");
                }

                __isBufferRented = false;
            }

            public byte[] Bytes => _bytes;
        }

        private const int MinSize = 256;
        private const int MaxSize = 16384;
        private const int MaxAllocationSize = 1024 * 1024 * 1024; // 1GB

        // private static fields
        [ThreadStatic]
        private static byte[] __buffer;
        [ThreadStatic]
        private static bool __isBufferRented;
        [ThreadStatic]
        private static int __threadId;

        public static RentedBuffer RentBuffer(int size)
        {
            if (__isBufferRented)
            {
                throw new InvalidOperationException("Thread static buffer is already in use.");
            }

            if (size <= 0 || size > MaxAllocationSize)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, $"Valid buffer size range is [1..{MaxAllocationSize}] bytes.");
            }

            __isBufferRented = true;

            if (__threadId == default)
            {
                __threadId = Thread.CurrentThread.ManagedThreadId;
            }

            if (size > MaxSize)
            {
                return new RentedBuffer(__threadId, new byte[size]);
            }

            if (__buffer == null || __buffer.Length < size)
            {
                var newSize = size <= MinSize ? MinSize : PowerOf2.RoundUpToPowerOf2(size);
                __buffer = new byte[newSize];
            }

            return new RentedBuffer(__threadId, __buffer);
        }
    }
}
