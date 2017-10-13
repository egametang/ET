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

using System;

namespace MongoDB.Driver.Core.Misc
{
    internal static class ThreadStaticRandom
    {
        // static fields
        [ThreadStatic]
        private static Random __threadStaticRandom;

        // static methods
        public static int Next(int maxValue)
        {
            var random = __threadStaticRandom;
            if (random == null)
            {
                random = __threadStaticRandom = new Random();
            }

            return random.Next(maxValue);
        }
    }
}
