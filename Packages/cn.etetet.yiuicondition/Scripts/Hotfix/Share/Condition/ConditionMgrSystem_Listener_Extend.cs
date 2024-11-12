using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 条件监听管理器  
    /// </summary>
    [FriendOf(typeof(ConditionMgr))]
    public static partial class ConditionMgrSystem
    {
        #region 对外可使用的 API

        #region 单条件

        /// <summary>
        /// 添加一个条件监听
        /// </summary>
        /// <param name="self"></param>
        /// <param name="handler">监听者</param>
        /// <param name="invokeName">变化时的回调</param>
        /// <param name="conditionCfg">条件配置</param>
        /// <param name="checkValue">如果是动态数据时 需要传入的数据</param>
        /// <param name="immediatelyTrigger">默认监听时立即执行一次</param>
        /// <returns></returns>
        public static long AddCheckConditionListener(this ConditionMgr self, Entity handler, string invokeName, ConditionConfig conditionCfg, ConditionCheckValue checkValue = null, bool immediatelyTrigger = true)
        {
            if (!self.CheckCanListener(conditionCfg))
            {
                if (immediatelyTrigger)
                    self.TriggerNotListener(handler, invokeName, conditionCfg, checkValue).NoContext();
                else
                    Log.Error($"条件 {conditionCfg.Id} 因为没有任何监听通知 所以永远不会变 且关闭了立即触发 所以永远没有被触发过 请注意");
                return 0;
            }

            var listenerInfo = Create(handler, invokeName, conditionCfg, checkValue);
            if (immediatelyTrigger)
                listenerInfo.Trigger().NoContext();
            self.AddListener(listenerInfo);
            return listenerInfo.InstanceId;
        }

        public static long AddCheckConditionListener(this ConditionMgr self, Entity handler, string invokeName, EConditionId conditionId, ConditionCheckValue checkValue = null, bool immediatelyTrigger = true)
        {
            var conditionCfg = ConditionConfigCategory.Instance.GetOrDefault(conditionId);
            if (conditionCfg == null)
            {
                Log.Error($"没有找到条件Condition配置 {conditionId}");
                return 0;
            }

            return self.AddCheckConditionListener(handler, invokeName, conditionCfg, checkValue, immediatelyTrigger);
        }

        #region 通过检查数据的监听

        public static long AddCheckConditionListener(this ConditionMgr self, Entity handler, string invokeName, ConditionCheckData checkData, bool immediatelyTrigger = true)
        {
            var conditionCfg = ConditionConfigCategory.Instance.GetOrDefault(checkData.Id);
            if (conditionCfg == null)
            {
                Log.Error($"没有找到条件Condition配置 {checkData.Id}");
                return 0;
            }

            if (!self.CheckCanListener(conditionCfg))
            {
                if (immediatelyTrigger)
                    self.TriggerNotListener(handler, invokeName, conditionCfg, checkData.CheckValue).NoContext();
                else
                    Log.Error($"条件 {conditionCfg.Id} 因为没有任何监听通知 所以永远不会变 且关闭了立即触发 所以永远没有被触发过 请注意");
                return 0;
            }

            var listenerInfo = Create(handler, invokeName, checkData);
            if (immediatelyTrigger)
                listenerInfo.Trigger().NoContext();
            self.AddListener(listenerInfo);
            return listenerInfo.InstanceId;
        }

        public static long AddCheckConditionListener(this ConditionMgr self, Entity handler, string invokeName, List<ConditionCheckData> checkDataList, bool immediatelyTrigger = true)
        {
            foreach (var checkData in checkDataList)
            {
                var conditionCfg = ConditionConfigCategory.Instance.GetOrDefault(checkData.Id);
                if (conditionCfg == null)
                {
                    Log.Error($"没有找到条件Condition配置 {checkData.Id}");
                    return 0;
                }

                if (!self.CheckCanListener(conditionCfg))
                {
                    if (immediatelyTrigger)
                        self.TriggerNotListener(handler, invokeName, checkDataList).NoContext();
                    else
                        Log.Error($"条件 {conditionCfg.Id} 因为没有任何监听通知 所以永远不会变 且关闭了立即触发 所以永远没有被触发过 请注意");
                    return 0;
                }
            }

            var listenerInfo = Create(handler, invokeName, checkDataList);
            if (immediatelyTrigger)
                listenerInfo.Trigger().NoContext();
            self.AddListener(listenerInfo);
            return listenerInfo.InstanceId;
        }

        #endregion

        #endregion

        #region 多条件

        /// <summary>
        /// 添加一个条件组监听
        /// </summary>
        /// <param name="self"></param>
        /// <param name="handler">监听者</param>
        /// <param name="invokeName">变化时的回调</param>
        /// <param name="groupCfg">条件配置</param>
        /// <param name="immediatelyTrigger">默认监听时立即执行一次</param>
        /// <returns></returns>
        public static long AddCheckConditionGroupListener(this ConditionMgr self, Entity handler, string invokeName, ConditionGroupConfig groupCfg, bool immediatelyTrigger = true)
        {
            if (!self.CheckGroupCanListener(groupCfg))
            {
                if (immediatelyTrigger)
                    Trigger(handler, invokeName, false, "");
                return 0;
            }

            if (!self.CheckGroupCanListener(groupCfg))
            {
                if (immediatelyTrigger)
                    self.TriggerGroupNotListener(handler, invokeName, groupCfg).NoContext();
                else
                    Log.Error($"条件组 {groupCfg.Id} 因为没有任何监听通知 所以永远不会变 且关闭了立即触发 所以永远没有被触发过 请注意");
                return 0;
            }

            var listenerInfo = Create(handler, invokeName, groupCfg);
            if (immediatelyTrigger)
                listenerInfo.Trigger().NoContext();
            self.AddListener(listenerInfo);
            return listenerInfo.InstanceId;
        }

        public static long AddCheckConditionGroupListener(this ConditionMgr self, Entity handler, string invokeName, EConditionGroupId conditionGroupId, bool immediatelyTrigger = true)
        {
            var groupCfg = ConditionGroupConfigCategory.Instance.GetOrDefault(conditionGroupId);
            if (groupCfg == null)
            {
                Log.Error($"没有找到条件组ConditionGroup配置 {conditionGroupId}");
                return 0;
            }

            return self.AddCheckConditionGroupListener(handler, invokeName, groupCfg, immediatelyTrigger);
        }

        #endregion

        //移除监听
        public static void RemoveCheckConditionListener(this ConditionMgr self, long instanceId)
        {
            self?.RemoveListener(instanceId);
        }

        #region 私有

        //无法监听时 如果有立即执行 则帮他触发一次
        private static async ETTask TriggerNotListener(this ConditionMgr self, Entity handler, string invokeName, ConditionConfig conditionCfg, ConditionCheckValue checkValue = null)
        {
            var (result, errorTips) = await self.CheckCondition(conditionCfg, checkValue);

            try
            {
                Trigger(handler, invokeName, result, errorTips);
            }
            catch (Exception e)
            {
                Log.Error($"触发条件回调发出错误 请检查:{e}");
            }
        }

        //无法监听时 如果有立即执行 则帮他触发一次
        private static async ETTask TriggerNotListener(this ConditionMgr self, Entity handler, string invokeName, List<ConditionCheckData> checkDataList)
        {
            var (result, errorTips) = await self.CheckCondition(checkDataList);

            try
            {
                Trigger(handler, invokeName, result, errorTips);
            }
            catch (Exception e)
            {
                Log.Error($"触发条件回调发出错误 请检查:{e}");
            }
        }

        //无法监听时 如果有立即执行 则帮他触发一次
        private static async ETTask TriggerGroupNotListener(this ConditionMgr self, Entity handler, string invokeName, ConditionGroupConfig groupCfg)
        {
            var (result, errorTips) = await self.CheckGroup(groupCfg);

            try
            {
                Trigger(handler, invokeName, result, errorTips);
            }
            catch (Exception e)
            {
                Log.Error($"触发条件回调发出错误 请检查:{e}");
            }
        }

        //检查组是否可以监听
        private static bool CheckGroupCanListener(this ConditionMgr self, ConditionGroupConfig groupCfg)
        {
            if (groupCfg == null)
            {
                Log.Error($"配置错误 groupCfg == null");
                return false;
            }

            var result = groupCfg.UseGroup ? self.CheckGroupCanListenerByGroup(groupCfg) : self.CheckGroupCanListenerByBase(groupCfg);

            if (!result)
            {
                Log.Error($"整个组内所有条件都是不能监听的 所以此监听永久无法被触发 请注意 {groupCfg.Id}");
            }

            return result;
        }

        //区分上面 方便报错用
        private static bool CheckGroupCanListener2(this ConditionMgr self, ConditionGroupConfig groupCfg)
        {
            if (groupCfg == null)
            {
                Log.Error($"配置错误 groupCfg == null");
                return false;
            }

            return groupCfg.UseGroup ? self.CheckGroupCanListenerByGroup(groupCfg) : self.CheckGroupCanListenerByBase(groupCfg);
        }

        private static bool CheckGroupCanListenerByGroup(this ConditionMgr self, ConditionGroupConfig groupCfg)
        {
            var checkListCount = groupCfg.CheckGroups_Ref.Count;
            for (int i = 0; i < checkListCount; i++)
            {
                //只要有其中一个可以监听 那么代表整条都可以监听
                if (self.CheckGroupCanListener2(groupCfg.CheckGroups_Ref[i]))
                {
                    return true;
                }
            }

            //如果整个组都是不能监听的 那么代表整条都不能监听
            return false;
        }

        private static bool CheckGroupCanListenerByBase(this ConditionMgr self, ConditionGroupConfig groupCfg)
        {
            for (int i = 0; i < groupCfg.CheckList.Count; i++)
            {
                //只要有其中一个可以监听 那么代表整条都可以监听
                if (self.CheckCanListener(groupCfg.CheckList[i].Id_Ref))
                {
                    return true;
                }
            }

            //如果整个组都是不能监听的 那么代表整条都不能监听
            return false;
        }

        //检查是否可以监听
        private static bool CheckCanListener(this ConditionMgr self, ConditionConfig conditionCfg)
        {
            if (conditionCfg == null)
            {
                Log.Error($"配置错误 conditionCfg == null");
                return false;
            }

            if (!conditionCfg.Listener)
            {
                Log.Error($"这个条件不允许监听 所以请不要使用监听 请自行判断 {conditionCfg.Id}");
                return false;
            }

            if (!self.m_Conditions.TryGetValue(conditionCfg.ConditionType, out var conditionInfo))
            {
                Log.Error($"没有找到条件ConditionType {conditionCfg.ConditionType} 没有实现");
                return false;
            }

            if (!conditionInfo.Listener)
            {
                Log.Error($"这个条件没有实现监听通知 所以请不要使用监听 请自行判断 {conditionCfg.Id} 或者先实现监听通知");
                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region 私有

        #region 内部添加移除

        private static void AddListener(this ConditionMgr self, ConditionListenerInfo info)
        {
            if (self.m_IsTrigger)
            {
                if (!self.m_AddConditionHash.Add(info))
                    Log.Error($"重复添加监听器，instanceId：{info.InstanceId}");
            }
            else
            {
                self.AddListenerByInfo(info);
            }
        }

        private static void AddListenerByInfo(this ConditionMgr self, ConditionListenerInfo info)
        {
            if (!self.m_AllConditionListenerInfos.TryAdd(info.InstanceId, info))
            {
                Log.Error($"重复添加监听器，instanceId：{info.InstanceId}");
                return;
            }

            foreach (var listenerType in info.ListenerTypeHash)
            {
                if (!self.m_ClassifyConditionListenerInfos.ContainsKey(listenerType))
                    self.m_ClassifyConditionListenerInfos.Add(listenerType, new Dictionary<long, ConditionListenerInfo>());
                var listenerDic = self.m_ClassifyConditionListenerInfos[listenerType];
                if (!listenerDic.TryAdd(info.InstanceId, info))
                    Log.Error($"分类中 重复添加监听器:{info.InstanceId}");
            }
        }

        private static void AddListenerByHash(this ConditionMgr self)
        {
            foreach (var info in self.m_AddConditionHash)
            {
                self.AddListenerByInfo(info);
            }

            self.m_AddConditionHash.Clear();
        }

        private static void RemoveListener(this ConditionMgr self, long instanceId)
        {
            if (self.m_IsTrigger)
            {
                if (self.m_AllConditionListenerInfos.TryGetValue(instanceId, out var info))
                {
                    info.Disposed();
                    self.m_RemoveConditionHashSet.Add(instanceId);
                }
            }
            else
                self.RemoveListenerById(instanceId);
        }

        private static void RemoveListenerById(this ConditionMgr self, long instanceId)
        {
            if (!self.m_AllConditionListenerInfos.Remove(instanceId, out var info)) return;

            foreach (var listenerType in info.ListenerTypeHash)
            {
                if (!self.m_ClassifyConditionListenerInfos.ContainsKey(listenerType)) continue;
                self.m_ClassifyConditionListenerInfos[listenerType].Remove(info.InstanceId);
            }

            info.Dispose();
        }

        private static void RemoveListenerByHash(this ConditionMgr self)
        {
            foreach (var instanceId in self.m_RemoveConditionHashSet)
            {
                self.RemoveListenerById(instanceId);
            }

            self.m_RemoveConditionHashSet.Clear();
        }

        #endregion

        #region 触发监听

        //触发监听
        //由各个模块监听实现 个人 非必要不要主动调用
        public static void TriggerListener(this ConditionMgr self, EConditionType triggerType)
        {
            if (!self.m_Conditions.TryGetValue(triggerType, out var conditionInfo))
            {
                Log.Error($"没有找到条件ConditionType {triggerType} 没有实现");
                return;
            }

            if (!conditionInfo.Listener)
            {
                Log.Error($"这个条件 {triggerType} 不允许监听通知 所以请不要触发 请检查");
                return;
            }

            self.TriggerListenerEnter(triggerType).NoContext();
        }

        private static async ETTask TriggerListenerEnter(this ConditionMgr self, EConditionType triggerType)
        {
            if (self.m_IsTrigger)
            {
                self.m_ConditionQueue.Enqueue(triggerType);
            }
            else
            {
                self.m_IsTrigger = true;
                await self.TriggerListenerByType(triggerType);
                await self.TriggerConditionQueue();
                self.AddListenerByHash();
                self.RemoveListenerByHash();
                self.m_IsTrigger = false;
            }
        }

        private static async ETTask TriggerListenerByType(this ConditionMgr self, EConditionType triggerType)
        {
            if (!self.m_ClassifyConditionListenerInfos.TryGetValue(triggerType, out var infoDic)) return;

            foreach (var info in infoDic.Values)
            {
                await info.Trigger();
            }
        }

        private static async ETTask TriggerConditionQueue(this ConditionMgr self)
        {
            var queueCount = self.m_ConditionQueue.Count;
            if (queueCount <= 0) return;
            for (int i = 0; i < queueCount; i++)
            {
                var triggerType = self.m_ConditionQueue.Dequeue();
                await self.TriggerListenerByType(triggerType);
            }

            await self.TriggerConditionQueue();
        }

        #endregion

        #endregion
    }
}
