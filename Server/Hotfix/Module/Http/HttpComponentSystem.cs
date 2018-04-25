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