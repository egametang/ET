using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example2_3_2
{
    class Program
    {
        private static int loopCount = 0;

        private static long time;
        private static TaskCompletionSource<bool> tcs;
        
        static void Main(string[] args)
        {
            Console.WriteLine($"主线程: {Thread.CurrentThread.ManagedThreadId}");

            Crontine();
            
            while (true)
            {
                Thread.Sleep(1);

                CheckTimerOut();
                
                ++loopCount;
                if (loopCount % 10000 == 0)
                {
                    Console.WriteLine($"loop count: {loopCount}");
                }
            }
        }
        
        private static async void Crontine()
        {
            await WaitTimeAsync(5000);
            Console.WriteLine($"当前线程: {Thread.CurrentThread.ManagedThreadId}, WaitTimeAsync finsih loopCount的值是: {loopCount}");
            await WaitTimeAsync(4000);
            Console.WriteLine($"当前线程: {Thread.CurrentThread.ManagedThreadId}, WaitTimeAsync finsih loopCount的值是: {loopCount}");
            await WaitTimeAsync(3000);
            Console.WriteLine($"当前线程: {Thread.CurrentThread.ManagedThreadId}, WaitTimeAsync finsih loopCount的值是: {loopCount}");
        }

        private static void CheckTimerOut()
        {
            if (time == 0)
            {
                return;
            }
            long nowTicks = DateTime.Now.Ticks / 10000;
            if (time > nowTicks)
            {
                return;
            }

            time = 0;
            tcs.SetResult(true);
        }
        
        private static Task WaitTimeAsync(int waitTime)
        {
            TaskCompletionSource<bool> t = new TaskCompletionSource<bool>();
            time = DateTime.Now.Ticks / 10000 + waitTime;
            tcs = t;
            return t.Task;
        }
    }
}