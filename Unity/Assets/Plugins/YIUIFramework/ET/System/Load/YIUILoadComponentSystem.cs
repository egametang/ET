//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace ET.Client
{
    /// <summary>
    /// UI面板组件
    /// </summary>
    [FriendOf(typeof (YIUILoadComponent))]
    [EntitySystemOf(typeof (YIUILoadComponent))]
    public static partial class YIUILoadComponentSystem
    {
        [EntitySystem]
        private static void Awake(this YIUILoadComponent self)
        {
            self.Awake();
        }

        [EntitySystem]
        private static void Awake(this YIUILoadComponent self, string packageName)
        {
            self.Awake(packageName);
        }

        [EntitySystem]
        private static void Destroy(this YIUILoadComponent self)
        {
            self.Destroy();
        }
    }
}