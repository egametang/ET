using System;
using System.Collections.Generic;

namespace ET
{
    public static class ETTaskHelper
    {
        private class CoroutineBlocker
        {
            private int count;

            private List<ETTask> tcss = new List<ETTask>();

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
                    List<ETTask> t = this.tcss;
                    this.tcss = null;
                    foreach (ETTask ttcs in t)
                    {
                        ttcs.SetResult();
                    }

                    return;
                }

                ETTask tcs = ETTask.Create(true);

                tcss.Add(tcs);
                await tcs;
            }
        }

        public static async ETTask<bool> WaitAny<T>(ETTask<T>[] tasks, ETCancellationToken cancellationToken = null)
        {
            if (tasks.Length == 0)
            {
                return false;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(2);

            foreach (ETTask<T> task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            async ETVoid RunOneTask(ETTask<T> task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }

            await coroutineBlocker.WaitAsync();

            if (cancellationToken == null)
            {
                return true;
            }

            return !cancellationToken.IsCancel();
        }

        public static async ETTask<bool> WaitAny(ETTask[] tasks, ETCancellationToken cancellationToken = null)
        {
            if (tasks.Length == 0)
            {
                return false;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(2);

            foreach (ETTask task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            async ETVoid RunOneTask(ETTask task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }

            await coroutineBlocker.WaitAsync();

            if (cancellationToken == null)
            {
                return true;
            }

            return !cancellationToken.IsCancel();
        }

        public static async ETTask<bool> WaitAll<T>(ETTask<T>[] tasks, ETCancellationToken cancellationToken = null)
        {
            if (tasks.Length == 0)
            {
                return false;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length + 1);

            foreach (ETTask<T> task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            async ETVoid RunOneTask(ETTask<T> task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }

            await coroutineBlocker.WaitAsync();

            if (cancellationToken == null)
            {
                return true;
            }

            return !cancellationToken.IsCancel();
        }

        public static async ETTask<bool> WaitAll<T>(List<ETTask<T>> tasks, ETCancellationToken cancellationToken = null)
        {
            if (tasks.Count == 0)
            {
                return false;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count + 1);

            foreach (ETTask<T> task in tasks)
            {
                RunOneTask(task).Coroutine();
            }

            async ETVoid RunOneTask(ETTask<T> task)
            {
                await task;
                await coroutineBlocker.WaitAsync();
            }

            await coroutineBlocker.WaitAsync();

            if (cancellationToken == null)
            {
                return true;
            }

            return !cancellationToken.IsCancel();
        }

        public static async ETTask<bool> WaitAll(ETTask[] tasks, ETCancellationToken cancellationToken = null)
        {
            if (tasks.Length == 0)
            {
                return false;
            }

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

            if (cancellationToken == null)
            {
                return true;
            }

            return !cancellationToken.IsCancel();
        }

        public static async ETTask<bool> WaitAll(List<ETTask> tasks, ETCancellationToken cancellationToken = null)
        {
            if (tasks.Count == 0)
            {
                return false;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count + 1);

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

            if (cancellationToken == null)
            {
                return true;
            }

            return !cancellationToken.IsCancel();
        }
    }
}