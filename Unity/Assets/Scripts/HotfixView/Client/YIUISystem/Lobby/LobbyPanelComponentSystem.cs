using System;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    [FriendOf(typeof(LobbyPanelComponent))]
    public static partial class LobbyPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Awake(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LobbyPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        private static async ETTask OnEventEnterAction(this LobbyPanelComponent self)
        {
            var banId = YIUIMgrComponent.Inst.BanLayerOptionForever();
            await EnterMapHelper.EnterMapAsync(self.Root());
            YIUIMgrComponent.Inst.RecoverLayerOptionForever(banId);
            self.UIPanel.Close(false,true);
        }
        
        #endregion YIUIEvent结束
    }
}