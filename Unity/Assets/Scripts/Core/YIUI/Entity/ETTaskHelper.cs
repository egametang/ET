using System;

namespace ET
{
    /// <summary>
    /// 额外增加的异步助手 用于等待完成
    /// </summary>
    public static class ETTaskHelperExtend
    {
        public static async ETTask WaitUntil(this Entity self, Func<bool> func)
        {
            while (true)
            {
                await self.Fiber().Root.GetComponent<TimerComponent>().WaitFrameAsync();
                if (func == null || func.Invoke()) return;
            }
        }
        
        public static async ETTask WaitUntil(this TimerComponent self, Func<bool> func)
        {
            while (true)
            {
                await self.WaitFrameAsync();
                if (func == null || func.Invoke()) return;
            }
        }
    }
}