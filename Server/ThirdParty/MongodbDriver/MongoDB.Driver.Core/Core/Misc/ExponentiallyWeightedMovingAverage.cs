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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Misc
{
    internal class ExponentiallyWeightedMovingAverage
    {
        // fields
        private readonly double _alpha;
        private TimeSpan? _average;

        // constructors
        public ExponentiallyWeightedMovingAverage(double alpha)
        {
            _alpha = Ensure.IsBetween(alpha, 0.0, 1.0, "alpha");
        }

        // properties
        public TimeSpan Average
        {
            get { return _average.GetValueOrDefault(TimeSpan.Zero); }
        }

        // methods
        public TimeSpan AddSample(TimeSpan value)
        {
            if (!_average.HasValue)
            {
                _average = value;
            }
            else
            {
                var ticks = (long)(_alpha * value.Ticks + (1 - _alpha) * _average.Value.Ticks);
                _average = TimeSpan.FromTicks(ticks);
            }

            return _average.Value;
        }
    }
}
