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
using System.Runtime.InteropServices;
using System.Security;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    internal sealed class DecryptedSecureString : IDisposable
    {
        // private fields
        private char[] _chars;
        private GCHandle _charsHandle;
        private IntPtr _charsIntPtr;
        private bool _disposed;
        private readonly SecureString _secureString;
        private byte[] _utf8Bytes;
        private GCHandle _utf8BytesHandle;

        // constructors
        public DecryptedSecureString(SecureString secureString)
        {
            _secureString = Ensure.IsNotNull(secureString, nameof(secureString));
        }

        // finalizer
        ~DecryptedSecureString()
        {
            Dispose(false);
        }

        // public methods
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public char[] GetChars()
        {
            if (_chars == null)
            {
#if NET452
                _charsIntPtr = Marshal.SecureStringToGlobalAllocUnicode(_secureString);
#else
                _charsIntPtr = SecureStringMarshal.SecureStringToGlobalAllocUnicode(_secureString);
#endif
                _chars = new char[_secureString.Length];
                _charsHandle = GCHandle.Alloc(_chars, GCHandleType.Pinned);
                Marshal.Copy(_charsIntPtr, _chars, 0, _secureString.Length);
            }

            return _chars;
        }

        public byte[] GetUtf8Bytes()
        {
            if (_utf8Bytes == null)
            {
                var chars = GetChars();
                var encoding = Utf8Encodings.Strict;
                _utf8Bytes = new byte[encoding.GetByteCount(chars)];
                _utf8BytesHandle = GCHandle.Alloc(_utf8Bytes, GCHandleType.Pinned);
                encoding.GetBytes(chars, 0, chars.Length, _utf8Bytes, 0);
            }

            return _utf8Bytes;
        }

        // private methods
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_utf8Bytes != null)
                    {
                        Array.Clear(_utf8Bytes, 0, _utf8Bytes.Length);
                    }
                    if (_chars != null)
                    {
                        Array.Clear(_chars, 0, _chars.Length);
                    }
                }
                if (_utf8BytesHandle.IsAllocated)
                {
                    _utf8BytesHandle.Free();
                }
                if (_charsHandle.IsAllocated)
                {
                    _charsHandle.Free();
                }
                if (_charsIntPtr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(_charsIntPtr);
                    _charsIntPtr = IntPtr.Zero; // to facilitate testing
                }
                _disposed = true;
            }
        }
    }
}
