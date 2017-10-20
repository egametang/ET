/* Copyright 2015 MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;

#if NET45
using System.Runtime.Remoting.Messaging;
#endif
#if !NET45
using System.Threading;
#endif

namespace MongoDB.Driver.Core.Events
{
    internal static class EventContext
    {
        private static readonly AsyncLocal<int?> __findOperationBatchSize = new AsyncLocal<int?>();
        private static readonly AsyncLocal<int?> __findOperationLimit = new AsyncLocal<int?>();
        private static readonly AsyncLocal<CollectionNamespace> __killCursorsCollectionNamespace = new AsyncLocal<CollectionNamespace>();
        private static readonly AsyncLocal<long?> __operationId = new AsyncLocal<long?>();

        public static int? FindOperationBatchSize
        {
            get
            {
                return __findOperationBatchSize.Value;
            }
            private set
            {
                __findOperationBatchSize.Value = value;
            }
        }

        public static int? FindOperationLimit
        {
            get
            {
                return __findOperationLimit.Value;
            }
            private set
            {
                __findOperationLimit.Value = value;
            }
        }

        public static CollectionNamespace KillCursorsCollectionNamespace
        {
            get
            {
                return __killCursorsCollectionNamespace.Value;
            }
            private set
            {
                __killCursorsCollectionNamespace.Value = value;
            }
        }

        public static long? OperationId
        {
            get
            {
                return __operationId.Value;
            }
            private set
            {
                __operationId.Value = value;
            }
        }

        public static IDisposable BeginFind(int? batchSize, int? limit)
        {
            return FindOperationBatchSize == null ?
                (IDisposable)new FindOperationDisposer(batchSize, limit) :
                NoOpDisposer.Instance;
        }

        public static IDisposable BeginKillCursors(CollectionNamespace collectionNamespace)
        {
            return KillCursorsCollectionNamespace == null ?
                (IDisposable)new KillCursorsOperationDisposer(collectionNamespace) :
                NoOpDisposer.Instance;
        }

        public static IDisposable BeginOperation()
        {
            return BeginOperation(null);
        }

        public static IDisposable BeginOperation(long? operationId)
        {
            return OperationId == null ?
                (IDisposable)new OperationIdDisposer(operationId ?? LongIdGenerator<OperationIdDisposer>.GetNextId()) :
                NoOpDisposer.Instance;
        }

        private sealed class NoOpDisposer : IDisposable
        {
            public static NoOpDisposer Instance = new NoOpDisposer();

            public void Dispose()
            {
                // do nothing
            }
        }

        private sealed class FindOperationDisposer : IDisposable
        {
            public FindOperationDisposer(int? batchSize, int? limit)
            {
                EventContext.FindOperationBatchSize = batchSize;
                EventContext.FindOperationLimit = limit;
            }

            public void Dispose()
            {
                EventContext.FindOperationBatchSize = null;
                EventContext.FindOperationLimit = null;
            }
        }

        private sealed class KillCursorsOperationDisposer : IDisposable
        {
            public KillCursorsOperationDisposer(CollectionNamespace collectionNamespace)
            {
                EventContext.KillCursorsCollectionNamespace = collectionNamespace;
            }

            public void Dispose()
            {
                EventContext.KillCursorsCollectionNamespace = null;
            }
        }

        private sealed class OperationIdDisposer : IDisposable
        {
            public OperationIdDisposer(long operationId)
            {
                EventContext.OperationId = operationId;
            }

            public void Dispose()
            {
                EventContext.OperationId = null;
            }
        }

#if NET45
        private class AsyncLocal<T>
        {
            private readonly string __name;

            public AsyncLocal()
            {
                __name = Guid.NewGuid().ToString();
            }

            public T Value
            {
                get
                {
                    var value = CallContext.LogicalGetData(__name);
                    return value == null ? default(T) : (T)value;
                }
                set
                {
                    CallContext.LogicalSetData(__name, value);
                }
            }
        }
#endif
    }
}
