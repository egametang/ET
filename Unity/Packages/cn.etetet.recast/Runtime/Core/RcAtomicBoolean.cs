using System.Threading;

namespace DotRecast.Core
{
    public class RcAtomicBoolean
    {
        private volatile int _location;

        public bool Set(bool v)
        {
            return 0 != Interlocked.Exchange(ref _location, v ? 1 : 0);
        }

        public bool Get()
        {
            return 0 != _location;
        }
    }
}