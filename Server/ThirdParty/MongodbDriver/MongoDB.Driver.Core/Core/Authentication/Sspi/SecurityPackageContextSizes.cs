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

using System.Runtime.InteropServices;

namespace MongoDB.Driver.Core.Authentication.Sspi
{
    /// <summary>
    /// A SecPkgContext_Sizes structure.
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa380097(v=vs.85).aspx
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityPackageContextSizes
    {
        public uint MaxToken;
        public uint MaxSignature;
        public uint BlockSize;
        public uint SecurityTrailer;
    };
}
