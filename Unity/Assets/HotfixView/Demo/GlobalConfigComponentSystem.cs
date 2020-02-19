using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class GlobalConfigComponentAwakeSystem : AwakeSystem<GlobalConfigComponent>
    {
        public override void Awake(GlobalConfigComponent self)
        {
            GameObject config = (GameObject)ResourcesHelper.Load("KV");
            string configStr = config.Get<TextAsset>("GlobalProto").text;
            self.GlobalProto = JsonHelper.FromJson<GlobalProto>(configStr);
        }
    }
}