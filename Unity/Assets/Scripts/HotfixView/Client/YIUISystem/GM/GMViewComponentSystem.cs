using System;
using YIUIFramework;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof (GMViewComponent))]
    public static partial class GMViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this GMViewComponent self)
        {
            self.m_CommandComponent = self.Root().GetComponent<GMCommandComponent>();
            self.GMTypeName = self.CommandComponent.GMTypeName;
            self.GMTypeLoop = new YIUILoopScroll<EGMType, GMTypeItemComponent>(self, self.u_ComGMTypeLoop, self.GMTypeTitleRenderer);
            self.GMTypeLoop.SetOnClickInfo("u_EventSelect", self.OnClickTitle);
            self.GMTypeData = new List<EGMType>();
            foreach (EGMType gmType in Enum.GetValues(typeof (EGMType)))
            {
                self.GMTypeData.Add(gmType);
            }

            self.GMCommandLoop = new YIUILoopScroll<GMCommandInfo, GMCommandItemComponent>(self, self.u_ComGMCommandLoop, self.GMCommandRenderer);
        }
        
        [EntitySystem]
        private static void Awake(this GMViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this GMViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask YIUIEvent(this GMViewComponent self, OnGMEventClose message)
        {
            await self.UIView.CloseAsync();
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this GMViewComponent self)
        {
            if (self.Opened) return true;
            self.GMTypeLoop.ClearSelect();
            self.GMTypeLoop.SetDataRefresh(self.GMTypeData, 0);
            self.GMTypeLoop.RefreshCells();
            self.Opened = true;
            await ETTask.CompletedTask;
            return true;
        }

        private static void OnClickTitle(this GMViewComponent self, int index, EGMType data, GMTypeItemComponent item, bool select)
        {
            item.SelectItem(select);
            if (select)
            {
                self.SelectTitleRefreshCommand(data);
            }
        }

        private static void GMTypeTitleRenderer(this GMViewComponent self, int index, EGMType data, GMTypeItemComponent item, bool select)
        {
            var name = data.ToString();
            if (self.GMTypeName.TryGetValue(data.ToString(), out var typeName))
            {
                name = typeName;
            }

            item.ResetItem(name, data);
            item.SelectItem(select);
            if (select)
            {
                self.SelectTitleRefreshCommand(data);
            }
        }

        private static void SelectTitleRefreshCommand(this GMViewComponent self, EGMType data)
        {
            if (self.CommandComponent.AllCommandInfo.TryGetValue(data, out var commandInfoList))
            {
                self.GMCommandLoop.SetDataRefresh(commandInfoList);
            }
            else
            {
                self.GMCommandLoop.SetDataRefresh(new List<GMCommandInfo>());
            }
        }

        private static void GMCommandRenderer(this GMViewComponent self, int index, GMCommandInfo data, GMCommandItemComponent item, bool select)
        {
            item.ResetItem(self.CommandComponent, data);
        }

        #region YIUIEvent开始

        private static void OnEventCloseAction(this GMViewComponent self)
        {
            self.UIView.Close();
        }

        #endregion YIUIEvent结束
    }
}