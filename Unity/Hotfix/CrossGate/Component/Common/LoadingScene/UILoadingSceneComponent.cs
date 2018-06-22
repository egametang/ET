using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class UILoadingSceneComponentSystem : AwakeSystem<UILoadingSceneComponent, string>
    {
        public override void Awake(UILoadingSceneComponent self, string type)
        {
            self.MyType = type;
            self.Awake();
        }
    }

    public class UILoadingSceneComponent : UIBaseComponent
    {
        //区分倒计时自动销毁标记
        public string MyType;

        public void Awake()
        {
            this.Type = this.MyType;
        }
    }
}
