using System;
using System.Collections.Generic;

namespace ET
{
    public static class ETTaskHelper
    {
        public static async ETTask<T> GetContextAsync<T>() where T: class
        {
            ETTask<object> tcs = ETTask<object>.Create(true);
            tcs.TaskType = TaskType.ContextTask;
            object ret = await tcs;
            if (ret == null)
            {
                return null;
            }
            return (T)ret;
        }
        
        public static bool IsCancel(this ETCancellationToken self)
        {
            if (self == null)
            {
                return false;
            }
            return self.IsDispose();
        }
        
        private class CoroutineBlocker
        {
            private int count;

            private ETTask tcs;

            public CoroutineBlocker(int count)
            {
                this.count = count;
            }
            
            public async ETTask RunSubCoroutineAsync(ETTask task)
            {
                try
                {
                    await task;
                }
                finally
                {
                    --this.count;
                
                    if (this.count <= 0 && this.tcs != null)
                    {
                        ETTask t = this.tcs;
                        this.tcs = null;
                        t.SetResult();
                    }
                }
            }

            public async ETTask WaitAsync()
            {
                if (this.count <= 0)
                {
                    return;
                }
                this.tcs = ETTask.Create(true);
                await tcs;
            }
        }

        public static async ETTask WaitAny(List<ETTask> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1);

            foreach (ETTask task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }

        public static async ETTask WaitAny(ETTask[] tasks)
        {
            if (tasks.Length == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1);

            foreach (ETTask task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }

        public static async ETTask WaitAll(ETTask[] tasks)
        {
            if (tasks.Length == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length);

            foreach (ETTask task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }

        public static async ETTask WaitAll(List<ETTask> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count);

            foreach (ETTask task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }
    }
}