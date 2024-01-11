using System;
using YIUIFramework;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof (TextTipsViewComponent))]
    public static partial class TextTipsViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this TextTipsViewComponent self)
        {
        }

        [EntitySystem]
        private static void Awake(this TextTipsViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TextTipsViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this TextTipsViewComponent self, ParamVo vo)
        {
            await ETTask.CompletedTask;
            var content = vo.Get<string>();
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError($"TextTipsView 必须有消息内容 请检查");
                return false;
            }

            self.u_DataMessageContent.SetValue(content);
            self.PlayAnimation().Coroutine();
            return true;
        }

        private static async ETTask PlayAnimation(this TextTipsViewComponent self)
        {
            self.u_ComAnimation.Play(self.u_ComAnimation.clip.name);
            await self.Fiber().Root.GetComponent<TimerComponent>().WaitAsync((long)(self.u_ComAnimation.clip.length * 1000));
            await TipsHelper.CloseTipsView(self);
        }

        #region YIUIEvent开始

        #endregion YIUIEvent结束
    }
}