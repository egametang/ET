using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    public static partial class YIUIMgrComponentSystem
    {
        #region 永久屏蔽 forever

        //永久屏蔽
        //适用于 不知道要屏蔽多久 但是能保证可以成对调用的
        //这个没有放到API类中
        //因为如果你不能保证请不要用
        //至少过程中要try起来finally 保证不会出错 否则请不要使用这个功能
        public static int BanLayerOptionForever(this YIUIMgrComponent self)
        {
            self.SetLayerBlockOption(false);
            var foreverBlockCode = IDHelper.GetGuid();
            self.m_AllForeverBlockCode.Add(foreverBlockCode);
            return foreverBlockCode;
        }

        //恢复永久屏蔽
        public static void RecoverLayerOptionForever(this YIUIMgrComponent self, int code)
        {
            if (!self.m_AllForeverBlockCode.Contains(code))
            {
                return;
            }

            self.m_AllForeverBlockCode.Remove(code);

            if (!self.IsForeverBlock)
            {
                //如果当前有其他倒计时 就等待倒计时恢复
                //否则可直接恢复
                if (self.m_LastCountDownGuid == 0)
                {
                    self.SetLayerBlockOption(true);
                }
            }
        }

        #endregion

        //如果当前被屏蔽了操作 可以拿到还有多久操作会恢复
        public static float GetLastRecoverOptionResidueTime(this YIUIMgrComponent self)
        {
            if (self.CanLayerBlockOption)
            {
                return 0;
            }

            return self.LastRecoverOptionTime - Time.unscaledTime;
        }

        /// <summary>
        /// 禁止层级操作
        /// </summary>
        /// <param name="time">需要禁止的时间</param>
        public static void BanLayerOption(this YIUIMgrComponent self, float time = 1f)
        {
            self.BanLayerOptionCountDown(time);
        }

        /// <summary>
        /// 强制恢复层级到可操作状态
        /// 此方法会强制打断倒计时 
        /// 清除所有永久屏蔽
        /// 根据需求调用
        /// </summary>
        public static void RecoverLayerOption(this YIUIMgrComponent self)
        {
            self.RecoverLayerOptionAll();
        }
    }
}