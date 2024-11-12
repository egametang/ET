using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 条件管理器   
    /// </summary>
    [FriendOf(typeof(ConditionMgr))]
    public static partial class ConditionMgrSystem
    {
        #region 单个条件判断

        /// <summary>
        /// 单数据判断
        /// </summary>
        /// <param name="self"></param>
        /// <param name="conditionId">条件表ID</param>
        /// <param name="checkValue"> null的时候使用配置挡里面的 否则使用传入检查值 但是一定要知道每个值的意思 少传了传错意思了都会错 所以尽量不要手动传值 除非你清楚</param>
        /// <returns></returns>
        public static async ETTask<(bool result, string errorTips)> CheckCondition(this ConditionMgr self, EConditionId conditionId, ConditionCheckValue checkValue = null)
        {
            var conditionCfg = ConditionConfigCategory.Instance.GetOrDefault(conditionId);
            if (conditionCfg == null)
            {
                Log.Error($"没有找到条件Condition配置 {conditionId}");
                return (false, "");
            }

            return await self.CheckCondition(conditionCfg, checkValue);
        }

        /// <summary>
        /// 单数据判断
        /// </summary>
        /// <param name="self"></param>
        /// <param name="conditionCfg">条件表配置</param>
        /// <param name="checkValue"> null的时候使用配置挡里面的 否则使用传入检查值 要匹配你检查的条件需要的值类型 </param>
        /// <returns></returns>
        public static async ETTask<(bool result, string errorTips)> CheckCondition(this ConditionMgr self, ConditionConfig conditionCfg, ConditionCheckValue checkValue = null)
        {
            if (conditionCfg == null)
            {
                Log.Error($"conditionCfg == null");
                return (false, "");
            }

            if (!self.m_Conditions.ContainsKey(conditionCfg.ConditionType))
            {
                Log.Error($"错误1 Condition配置ID: {conditionCfg.Id}  条件类型: {conditionCfg.ConditionType} 没有找到实现");
                return (false, "");
            }

            var conditionInfo = self.m_Conditions[conditionCfg.ConditionType];
            if (conditionInfo?.Condition == null)
            {
                Log.Error($"错误2 Condition配置ID: {conditionCfg.Id}  条件类型: {conditionCfg.ConditionType} 没有找到实现");
                return (false, "");
            }

            ConditionCheckValue useCheckValue = null;

            //使用动态参数 则需要外部传入 否则使用配置挡里面的
            if (conditionCfg.DynamicCondition)
            {
                if (checkValue == null)
                {
                    Log.Error($"错误3 Condition配置ID: {conditionCfg.Id}  条件类型: {conditionCfg.ConditionType} 要求使用动态参数 但是未传入checkValue判断值");
                    return (false, "");
                }

                useCheckValue = checkValue;
            }
            else
            {
                useCheckValue = conditionCfg.CheckValue;
            }

            if (conditionInfo.CheckValueType != useCheckValue.GetType())
            {
                Log.Error($"错误4 Condition配置ID: {conditionCfg.Id}  条件类型: {conditionCfg.ConditionType} 条件参数类型 需求:{conditionInfo.CheckValueType} 传入{useCheckValue.GetType()}");
                return (false, "");
            }

            return await conditionInfo.Condition.Check(self.Scene(), conditionCfg, useCheckValue);
        }

        //由检查数据构建的检查结构 由配置表提供的数据
        public static async ETTask<(bool result, string errorTips)> CheckCondition(this ConditionMgr self, ConditionCheckData conditionCheckData)
        {
            return await self.CheckCondition(conditionCheckData.Id, conditionCheckData.CheckValue);
        }

        //由检查数据构建的检查结构 由配置表提供的数据多个检查数据时 默认使用与
        public static async ETTask<(bool result, string errorTips)> CheckCondition(this ConditionMgr self, List<ConditionCheckData> conditionCheckDataList)
        {
            foreach (var conditionCheckData in conditionCheckDataList)
            {
                var (result, errorTips) = await self.CheckCondition(conditionCheckData.Id, conditionCheckData.CheckValue);

                //多个checkdata 检查 默认使用与判断 有一个错了就返回错误
                if (!result)
                {
                    return (false, errorTips);
                }
            }

            //没有错则返回成功
            return (true, "");
        }

        #endregion

        #region 组判断

        /// <summary>
        /// 根据配置的条件组ID进行判断
        /// 这个没有自由传入检查值的说法 都在配置中填好了
        /// </summary>
        /// <param name="self"></param>
        /// <param name="conditionGroupId">条件组ID</param>
        /// <returns></returns>
        public static async ETTask<(bool result, string errorTips)> CheckGroup(this ConditionMgr self, EConditionGroupId conditionGroupId)
        {
            var groupCfg = ConditionGroupConfigCategory.Instance.GetOrDefault(conditionGroupId);
            if (groupCfg == null)
            {
                Log.Error($"没有找到条件组ConditionGroup配置 {conditionGroupId}");
                return (false, "");
            }

            return await self.CheckGroup(groupCfg);
        }

        public static async ETTask<(bool result, string errorTips)> CheckGroup(this ConditionMgr self, ConditionGroupConfig groupCfg)
        {
            if (groupCfg.UseGroup)
            {
                return await self.CheckGroupByGroup(groupCfg);
            }
            else
            {
                return await self.CheckGroupByBase(groupCfg);
            }
        }

        //这个条件内的检查是条件ID
        private static async ETTask<(bool result, string errorTips)> CheckGroupByBase(this ConditionMgr self, ConditionGroupConfig groupCfg)
        {
            var orErrorTips    = "";
            var checkListCount = groupCfg.CheckList.Count;
            for (int i = 0; i < checkListCount; i++)
            {
                var (result, errorTips) = await self.CheckCondition(groupCfg.CheckList[i]);
                switch (groupCfg.OperationType)
                {
                    //全部都满足才返回true
                    case EOperatorType.And:
                        if (!result) return (false, errorTips);         //其中一个错了就直接返回
                        if (i >= checkListCount - 1) return (true, ""); //循环完都没有错的 就代表都对了
                        break;

                    //有一个满足就返回true
                    case EOperatorType.Or:
                        if (result) return (true, ""); //其中一个对了就直接返回
                        if (string.IsNullOrEmpty(orErrorTips))
                            orErrorTips = errorTips;
                        else
                            orErrorTips += $"或 {errorTips}"; //TODOLsy (或字有多语言)
                        if (i >= checkListCount - 1)
                            return (false, orErrorTips); //循环完都没有对的 就代表全错了
                        break;

                    //错误类型
                    default:
                        Log.Error($"未实现的条件组操作类型 {groupCfg.OperationType}");
                        return (false, "");
                }
            }

            Log.Error($"错误 条件组 {groupCfg.Id} 没有配置条件");
            return (false, "");
        }

        //这个条件组内的检查是组ID
        private static async ETTask<(bool result, string errorTips)> CheckGroupByGroup(this ConditionMgr self, ConditionGroupConfig groupCfg)
        {
            var orErrorTips    = "";
            var checkListCount = groupCfg.CheckGroups_Ref.Count;
            for (int i = 0; i < checkListCount; i++)
            {
                var (result, errorTips) = await self.CheckGroup(groupCfg.CheckGroups_Ref[i]);
                switch (groupCfg.OperationType)
                {
                    //全部都满足才返回true
                    case EOperatorType.And:
                        if (!result) return (false, errorTips);         //其中一个错了就直接返回
                        if (i >= checkListCount - 1) return (true, ""); //循环完都没有错的 就代表都对了
                        break;

                    //有一个满足就返回true
                    case EOperatorType.Or:
                        if (result) return (true, ""); //其中一个对了就直接返回
                        if (string.IsNullOrEmpty(orErrorTips))
                            orErrorTips = errorTips;
                        else
                            orErrorTips += $"或 {errorTips}"; //TODOLsy (或字有多语言)
                        if (i >= checkListCount - 1)
                            return (false, orErrorTips); //循环完都没有对的 就代表全错了
                        break;

                    //错误类型
                    default:
                        Log.Error($"未实现的条件组操作类型 {groupCfg.OperationType}");
                        return (false, "");
                }
            }

            Log.Error($"未知的错误 是不是使用的组套组套组... 但是最后不是条件的结构 {groupCfg.Id}");
            return (false, "");
        }

        #endregion
    }
}
