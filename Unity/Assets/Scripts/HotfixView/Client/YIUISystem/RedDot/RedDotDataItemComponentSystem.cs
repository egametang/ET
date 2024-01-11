using System;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    [FriendOf(typeof (RedDotDataItemComponent))]
    public static partial class RedDotDataItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RedDotDataItemComponent self)
        {
        }

        [EntitySystem]
        private static void Awake(this RedDotDataItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this RedDotDataItemComponent self)
        {
        }

        public static void RefreshData(this RedDotDataItemComponent self, RedDotData data)
        {
            self.m_Data        = data;
            self.u_DataCount.SetValue(data.Count);
            self.u_DataName.SetValue(RedDotMgr.Inst.GetKeyDes(data.Key));
            self.u_DataTips.SetValue(data.Tips);
            self.u_DataKeyId.SetValue((int)data.Key);
            self.u_DataParentCount.SetValue(data.ParentList.Count);
            self.u_DataChildCount.SetValue(data.ChildList.Count);
            self.u_DataSwitchTips.SetValue(data.Config.SwitchTips);
        }

        #region YIUIEvent开始

        private static void OnEventTipsAction(this RedDotDataItemComponent self, bool p1)
        {
            RedDotMgr.Inst.SetTips(self.m_Data.Key, p1);
        }

        private static void OnEventParentAction(this RedDotDataItemComponent self)
        {
            self.Fiber().UIEvent(new OnClickParentListEvent() { Data = self.m_Data }).Coroutine();
        }

        private static void OnEventClickItemAction(this RedDotDataItemComponent self)
        {
            self.Fiber().UIEvent(new OnClickItemEvent { Data = self.m_Data }).Coroutine();
        }

        private static void OnEventChildAction(this RedDotDataItemComponent self)
        {
            self.Fiber().UIEvent(new OnClickChildListEvent { Data = self.m_Data }).Coroutine();
        }
        #endregion YIUIEvent结束
    }
}