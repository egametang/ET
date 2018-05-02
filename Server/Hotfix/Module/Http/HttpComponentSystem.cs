using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ObjectSystem]
    public class HttpComponentComponentAwakeSystem : AwakeSystem<HttpComponent>
    {
        public override void Awake(HttpComponent self)
        {
            self.Run((context, next) =>
            {
                Log.Info("这是第一个中间件....." + context.Request.Url.AbsolutePath);

                return next(context);
            });


            self.Run("/t", (c, n) =>
            {
                Log.Info("这是第二个中间件 拦截 /t 请求，继续往后调用.....");

                return n(c);
            });

            self.Run((c, n) =>
            {

                Log.Info("这是第三个中间件，不会往后继续调用了.....");

                return null; // 不往后执行了。
            });

            self.UseMvc();

            self.Awake();
        }
    }

    [ObjectSystem]
    public class HttpComponentComponentLoadSystem : LoadSystem<HttpComponent>
    {
        public override void Load(HttpComponent self)
        {
            self.Load();
        }
    }

    [ObjectSystem]
    public class HttpComponentComponentStartSystem : StartSystem<HttpComponent>
    {
        public override void Start(HttpComponent self)
        {
            self.Start();
        }
    }
}