//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// UI主体
    /// </summary>
    [FriendOf(typeof (YIUIComponent))]
    [EntitySystemOf(typeof (YIUIComponent))]
    public static partial class YIUIComponentSystem
    {
        [EntitySystem]
        private static void Awake(this YIUIComponent self, YIUIBindVo uiBindVo, GameObject obj)
        {
            self.InitUIBase(uiBindVo, obj);
        }

        [EntitySystem]
        private static void Destroy(this YIUIComponent self)
        {
            if (self.OwnerGameObject != null)
                UnityEngine.Object.Destroy(self.OwnerGameObject);
        }
    }
}