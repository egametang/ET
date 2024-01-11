//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace ET.Client
{
    [FriendOf(typeof (YIUIMgrComponent))]
    [EntitySystemOf(typeof (YIUIMgrComponent))]
    public static partial class YIUIMgrComponentSystem
    {
        [EntitySystem]
        private static void Awake(this YIUIMgrComponent self)
        {
            YIUIMgrComponent.Inst = self;
            self.InitAllBind();
            self.AddComponent<YIUIEventComponent>();
            self.AddComponent<YIUILoadComponent>();
        }

        [EntitySystem]
        private static void Destroy(this YIUIMgrComponent self)
        {
            self.OnBlockDispose();
        }
    }
}