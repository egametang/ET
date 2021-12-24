using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ILRuntime.CLR.Method;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    class AsyncJITCompileWorker
    {
        AutoResetEvent evt = new AutoResetEvent(false);
        Queue<ILMethod> jobs = new Queue<ILMethod>();
        bool exit;
        Thread thread;
        public AsyncJITCompileWorker()
        {
            thread = new Thread(DoJob);
            thread.Name = "ILRuntime JIT Worker";
            thread.Start();
        }
        public void QueueCompileJob(ILMethod method)
        {
            if (exit)
                throw new NotSupportedException("Already disposed");
            lock (jobs)
                jobs.Enqueue(method);
            evt.Set();
        }

        public void Dispose()
        {
            exit = true;
            evt.Set ();
        }
        void DoJob()
        {
            while (!exit)
            {
                evt.WaitOne();
                while (jobs.Count > 0)
                {
                    ILMethod m;
                    lock (jobs)
                        m = jobs.Dequeue();
                    try
                    {
                        m.InitCodeBody(true);
                    }
                    catch(Exception ex)
                    {
                        string str = string.Format("Compile {0} failed\r\n{1}", m, ex);
#if UNITY_5_5_OR_NEWER
                        UnityEngine.Debug.LogError(str);
#else
                        Console.WriteLine(str);
#endif
                    }
                }
            }
        }
    }
}
