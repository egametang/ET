using UnityEngine;

namespace ET.Client
{
    [ObjectSystem]
    public class GlobalComponentAwakeSystem: AwakeSystem<GlobalComponent>
    {
        protected override void Awake(GlobalComponent self)
        {
            GlobalComponent.Instance = self;
            
            self.Global = GameObject.Find("/Global").transform;
            self.Unit = GameObject.Find("/Global/Unit").transform;
            self.UI = GameObject.Find("/Global/UI").transform;
            self.NormalRoot = GameObject.Find("Global/UI/NormalRoot").transform;
            self.PopUpRoot = GameObject.Find("Global/UI/PopUpRoot").transform;
            self.FixedRoot = GameObject.Find("Global/UI/FixedRoot").transform;
            self.OtherRoot = GameObject.Find("Global/UI/OtherRoot").transform;
            self.PoolRoot =  GameObject.Find("Global/PoolRoot").transform;
        }
    }
}