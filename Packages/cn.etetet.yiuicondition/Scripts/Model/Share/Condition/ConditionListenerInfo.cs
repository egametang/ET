using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 条件监听器信息
    /// </summary>
    [EnableClass]
    public class ConditionListenerInfo : IPool
    {
        #region 定义字段

        public long                      InstanceId       { get; set; }          //唯一ID
        public bool                      IsDisposed       { get; set; }          //已摧毁不响应
        public bool                      IsGroup          { get; set; }          //是否是组
        public ConditionGroupConfig      GroupCfg         { get; set; }          //组 配置
        public List<ConditionConfig>     ConditionCfgList { get; set; } = new(); //不是组下的配置
        public List<ConditionCheckValue> CheckValueList   { get; set; } = new(); //不是组下的检查表
        public HashSet<EConditionType>   ListenerTypeHash { get; set; } = new(); //需要监听的类型

        public EntityRef<Entity> m_HandlerEntity;
        public Entity            HandlerEntity => m_HandlerEntity;
        public string            InvokeName;

        #endregion

        #region 回收

        public void Disposed()
        {
            IsDisposed = true;
        }

        public void Dispose()
        {
            InstanceId      = 0;
            m_HandlerEntity = null;
            InvokeName      = null;
            IsDisposed      = false;
            IsGroup         = false;
            GroupCfg        = null;
            ConditionCfgList?.Clear();
            CheckValueList?.Clear();
            ListenerTypeHash?.Clear();
            ObjectPool.Recycle(this);
        }

        public bool IsFromPool { get; set; }

        #endregion
    }
}
