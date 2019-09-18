/* Copyright 2017-present MongoDB Inc.
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
using System.Collections.Generic;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    internal class CoreServerSessionPool : ICoreServerSessionPool
    {
        // private fields
        private readonly ICluster _cluster;
        private readonly object _lock = new object();
        private readonly List<ICoreServerSession> _pool = new List<ICoreServerSession>();

        // constructors
        public CoreServerSessionPool(ICluster cluster)
        {
            _cluster = Ensure.IsNotNull(cluster, nameof(cluster));
        }

        /// <inheritdoc />
        public ICoreServerSession AcquireSession()
        {
            lock (_lock)
            {
                for (var i = _pool.Count - 1; i >= 0; i--)
                {
                    var pooledSession = _pool[i];
                    if (IsAboutToExpire(pooledSession))
                    {
                        pooledSession.Dispose();
                    }
                    else
                    {
                        var removeCount = _pool.Count - i; // the one we're about to return and any about to expire ones we skipped over
                        _pool.RemoveRange(i, removeCount);
                        return new ReleaseOnDisposeCoreServerSession(pooledSession, this);
                    }
                }

                _pool.Clear(); // they're all about to expire
            }

            return new ReleaseOnDisposeCoreServerSession(new CoreServerSession(), this);
        }

        /// <inheritdoc />
        public void ReleaseSession(ICoreServerSession session)
        {
            lock (_lock)
            {
                var removeCount = 0;
                for (var i = 0; i < _pool.Count; i++)
                {
                    var pooledSession = _pool[i];
                    if (IsAboutToExpire(pooledSession))
                    {
                        pooledSession.Dispose();
                        removeCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                _pool.RemoveRange(0, removeCount);

                if (IsAboutToExpire(session))
                {
                    session.Dispose();
                }
                else
                {
                    _pool.Add(session);
                }
            }
        }

        // private methods
        private bool IsAboutToExpire(ICoreServerSession session)
        {
            var logicalSessionTimeout = _cluster.Description.LogicalSessionTimeout;
            if (!session.LastUsedAt.HasValue || !logicalSessionTimeout.HasValue)
            {
                return true;
            }
            else
            {
                var expiresAt = session.LastUsedAt.Value + logicalSessionTimeout.Value;
                var timeRemaining = expiresAt - DateTime.UtcNow;
                return timeRemaining < TimeSpan.FromMinutes(1);
            }
        }

        // nested types
        internal sealed class ReleaseOnDisposeCoreServerSession : WrappingCoreServerSession
        {
            // private fields
            private readonly ICoreServerSessionPool _pool;

            // constructors
            public ReleaseOnDisposeCoreServerSession(ICoreServerSession wrapped, ICoreServerSessionPool pool)
                : base(wrapped, ownsWrapped: false)
            {
                _pool = Ensure.IsNotNull(pool, nameof(pool));
            }

            // protected methods
            protected override void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _pool.ReleaseSession(Wrapped);
                    }
                }
                base.Dispose(disposing);
            }
        }
    }
}
