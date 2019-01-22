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
using MongoDB.Bson;

namespace MongoDB.Driver.Core.Bindings
{
    internal class NoCoreServerSession : ICoreServerSession
    {
        #region static
        // private static fields
        private static readonly ICoreServerSession __instance = new NoCoreServerSession();

        // public static properties
        public static ICoreServerSession Instance => __instance;
        #endregion

        public BsonDocument Id => null;

        public DateTime? LastUsedAt => null;

        public long AdvanceTransactionNumber()
        {
            return -1;
        }

        public void Dispose()
        {
        }

        public void WasUsed()
        {
        }
    }
}
