using System;

namespace DotRecast.Core
{
    public readonly struct RcTelemetryTick
    {
        public readonly string Key;
        public readonly long Ticks;
        public long Millis => Ticks / TimeSpan.TicksPerMillisecond;
        public long Micros => Ticks / 10; // TimeSpan.TicksPerMicrosecond;

        public RcTelemetryTick(string key, long ticks)
        {
            Key = key;
            Ticks = ticks;
        }
    }
}