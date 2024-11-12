using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 监听扩展    
    /// </summary>
    [FriendOf(typeof(ConditionMgr))]
    public static partial class ConditionMgrSystem
    {
        #region 创建

        private static ConditionListenerInfo Create(Entity handler, string invokeName, ConditionGroupConfig groupCfg)
        {
            var info = Create();
            info.Init(handler, invokeName, groupCfg);
            return info;
        }

        private static ConditionListenerInfo Create(Entity handler, string invokeName, ConditionConfig conditionCfg, ConditionCheckValue checkValue = null)
        {
            var info = Create();
            info.Init(handler, invokeName, conditionCfg, checkValue);
            return info;
        }

        private static ConditionListenerInfo Create(Entity handler, string invokeName, ConditionCheckData checkData)
        {
            var info = Create();
            info.Init(handler, invokeName);
            info.AddCheckData(checkData);
            return info;
        }

        private static ConditionListenerInfo Create(Entity handler, string invokeName, List<ConditionCheckData> checkDataList)
        {
            var info = Create();
            info.Init(handler, invokeName);
            foreach (var checkData in checkDataList)
            {
                info.AddCheckData(checkData);
            }

            return info;
        }

        private static ConditionListenerInfo Create()
        {
            var info = ObjectPool.Fetch<ConditionListenerInfo>();
            info.InstanceId = IdGenerater.Instance.GenerateInstanceId();
            return info;
        }

        #endregion

        #region 初始化

        //初始化添加数据
        private static void AddCheckData(this ConditionListenerInfo self, ConditionCheckData checkData)
        {
            var conditionCfg = ConditionConfigCategory.Instance.GetOrDefault(checkData.Id);
            if (conditionCfg == null)
            {
                Log.Error($"没有找到条件Condition配置 {checkData.Id}");
                return;
            }

            self.ConditionCfgList.Add(conditionCfg);
            self.CheckValueList.Add(checkData.CheckValue);
            self.ListenerTypeHash.Add(conditionCfg.ConditionType);
        }

        //初始化 单个回调
        private static void Init(this ConditionListenerInfo self, Entity handler, string invokeName)
        {
            self.IsGroup         = false;
            self.m_HandlerEntity = handler;
            self.InvokeName      = invokeName;
        }

        //初始化 单个
        private static void Init(this ConditionListenerInfo self, Entity handler, string invokeName, ConditionConfig conditionCfg, ConditionCheckValue checkValue = null)
        {
            self.IsGroup         = false;
            self.m_HandlerEntity = handler;
            self.InvokeName      = invokeName;
            self.ConditionCfgList.Add(conditionCfg);
            self.CheckValueList.Add(checkValue);
            self.ListenerTypeHash.Add(conditionCfg.ConditionType);
        }

        //初始化 组
        private static void Init(this ConditionListenerInfo self, Entity handler, string invokeName, ConditionGroupConfig groupCfg)
        {
            self.IsGroup         = true;
            self.m_HandlerEntity = handler;
            self.InvokeName      = invokeName;
            self.GroupCfg        = groupCfg;
            self.InitGroupListenerType(groupCfg);
        }

        private static void InitGroupListenerType(this ConditionListenerInfo self, ConditionGroupConfig groupCfg)
        {
            if (groupCfg.UseGroup)
                self.AddGroupListenerTypeByGroup(groupCfg);
            else
                self.AddGroupListenerTypeByBase(groupCfg);
        }

        private static void AddGroupListenerTypeByGroup(this ConditionListenerInfo self, ConditionGroupConfig groupCfg)
        {
            for (int i = 0; i < groupCfg.CheckGroups_Ref.Count; i++)
            {
                self.InitGroupListenerType(groupCfg.CheckGroups_Ref[i]);
            }
        }

        private static void AddGroupListenerTypeByBase(this ConditionListenerInfo self, ConditionGroupConfig groupCfg)
        {
            for (int i = 0; i < groupCfg.CheckList.Count; i++)
            {
                self.ListenerTypeHash.Add(groupCfg.CheckList[i].Id_Ref.ConditionType);
            }
        }

        #endregion

        #region 触发

        //正常触发判断条件
        private static async ETTask Trigger(this ConditionListenerInfo self)
        {
            if (self.IsDisposed) return;
            if (self.IsGroup)
                await self.TriggerGroup();
            else
                await self.TriggerBase();
        }

        //某些特殊条件下 无需判断直接返回false
        private static void TriggerFalse(this ConditionListenerInfo self)
        {
            if (self.IsDisposed) return;
            self.Trigger(false, "");
        }

        private static void Trigger(Entity handler, string invokeName, bool result, string errorTips)
        {
            try
            {
                YIUIInvokeSystem.Instance?.Invoke(handler, invokeName, result, errorTips);
            }
            catch (Exception e)
            {
                Log.Error($"触发条件回调发出错误 请检查:{e}");
            }
        }

        private static void Trigger(this ConditionListenerInfo self, bool result, string errorTips)
        {
            try
            {
                YIUIInvokeSystem.Instance?.Invoke(self.HandlerEntity, self.InvokeName, result, errorTips);
            }
            catch (Exception e)
            {
                Log.Error($"触发条件回调发出错误 请检查:{e}");
            }
        }

        private static async ETTask TriggerBase(this ConditionListenerInfo self)
        {
            var count = self.ConditionCfgList.Count;
            var (result, errorTips) = (true, "");
            for (int i = 0; i < count; i++)
            {
                var conditionCfg = self.ConditionCfgList[i];
                var checkValue   = self.CheckValueList[i];
                (result, errorTips) = await ConditionMgr.Instance.CheckCondition(conditionCfg, checkValue);
                if (!result) break;
            }

            self.Trigger(result, errorTips);
        }

        private static async ETTask TriggerGroup(this ConditionListenerInfo self)
        {
            var (result, errorTips) = await ConditionMgr.Instance.CheckGroup(self.GroupCfg);
            self.Trigger(result, errorTips);
        }

        #endregion
    }
}
