using System;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2023.11.30
    /// Desc
    /// </summary>
    [FriendOf(typeof(CommonPanelComponent))]
    public static partial class CommonPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this CommonPanelComponent self)
        {
        }
        
        [EntitySystem]
        private static void Awake(this CommonPanelComponent self)
        {
        }
        
        [EntitySystem]
        private static void Destroy(this CommonPanelComponent self)
        {
        }
        
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this CommonPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }
        
        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}