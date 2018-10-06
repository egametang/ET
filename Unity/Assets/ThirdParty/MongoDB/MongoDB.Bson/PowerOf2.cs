/* Copyright 2010-2015 MongoDB Inc.
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

namespace MongoDB.Bson
{
    internal static class PowerOf2
    {
        public static bool IsPowerOf2(int n)
        {
            return n == RoundUpToPowerOf2(n);
        }

        public static int RoundUpToPowerOf2(int n)
        {
            if (n < 0 || n > 0x40000000)
            {
                throw new ArgumentOutOfRangeException("n");
            }

            // see: Hacker's Delight, by Henry S. Warren
            n = n - 1;
            n = n | (n >> 1);
            n = n | (n >> 2);
            n = n | (n >> 4);
            n = n | (n >> 8);
            n = n | (n >> 16);
            return n + 1;
        }
    }
}
