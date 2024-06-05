using System;
using System.Threading;

namespace ET
{
    public static class Program
    {
        public static void Main()
        {
            //这里大家看着可能比较困惑，为什么要绕一大圈呢，之前这里直接调用Model层，现在却要在CoderLoader中获取Model的程序集找到Entry入口再调用
            //原因是，之前ET.App直接依赖Model，但是在客户端，之前的Loader却不依赖Model。这导致前端跟后端程序集依赖不太一样
            //这样前后端Model都引用Loader，Loader通过反射去调用Model的Entry。
            //这样前后端的程序集依赖就保持了一致。这里调用了Entry.Init()是为了防止dotnet裁剪Model的程序集，毕竟如果App没有调用model，那么dotnet认为
            //model并没有用到，就不会加载，结果会导致CodeLoader反射调用model失败。
            //客户端服务端不热更不共享的组件可以写到Loader中，比如表现层需要一个组件不需要热更，可以写在Loader中，这样性能更高。如果客户端跟服务端共享的并且不需要热更的
            //的组件可以写在Core中
            Entry.Init();
            
            Init init = new();
            init.Start();
            
            while (true)
            {
                Thread.Sleep(1);
                try
                {
                    init.Update();
                    init.LateUpdate();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}