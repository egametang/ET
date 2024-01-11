using System;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  Lsy
    /// Date    2023.9.19
    /// Desc
    /// </summary>
    [FriendOf(typeof(LoginPanelComponent))]
    public static partial class LoginPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Awake(this LoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LoginPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        private static void OnEventPasswordAction(this LoginPanelComponent self, string p1)
        {
            Log.Info($"当前密码: {p1}");
            self.Password = p1;
        }
        
        private static void OnEventAccountAction(this LoginPanelComponent self, string p1)
        {
            Log.Info($"当前账号: {p1}");
            self.Account = p1;
        }
        
        private static async ETTask OnEventLoginAction(this LoginPanelComponent self)
        {
            Log.Info($"登录");
            var banId = YIUIMgrComponent.Inst.BanLayerOptionForever();
            await LoginHelper.Login(self.Root(), self.Account, self.Password);
            YIUIMgrComponent.Inst.RecoverLayerOptionForever(banId);
        }
        #endregion YIUIEvent结束
    }
}