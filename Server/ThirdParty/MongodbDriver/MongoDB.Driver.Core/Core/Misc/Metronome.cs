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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Misc
{
    internal sealed class Metronome
    {
        // fields
        private readonly IClock _clock;
        private DateTime _nextTick;
        private readonly TimeSpan _period;

        // constructors
        public Metronome(TimeSpan period)
            : this(period, SystemClock.Instance)
        {
        }

        internal Metronome(TimeSpan period, IClock clock)
        {
            _period = Ensure.IsInfiniteOrGreaterThanOrEqualToZero(period, nameof(period));
            _clock = Ensure.IsNotNull(clock, nameof(clock));
            _nextTick = clock.UtcNow;
        }

        // properties
        public DateTime NextTick
        {
            get { return _nextTick; }
        }

        public TimeSpan Period
        {
            get { return _period; }
        }

        // methods
        public TimeSpan GetNextTickDelay()
        {
            if (_period == Timeout.InfiniteTimeSpan)
            {
                return Timeout.InfiniteTimeSpan;
            }

            var now = _clock.UtcNow;
            while (_nextTick < now)
            {
                _nextTick += _period;
            }
            return _nextTick - now;
        }
    }
}
