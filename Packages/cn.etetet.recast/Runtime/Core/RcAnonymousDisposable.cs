using System;

namespace DotRecast.Core
{
    public struct RcAnonymousDisposable : IDisposable
    {
        private Action _dispose;

        public RcAnonymousDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}