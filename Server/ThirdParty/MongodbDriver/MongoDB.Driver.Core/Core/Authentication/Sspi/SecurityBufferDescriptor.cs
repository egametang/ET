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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MongoDB.Driver.Core.Authentication.Sspi
{
    /// <summary>
    /// A SecBufferDesc structure.
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa379815(v=vs.85).aspx
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable", Justification = "Implementing IDisposable on a struct leads to memory leaks.")]
    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityBufferDescriptor
    {
        // fields
        public SecurityBufferType BufferType;
        public int NumBuffers;
        public IntPtr BufferPtr; //Point to SecBuffer

        // constructors
        public SecurityBufferDescriptor(int bufferSize)
        {
            BufferType = SecurityBufferType.Version;
            NumBuffers = 1;
            var buffer = new SecurityBuffer(bufferSize);
            BufferPtr = Marshal.AllocHGlobal(Marshal.SizeOf(buffer));
            Marshal.StructureToPtr(buffer, BufferPtr, false);
        }

        public SecurityBufferDescriptor(byte[] secBufferBytes)
        {
            BufferType = SecurityBufferType.Version;
            NumBuffers = 1;
            var buffer = new SecurityBuffer(secBufferBytes);
            BufferPtr = Marshal.AllocHGlobal(Marshal.SizeOf(buffer));
            Marshal.StructureToPtr(buffer, BufferPtr, false);
        }

        public SecurityBufferDescriptor(SecurityBuffer[] buffers)
        {
            if (buffers == null || buffers.Length == 0)
            {
                throw new ArgumentException("cannot be null or 0 length", "buffers");
            }

            BufferType = SecurityBufferType.Version;
            NumBuffers = buffers.Length;

            //Allocate memory for SecBuffer Array....
#if NET45
            BufferPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SecurityBuffer)) * NumBuffers);
#else
            BufferPtr = Marshal.AllocHGlobal(Marshal.SizeOf<SecurityBuffer>() * NumBuffers);
#endif

            for (int i = 0; i < buffers.Length; i++)
            {
                var currentBuffer = buffers[i];
#if NET45
                var currentOffset = i * Marshal.SizeOf(typeof(SecurityBuffer));
#else
                var currentOffset = i * Marshal.SizeOf<SecurityBuffer>();
#endif
                Marshal.WriteInt32(BufferPtr, currentOffset, currentBuffer.Count);

#if NET45
                var length = currentOffset + Marshal.SizeOf(typeof(int));
#else
                var length = currentOffset + Marshal.SizeOf<int>();
#endif
                Marshal.WriteInt32(BufferPtr, length, (int)currentBuffer.BufferType);

#if NET45
                length = currentOffset + Marshal.SizeOf(typeof(int)) + Marshal.SizeOf(typeof(int));
#else
                length = currentOffset + Marshal.SizeOf<int>() + Marshal.SizeOf<int>();
#endif
                Marshal.WriteIntPtr(BufferPtr, length, currentBuffer.Token);
            }
        }

        // methods
        public void Free()
        {
            if (BufferPtr != IntPtr.Zero)
            {
                if (NumBuffers == 1)
                {
#if NET45
                    var buffer = (SecurityBuffer)Marshal.PtrToStructure(BufferPtr, typeof(SecurityBuffer));
#else
                    var buffer = Marshal.PtrToStructure<SecurityBuffer>(BufferPtr);
#endif
                    buffer.Free();
                }
                else
                {
                    // Since we aren't sending any messages using the kerberos encrypt/decrypt.
                    // The 1st buffer is going to be empty. We can skip it.
                    for (int i = 1; i < NumBuffers; i++)
                    {
#if NET45
                        var currentOffset = i * Marshal.SizeOf(typeof(SecurityBuffer));
                        var totalLength = currentOffset + Marshal.SizeOf(typeof(int)) + Marshal.SizeOf(typeof(int));
#else
                        var currentOffset = i * Marshal.SizeOf<SecurityBuffer>();
                        var totalLength = currentOffset + Marshal.SizeOf<int>() + Marshal.SizeOf<int>();
#endif
                        var buffer = Marshal.ReadIntPtr(BufferPtr, totalLength);
                        Marshal.FreeHGlobal(buffer);
                    }
                }

                Marshal.FreeHGlobal(BufferPtr);
                BufferPtr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// To the byte array.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Object has already been disposed!!!</exception>
        public byte[] ToByteArray()
        {
            byte[] bytes = null;

            if (BufferPtr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Object has already been disposed!!!");
            }

            if (NumBuffers == 1)
            {
#if NET45
                var buffer = (SecurityBuffer)Marshal.PtrToStructure(BufferPtr, typeof(SecurityBuffer));
#else
                var buffer = Marshal.PtrToStructure<SecurityBuffer>(BufferPtr);
#endif

                if (buffer.Count > 0)
                {
                    bytes = new byte[buffer.Count];
                    Marshal.Copy(buffer.Token, bytes, 0, buffer.Count);
                }
            }
            else
            {
                var bytesToAllocate = 0;

                for (int i = 0; i < NumBuffers; i++)
                {
#if NET45
                    var currentOffset = i * Marshal.SizeOf(typeof(SecurityBuffer));
#else
                    var currentOffset = i * Marshal.SizeOf<SecurityBuffer>();
#endif
                    bytesToAllocate += Marshal.ReadInt32(BufferPtr, currentOffset);
                }

                bytes = new byte[bytesToAllocate];

                for (int i = 0, bufferIndex = 0; i < NumBuffers; i++)
                {
#if NET45
                    var currentOffset = i * Marshal.SizeOf(typeof(SecurityBuffer));
#else
                    var currentOffset = i * Marshal.SizeOf<SecurityBuffer>();
#endif
                    var bytesToCopy = Marshal.ReadInt32(BufferPtr, currentOffset);
#if NET45
                    var length = currentOffset + Marshal.SizeOf(typeof(int)) + Marshal.SizeOf(typeof(int));
#else
                    var length = currentOffset + Marshal.SizeOf<int>() + Marshal.SizeOf<int>();
#endif
                    IntPtr SecBufferpvBuffer = Marshal.ReadIntPtr(BufferPtr, length);
                    Marshal.Copy(SecBufferpvBuffer, bytes, bufferIndex, bytesToCopy);
                    bufferIndex += bytesToCopy;
                }
            }

            return (bytes);
        }
    }
}
