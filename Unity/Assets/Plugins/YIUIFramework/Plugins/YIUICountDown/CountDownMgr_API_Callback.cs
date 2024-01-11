//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using UnityEngine;

namespace YIUIFramework
{
    public partial class CountDownMgr
    {
        private bool RemoveByData(CountDownData data)
        {
            return m_CallbackGuidDic.Remove(data.TimerCallback);
        }

        private bool TryAddCallback(TimerCallback timerCallback)
        {
            if (m_CallbackGuidDic.ContainsKey(timerCallback))
            {
                Debug.LogError($"当前回调已存在 不能重复添加 如果想重复添加请使用另外一个API 使用GUID");
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// 移除一个回调
        /// </summary>
        public bool Remove(TimerCallback timerCallback)
        {
            if (!m_CallbackGuidDic.ContainsKey(timerCallback))
            {
                return false;
            }

            var callbackGuid = m_CallbackGuidDic[timerCallback];
            return Remove(callbackGuid);
        }

        /// <summary>
        /// 添加一个回调
        /// </summary>
        /// <param name="timerCallback">回调方法</param>
        /// <param name="totalTime">总时间</param>
        /// <param name="interval">间隔</param>
        /// <param name="startCallback">是否立即回调一次</param>
        /// <returns>是否添加成功</returns>
        public bool Add(TimerCallback timerCallback, double totalTime, double interval, bool startCallback = false)
        {
            if (timerCallback == null)
            {
                Logger.LogError($"<color=red> 必须有callback </color>");
                return false;
            }

            if (!TryAddCallback(timerCallback))
            {
                return false;
            }
            
            var callbackGuid = 0;
            var result       = Add(ref callbackGuid, totalTime, interval, timerCallback, startCallback);
            if (result)
            {
                m_CallbackGuidDic.Add(timerCallback, callbackGuid);
            }

            return result;
        }

        /// <summary>
        /// 添加一个只有一次的回调
        /// </summary>
        public bool Add(TimerCallback timerCallback, double totalTime, bool startCallback = false)
        {
            if (timerCallback == null)
            {
                Logger.LogError($"<color=red> 必须有callback </color>");
                return false;
            }
            
            if (!TryAddCallback(timerCallback))
            {
                return false;
            }
            
            var callbackGuid = 0;
            var result       = Add(ref callbackGuid, totalTime, timerCallback, startCallback);
            if (result)
            {
                m_CallbackGuidDic.Add(timerCallback, callbackGuid);
            }

            return result;
        }

        /// <summary>
        /// 有设置是否循环的
        /// </summary>
        public bool Add(TimerCallback timerCallback,
                        double        totalTime,
                        double        interval,
                        bool          forever,
                        bool          startCallback)
        {
            if (timerCallback == null)
            {
                Logger.LogError($"<color=red> 必须有callback </color>");
                return false;
            }

            if (!TryAddCallback(timerCallback))
            {
                return false;
            }
            
            var callbackGuid = 0;
            var result       = Add(ref callbackGuid, totalTime, interval, timerCallback, forever, startCallback);
            if (result)
            {
                m_CallbackGuidDic.Add(timerCallback, callbackGuid);
            }

            return result;
        }

        /// <summary>
        /// 重新设置这个倒计时的已过去时间
        /// </summary>
        public bool SetElapseTime(TimerCallback timerCallback, double elapseTime)
        {
            if (!m_CallbackGuidDic.TryGetValue(timerCallback, out int guid))
                return false;

            return SetElapseTime(guid, elapseTime);
        }

        /// <summary>
        /// 获取一个倒计时剩余的倒计时时间
        /// </summary>
        public double GetRemainTime(TimerCallback timerCallback)
        {
            if (!m_CallbackGuidDic.TryGetValue(timerCallback, out int guid))
                return 0;

            return GetRemainTime(guid);
        }

        /// <summary>
        /// 强制执行 一个倒计时到最后时间
        /// </summary>
        public bool ForceToEndTime(TimerCallback timerCallback)
        {
            if (!m_CallbackGuidDic.TryGetValue(timerCallback, out int guid))
                return false;

            return ForceToEndTime(guid);
        }

        /// <summary>
        /// 让倒计时重新开始
        /// </summary>
        public bool Restart(TimerCallback timerCallback)
        {
            if (!m_CallbackGuidDic.TryGetValue(timerCallback, out int guid))
                return false;

            return Restart(guid);
        }
    }
}