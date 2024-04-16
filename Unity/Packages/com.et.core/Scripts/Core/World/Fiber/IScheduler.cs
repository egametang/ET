using System;

namespace ET
{
    public interface IScheduler: IDisposable
    {
        void Add(int fiberId);
    }
}