using System.Runtime.InteropServices;

namespace ET
{
    public static class WinPeriod
    {
        // 一般默认的精度不止1毫秒（不同操作系统有所不同），需要调用timeBeginPeriod与timeEndPeriod来设置精度
        [DllImport("winmm")]
        private static extern void timeBeginPeriod(int t);
        //[DllImport("winmm")]
        //static extern void timeEndPeriod(int t);

        public static void Init()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                timeBeginPeriod(1);
            }
        }
    }
}