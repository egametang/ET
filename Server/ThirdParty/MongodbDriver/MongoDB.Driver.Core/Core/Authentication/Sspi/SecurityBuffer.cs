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
    /// A SecBuffer structure.
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa379814(v=vs.85).aspx
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable", Justification="Implementing IDisposable on a struct leads to memory leaks.")] 
    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityBuffer
    {
        // fields
        public int Count;
        public SecurityBufferType BufferType;
        public IntPtr Token;

        // constructors
        public SecurityBuffer(int bufferSize)
        {
            Count = bufferSize;
            BufferType = SecurityBufferType.Token;
            Token = Marshal.AllocHGlobal(bufferSize);
        }

        public SecurityBuffer(byte[] bytes)
        {
            Count = bytes.Length;
            BufferType = SecurityBufferType.Token;
            Token = Marshal.AllocHGlobal(Count);
            Marshal.Copy(bytes, 0, Token, Count);
        }

        public SecurityBuffer(byte[] bytes, SecurityBufferType bufferType)
        {
            BufferType = bufferType;

            if (bytes != null && bytes.Length != 0)
            {
                Count = bytes.Length;
                Token = Marshal.AllocHGlobal(Count);
                Marshal.Copy(bytes, 0, Token, Count);
            }
            else
            {
                Count = 0;
                Token = IntPtr.Zero;
            }
        }

        // methods
        public void Free()
        {
            if (Token != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Token);
                Token = IntPtr.Zero;
            }
        }
    }
}
