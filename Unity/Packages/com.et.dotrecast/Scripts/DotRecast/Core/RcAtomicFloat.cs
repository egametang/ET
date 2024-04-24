using System.Threading;

namespace DotRecast.Core
{
    public class RcAtomicFloat
    {
        private volatile float _location;

        public RcAtomicFloat(float location)
        {
            _location = location;
        }

        public float Get()
        {
            return _location;
        }

        public float Exchange(float exchange)
        {
            return Interlocked.Exchange(ref _location, exchange);
        }

        public float CompareExchange(float value, float comparand)
        {
            return Interlocked.CompareExchange(ref _location, value, comparand);
        }
    }
}