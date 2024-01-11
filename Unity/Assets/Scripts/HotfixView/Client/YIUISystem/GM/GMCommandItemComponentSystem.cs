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
    [FriendOf(typeof(GMCommandItemComponent))]
    public static partial class GMCommandItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this GMCommandItemComponent self)
        {
            self.GMParamLoop = new YIUILoopScroll<GMParamInfo, GMParamItemComponent>(self, self.u_ComParamLoop, self.GMParamRenderer);
        }
         
        [EntitySystem]
        private static void Awake(this GMCommandItemComponent self)
        {
        }
        
        [EntitySystem]
        private static void Destroy(this GMCommandItemComponent self)
        {
        }

        public static void ResetItem(this GMCommandItemComponent self, GMCommandComponent commandComponent, GMCommandInfo info)
        {
            self.m_CommandComponent = commandComponent;
            self.Info             = info;
            self.u_DataName.SetValue(info.GMName);
            self.u_DataDesc.SetValue(info.GMDesc);
            self.u_DataShowParamLoop.SetValue(info.ParamInfoList.Count >= 1);
            self.GMParamLoop.SetDataRefresh(info.ParamInfoList);
            self.WaitRefresh().Coroutine();
        }

        private static async ETTask WaitRefresh(this GMCommandItemComponent self)
        {            
            await self.Fiber().Root.GetComponent<TimerComponent>().WaitAsync(500);
            self.GMParamLoop.RefreshCells();
        }
        
        private static void GMParamRenderer(this GMCommandItemComponent self, int index, GMParamInfo data, GMParamItemComponent item, bool select)
        {
            item.ResetItem(data);
        }
        
        #region YIUIEvent开始
        
        private static void OnEventRunAction(this GMCommandItemComponent self)
        {
            self.CommandComponent?.Run(self.Info).Coroutine();
        }
        #endregion YIUIEvent结束
    }
}