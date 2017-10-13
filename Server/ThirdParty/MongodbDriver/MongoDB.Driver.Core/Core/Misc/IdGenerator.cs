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

using System.Threading;

namespace MongoDB.Driver.Core.Misc
{
    internal static class IdGenerator<T>
    {
        // static fields
        private static int __lastId;

        // static methods
        public static int GetNextId()
        {
            return Interlocked.Increment(ref __lastId);
        }
    }

    internal static class LongIdGenerator<T>
    {
        // static fields
        private static long __lastId;

        // static methods
        public static long GetNextId()
        {
            return Interlocked.Increment(ref __lastId);
        }
    }
}
