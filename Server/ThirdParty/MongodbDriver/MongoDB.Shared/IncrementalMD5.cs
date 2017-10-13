/* Copyright 2016 MongoDB Inc.
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
using System.Security.Cryptography;

namespace MongoDB.Shared
{
    internal abstract class IncrementalMD5 : IDisposable
    {
        public static IncrementalMD5 Create()
        {
#if NET45
            return new IncrementalMD5Net45();
#else
            return new IncrementalMD5NetStandard16();
#endif
        }

        public abstract void AppendData(byte[] data, int offset, int count);
        public abstract void Dispose();
        public abstract byte[] GetHashAndReset();
    }

#if NET45
    internal class IncrementalMD5Net45 : IncrementalMD5
    {
        private static readonly byte[] __emptyByteArray = new byte[0];

        private MD5 _md5;

        public override void AppendData(byte[] data, int offset, int count)
        {
            if (_md5 == null)
            {
                _md5 = MD5.Create();
            }
            _md5.TransformBlock(data, offset, count, null, 0);
        }

        public override void Dispose()
        {
            if (_md5 != null)
            {
                _md5.Dispose();
            }
        }

        public override byte[] GetHashAndReset()
        {
            if (_md5 == null)
            {
                _md5 = MD5.Create();
            }
            _md5.TransformFinalBlock(__emptyByteArray, 0, 0);
            var hash = _md5.Hash;
            _md5.Dispose();
            _md5 = null;
            return hash;
        }
    }
#else
    internal class IncrementalMD5NetStandard16 : IncrementalMD5
    {
        private readonly IncrementalHash _incrementalHash;

        public IncrementalMD5NetStandard16()
        {
            _incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        }

        public override void AppendData(byte[] data, int offset, int count)
        {
            _incrementalHash.AppendData(data, offset, count);
        }

        public override void Dispose()
        {
            _incrementalHash.Dispose();
        }

        public override byte[] GetHashAndReset()
        {
            return _incrementalHash.GetHashAndReset();
        }
    }
#endif
}
