using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public partial class VProcessManager: Singleton<VProcessManager>
    {
        // 一个Process一个固定的线程
        public class ThreadScheduler: Singleton<ThreadScheduler>, IVProcessScheduler
        {
            private bool isStart;

            private readonly ConcurrentDictionary<VProcess, Thread> dictionary = new();

            public void Awake()
            {
                this.isStart = true;
            }

            private void Loop(int vProcessId)
            {
                VProcess vProcess = VProcessManager.Instance.Get(vProcessId);
                if (vProcess == null)
                {
                    return;
                }

                while (this.isStart)
                {
                    try
                    {
                        vProcess.Update();
                        vProcess.LateUpdate();

                        Thread.Sleep(1);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            public override void Dispose()
            {
                base.Dispose();
                
                this.isStart = false;
                foreach (var kv in this.dictionary)
                {
                    kv.Value.Join();
                }
            }

            public int Create(int vProcessId = 0)
            {
                vProcessId = VProcessManager.Instance.Create(vProcessId);
                Thread thread = new(() => this.Loop(vProcessId));
                thread.Start();
                return vProcessId;
            }
        }
    }
}