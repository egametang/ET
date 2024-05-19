using System.Threading;

namespace DotRecast.Core
{
    public class RcAtomicInteger
    {
        private volatile int _location;

        public RcAtomicInteger() : this(0)
        {
        }

        public RcAtomicInteger(int location)
        {
            _location = location;
        }

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref _location);
        }

        public int GetAndIncrement()
        {
            var next = Interlocked.Increment(ref _location);
            return next - 1;
        }


        public int DecrementAndGet()
        {
            return Interlocked.Decrement(ref _location);
        }

        public int Read()
        {
            return _location;
        }

        public int GetSoft()
        {
            return _location;
        }

        public int Exchange(int exchange)
        {
            return Interlocked.Exchange(ref _location, exchange);
        }

        public int Decrease(int value)
        {
            return Interlocked.Add(ref _location, -value);
        }

        public int CompareExchange(int value, int comparand)
        {
            return Interlocked.CompareExchange(ref _location, value, comparand);
        }

        public int Add(int value)
        {
            return Interlocked.Add(ref _location, value);
        }
    }
}