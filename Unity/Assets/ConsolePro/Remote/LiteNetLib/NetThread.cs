#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
#if WINRT && !UNITY_EDITOR
#define USE_WINRT
#endif

using System;
using System.Threading;

#if USE_WINRT
using Windows.Foundation;
using Windows.System.Threading;
using Windows.System.Threading.Core;
#endif

namespace FlyingWormConsole3.LiteNetLib
{
    public sealed class NetThread
    {
#if USE_WINRT
        private readonly ManualResetEvent _updateWaiter = new ManualResetEvent(false);
        private readonly ManualResetEvent _joinWaiter = new ManualResetEvent(false);
#else
        private Thread _thread;
#endif

        private readonly Action _callback;

        public int SleepTime;
        private bool _running;
        private readonly string _name;

        public bool IsRunning
        {
            get { return _running; }
        }

        public NetThread(string name, int sleepTime, Action callback)
        {
            _callback = callback;
            SleepTime = sleepTime;
            _name = name;
        }

        public void Start()
        {
            if (_running)
                return;
            _running = true;
#if USE_WINRT
            var thread = new PreallocatedWorkItem(ThreadLogic, WorkItemPriority.Normal, WorkItemOptions.TimeSliced);
            thread.RunAsync().AsTask();
#else
            _thread = new Thread(ThreadLogic)
            {
                Name = _name,
                IsBackground = true
            };
            _thread.Start();
#endif
        }

        public void Stop()
        {
            if (!_running)
                return;
            _running = false;

#if USE_WINRT
            _joinWaiter.WaitOne();
#else
            _thread.Join();
#endif
        }

#if USE_WINRT
        private void ThreadLogic(IAsyncAction action)
        {
            while (_running)
            {
                _callback();
                _updateWaiter.WaitOne(SleepTime);
            }
            _joinWaiter.Set();
        }
#else
        private void ThreadLogic()
        {
            while (_running)
            {
                _callback();
                Thread.Sleep(SleepTime);
            }
        }
#endif
    }
}
#endif
