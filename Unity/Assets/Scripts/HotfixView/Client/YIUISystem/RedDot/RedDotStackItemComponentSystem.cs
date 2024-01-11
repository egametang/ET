using System;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    [FriendOf(typeof(RedDotStackItemComponent))]
    public static partial class RedDotStackItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RedDotStackItemComponent self)
        {
        }

        [EntitySystem]
        private static void Awake(this RedDotStackItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this RedDotStackItemComponent self)
        {
        }

        #region YIUIEvent开始
        
        private static void OnEventShowStackAction(this RedDotStackItemComponent self)
        {
            self.u_ComStackText.text = self.RedDotStackData?.GetStackContent() ?? "";
        }
        #endregion YIUIEvent结束
    }
}