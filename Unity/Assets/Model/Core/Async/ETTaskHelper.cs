using System.Collections.Generic;

namespace ET
{
    public static class ETTaskHelper
    {
        private class CoroutineBlocker
        {
            private int count;

            private List<ETTaskCompletionSource> tcss = new List<ETTaskCompletionSource>();

            public CoroutineBlocker(int count)
            {
                this.count = count;
            }

            public async ETTask WaitAsync()
            {
                --this.count;
                if (this.count < 0)
                {
                    return;
                }

                if (this.count == 0)
                {
                    List<ETTaskCompletionSource> t = this.tcss;
                    this.tcss = null;
                    foreach (ETTaskCompletionSource ttcs in t)
                    {
                        ttcs.SetResult();
                    }

                    return;
                }

                ETTaskCompletionSource tcs = new ETTaskCompletionSource();
                tcss.Add(tcs);
                await tcs.Task;
            }
        }

        public static async ETTask WaitAny<T>(ETTask<T>[] tasks)
        {
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(2);
            foreach (ETTask<T> task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            await coroutineBlocker.WaitAsync();

            async ETVoid RunOneTask(ETTask<T> task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }
        }

        public static async ETTask WaitAny(ETTask[] tasks)
        {
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(2);
            foreach (ETTask task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            await coroutineBlocker.WaitAsync();

            async ETVoid RunOneTask(ETTask task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }
        }

        public static async ETTask WaitAll<T>(ETTask<T>[] tasks)
        {
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length + 1);
            foreach (ETTask<T> task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            await coroutineBlocker.WaitAsync();

            async ETVoid RunOneTask(ETTask<T> task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }
        }

        public static async ETTask WaitAll(ETTask[] tasks)
        {
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length + 1);
            foreach (ETTask task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            await coroutineBlocker.WaitAsync();

            async ETVoid RunOneTask(ETTask task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }
        }
    }
}