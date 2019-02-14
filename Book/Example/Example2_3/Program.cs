using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example2_3
{
    
    class Program
    {
        private static int loopCount = 0;

        private static long time;
        private static Action action;
        
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
        
        private static void Crontine()
        {
            WaitTimeAsync(5000, WaitTimeAsyncCallback1);
        }

        private static void WaitTimeAsyncCallback1()
        {
            Console.WriteLine($"当前线程: {Thread.CurrentThread.ManagedThreadId}, WaitTimeAsync finsih loopCount的值是: {loopCount}");
            WaitTimeAsync(4000, WaitTimeAsyncCallback2);
        }
        
        private static void WaitTimeAsyncCallback2()
        {
            Console.WriteLine($"当前线程: {Thread.CurrentThread.ManagedThreadId}, WaitTimeAsync finsih loopCount的值是: {loopCount}");
            WaitTimeAsync(3000, WaitTimeAsyncCallback3);
        }
        
        private static void WaitTimeAsyncCallback3()
        {
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
            action.Invoke();
        }
        
        private static void WaitTimeAsync(int waitTime, Action a)
        {
            time = DateTime.Now.Ticks / 10000 + waitTime;
            action = a;
        }
    }
}