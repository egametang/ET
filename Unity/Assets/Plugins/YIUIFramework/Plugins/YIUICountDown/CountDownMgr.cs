//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System.Collections.Generic;
using ET;
using UnityEngine;

namespace YIUIFramework
{
    
    public static class CountDownMgrSystem
    {
        [EntitySystem]
        public class CountDownMgr_AwakeSystem: AwakeSystem<CountDownMgr>
        {
            protected override void Awake(CountDownMgr self)
            {
                CountDownMgr.Inst = self;
            }
        }
        
        [EntitySystem]
        public class CountDownMgr_LateUpdateSystem: LateUpdateSystem<CountDownMgr>
        {
            protected override void LateUpdate(CountDownMgr self)
            {
                self.LateUpdate();
            }
        }
    }
    
    /// <summary>
    /// 倒计时管理器
    /// 区别于Times 个人认为更适合UI上的时间倒计时
    ///                            Times                   CountDown
    /// 回调频率                     不可改                     可改                        (虽然中途改频率这个事情很少)
    /// 如果暂停                中间丢失的时间就没了      中途丢失的时间会快速倒计时             (万一有需求 中间的各种计算就丢掉了)
    /// 添加时可立即调用一次      否(还需要自己调一次)          可传参数控制                     (很多时候倒计时都需要第一时间刷新一次的)
    /// 一对多                        否                         是                         (因为用Callback做K 就没办法在同一个Callback下 被别人倒计时)
    /// 可提前结束                     否                         是                         (针对于 比如 匿名函数 等特殊情况)
    /// 回调参数            obj 但是麻烦 而且不可变           已过去时间/总时间                   (更适合于UI上的一些数字倒计时)
    /// 可循环                        否                          是                         (虽然0 都可以无限 但是万一要的是不是0的情况下循环呢 就得递归调自己吗)
    /// 多重载                        否                          是                         (满足各种需求)
    /// 匿名函数                      否                          是                          (匿名函数也可以被暂停 移除等操作)
    /// ......
    /// </summary>
    public partial class CountDownMgr: Entity, IAwake, ILateUpdate
    {
        public static CountDownMgr Inst;
        
        /// <summary>
        /// 回调方法
        /// </summary>
        /// <param name="residueTime">剩余时间</param>
        /// <param name="elapseTime">已过去时间</param>
        /// <param name="totalTime">总时间</param>
        public delegate void TimerCallback(double residueTime, double elapseTime, double totalTime);

        /// <summary>
        /// 所有需要被倒计时的目标
        /// 这个可以一对多
        /// </summary>
        private Dictionary<int, CountDownData> m_AllCountDown = new Dictionary<int, CountDownData>();

        /// <summary>
        /// 临时存储
        /// 下一帧添加的倒计时
        /// </summary>
        private Dictionary<int, CountDownData> m_ToAddCountDown = new Dictionary<int, CountDownData>();

        /// <summary>
        /// 所有需要被倒计时的目标
        /// 这个只能一对一
        /// </summary>
        private Dictionary<TimerCallback, int> m_CallbackGuidDic = new Dictionary<TimerCallback, int>();

        /// <summary>
        /// 临时存储
        /// 下一帧移除的倒计时
        /// </summary>
        private List<int> m_RemoveGuid = new List<int>();

        /// <summary>
        /// 可容纳的最大倒计时
        /// </summary>
        private int m_MaxCount = 1000;

        /// <summary>
        /// 当然已经存在的倒计时数量
        /// </summary>
        private int m_AtCount = 0;
        
        //统一所有取时间都用这个 且方便修改
        private static float GetTime()
        {
            //这是一个倒计时时间不受暂停影响的
            return Time.realtimeSinceStartup;
        }
        
        public void LateUpdate()
        {
            ManagerUpdate();
        }
        
        //为了不受mono暂停影响 所以使用异步调用
        public void ManagerUpdate()
        {
            var time = GetTime();

            //需要被添加的
            if (m_ToAddCountDown.Count >= 1)
            {
                foreach (var countDown in m_ToAddCountDown.Values)
                {
                    ToAddCountDownTimer(countDown);
                }

                m_ToAddCountDown.Clear();
            }

            //需要被移除的
            if (m_RemoveGuid.Count >= 1)
            {
                foreach (var guid in m_RemoveGuid)
                {
                    RemoveCountDownTimer(guid);
                }

                m_RemoveGuid.Clear();
            }

            //倒计时
            foreach (var data in m_AllCountDown.Values)
            {
                data.ElapseTime = time - data.StartTime;

                //已经用完时间
                if (data.TotalTime != 0 && data.ElapseTime >= data.TotalTime)
                {
                    data.ElapseTime = data.TotalTime;

                    Callback(data);

                    if (data.Forver)
                    {
                        Restart(data);
                    }
                    else
                    {
                        m_RemoveGuid.Add(data.Guid);
                    }
                }
                else if (time - data.LastCallBackTime >= data.Interval)
                {
                    //这样才可以确保如果卡了很久  该间隔回调的还是会回 不会遗漏
                    //但是如果一瞬间就到最后了 中途的还是会没有  因为这个倒计时类的需求就是这样的
                    data.LastCallBackTime += data.Interval;
                    Callback(data);
                }
            }
        }

        #region 私有

        /// <summary>
        /// 是否可添加倒计时   有同时倒计时上限限制
        /// </summary>
        /// <returns></returns>
        private bool TryAdd()
        {
            return !(m_AtCount >= m_MaxCount);
        }

        /// <summary>
        /// 获取当前索引
        /// </summary>
        /// <returns></returns>
        private int GetGuid()
        {
            return IDHelper.GetGuid();
        }

        //从缓存列表添加到运行列表
        private void ToAddCountDownTimer(CountDownData countDownData)
        {
            if (m_AllCountDown.ContainsKey(countDownData.Guid))
            {
                Logger.LogError($"<color=red> 添加的这个已经存在 :{countDownData.Guid}</color>");
                return;
            }

            m_AllCountDown.Add(countDownData.Guid, countDownData);
            m_AtCount++;
        }

        /// <summary>
        /// 添加一个到字典中
        /// </summary>
        private int AddCountDownTimer(CountDownData countDownData)
        {
            int guid = GetGuid();
            countDownData.Guid = guid;
            m_ToAddCountDown.Add(guid, countDownData);
            return guid;
        }

        private bool RemoveCountDownTimer(int guid)
        {
            if (guid == 0)
            {
                return true;
            }

            if (!m_AllCountDown.ContainsKey(guid))
            {
                return false;
            }

            var data = m_AllCountDown[guid];
            m_AllCountDown.Remove(guid);
            RemoveByData(data);
            RefPool.Put(data);
            m_AtCount--;

            return true;
        }

        /// <summary>
        /// 移除一个回调
        /// 根据需求也可使用 Remove(ref int guid)
        /// </summary>
        public bool Remove(int guid)
        {
            if (guid == 0)
            {
                return true;
            }

            //如果对方还在添加列表中 在移除列表中添加
            //在下一次update的时候 添加列表还是会先添加到all中
            //然后就会被移除 所以并不会被执行
            //最终所有移除的都是从all中移除的
            if (m_ToAddCountDown.ContainsKey(guid))
            {
                m_RemoveGuid.Add(guid);
                return true;
            }

            //如果正在执行中 则添加到移除中
            //下一次循环移除
            if (m_AllCountDown.ContainsKey(guid))
            {
                m_RemoveGuid.Add(guid);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 让倒计时重新开始
        /// </summary>
        private bool Restart(CountDownData data)
        {
            data.ElapseTime       = 0;
            data.LastCallBackTime = GetTime();
            data.StartTime        = data.LastCallBackTime;
            data.EndTime          = data.LastCallBackTime + data.TotalTime;
            return true;
        }

        private void Callback(int guid)
        {
            var exist = m_AllCountDown.TryGetValue(guid, out CountDownData data);
            if (exist)
                Callback(data);
        }

        private void Callback(CountDownData data)
        {
            data.TimerCallback?.Invoke(data.TotalTime - data.ElapseTime, data.ElapseTime, data.TotalTime);
        }

        private void Callback(CountDownData data, double elapseTime)
        {
            data.TimerCallback?.Invoke(data.TotalTime - elapseTime, elapseTime, data.TotalTime);
        }

        #endregion
        
    }
}