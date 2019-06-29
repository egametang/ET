﻿/* Copyright 2013-present MongoDB Inc.
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

using System.Collections.Generic;

namespace MongoDB.Driver.Core.Misc
{
    internal class ToStringComparer<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            if (object.ReferenceEquals(x, null) && object.ReferenceEquals(y, null))
            {
                return 0;
            }
            if (object.ReferenceEquals(x, null))
            {
                return -1;
            }
            if (object.ReferenceEquals(y, null))
            {
                return 1;
            }
            return x.ToString().CompareTo(y.ToString());
        }
    }
}
