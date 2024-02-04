using System;

namespace NativeCollection
{
    public interface INativeCollectionClass : IDisposable
    {
        void ReInit();

        bool IsDisposed { get; }
    }
}

