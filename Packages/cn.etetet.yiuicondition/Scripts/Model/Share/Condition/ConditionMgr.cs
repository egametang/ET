using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 条件管理
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ConditionMgr : Entity, IAwake, IDestroy
    {
        public Dictionary<EConditionType, ConditionInfo>                           m_Conditions                     = new(); //所有已反射的条件
        public Queue<EConditionType>                                               m_ConditionQueue                 = new(); //触发队列
        public Dictionary<long, ConditionListenerInfo>                             m_AllConditionListenerInfos      = new(); //所以已知需要被触发的
        public Dictionary<EConditionType, Dictionary<long, ConditionListenerInfo>> m_ClassifyConditionListenerInfos = new(); //分类后需要被触发的
        public HashSet<ConditionListenerInfo>                                      m_AddConditionHash               = new(); //需要被添加的缓存
        public HashSet<long>                                                       m_RemoveConditionHashSet         = new(); //需要被移除的缓存
        public bool                                                                m_IsTrigger                      = false; //当前正在触发中

        [StaticField]
        public static EntityRef<ConditionMgr> m_InstRef;

        [StaticField]
        public static ConditionMgr Instance => m_InstRef;
    }
}
