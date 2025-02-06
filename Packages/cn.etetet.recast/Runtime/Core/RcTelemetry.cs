/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DotRecast.Core
{
    public class RcTelemetry
    {
        private readonly ThreadLocal<Dictionary<string, RcAtomicLong>> _timerStart;
        private readonly ConcurrentDictionary<string, RcAtomicLong> _timerAccum;

        public RcTelemetry()
        {
            _timerStart = new ThreadLocal<Dictionary<string, RcAtomicLong>>(() => new Dictionary<string, RcAtomicLong>());
            _timerAccum = new ConcurrentDictionary<string, RcAtomicLong>();
        }

        public IDisposable ScopedTimer(RcTimerLabel label)
        {
            StartTimer(label);
            return new RcAnonymousDisposable(() => StopTimer(label));
        }
        
        public void StartTimer(RcTimerLabel label)
        {
            _timerStart.Value[label.Name] = new RcAtomicLong(RcFrequency.Ticks);
        }


        public void StopTimer(RcTimerLabel label)
        {
            _timerAccum
                .GetOrAdd(label.Name, _ => new RcAtomicLong(0))
                .AddAndGet(RcFrequency.Ticks - _timerStart.Value?[label.Name].Read() ?? 0);
        }

        public void Warn(string message)
        {
            Console.WriteLine(message);
        }

        public List<RcTelemetryTick> ToList()
        {
            return _timerAccum
                .Select(x => new RcTelemetryTick(x.Key, x.Value.Read()))
                .ToList();
        }
    }
}