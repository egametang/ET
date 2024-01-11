//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using UnityEngine;

namespace YIUIFramework
{
    //ref回调索引  这个回调就可以一对多
    public partial class CountDownMgr
    {
        /// <summary>
        /// ref传递 成功移除 则会吧这个值改为 0
        /// 省去自己修改
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool Remove(ref int guid)
        {
            var result = Remove(guid);
            if (result)
                guid = 0;
            return result;
        }

        /// <summary>
        /// 添加一个回调  (之所以没有用callback 做K 因为我可以对一个回调做不同的间隔调用 才可以一对多)
        /// 这个倒计时如果到最后一次回调 会自动回收 不必手动回收  除非提前移除
        /// 注意 这个最后0秒肯定会回调一次 这个时候会无视间隔 如果你要确保间隔回调 请 总时间/间隔 = 整数
        /// </summary>
        /// <param name="guid">返回索引</param>
        /// <param name="totalTime">总时间 0 =无限</param>
        /// <param name="interval">间隔</param>
        /// <param name="timerCallback">回调</param>
        /// <param name="startCallback">开始的时候马上回调一次</param>
        /// <returns></returns>
        public bool Add(ref int       guid,
                        double        totalTime,
                        double        interval,
                        TimerCallback timerCallback,
                        bool          startCallback = false)
        {
            if (!TryAdd())
            {
                Debug.LogError("已经达到可同时倒计时上限 请确认");
                guid = 0;
                return false;
            }

            var newCountDownData = RefPool.Get<CountDownData>();
            newCountDownData.Reset(totalTime, interval, timerCallback);
            guid = AddCountDownTimer(newCountDownData);

            if (startCallback)
                Callback(newCountDownData);

            return true;
        }

        /// <summary>
        /// 不要索引的
        /// </summary>
        public bool Add(double totalTime, double interval, TimerCallback timerCallback, bool startCallback = false)
        {
            var guid = 0;
            return Add(ref guid, totalTime, interval, timerCallback, startCallback);
        }

        /// <summary>
        /// 添加一个只有一次的回调
        /// </summary>
        public bool Add(ref int guid, double totalTime, TimerCallback timerCallback, bool startCallback = false)
        {
            return Add(ref guid, totalTime, totalTime, timerCallback, startCallback);
        }

        /// <summary>
        /// 不要索引的 一次回调
        /// </summary>
        public bool Add(double totalTime, TimerCallback timerCallback, bool startCallback = false)
        {
            var guid = 0;
            return Add(ref guid, totalTime, totalTime, timerCallback, startCallback);
        }

        /// <summary>
        /// 有设置是否循环的
        /// </summary>
        public bool Add(ref int       guid,
                        double        totalTime,
                        double        interval,
                        TimerCallback timerCallback,
                        bool          forever,
                        bool          startCallback)
        {
            if (!TryAdd())
            {
                Debug.LogError("已经达到可同时倒计时上限 请确认");
                guid = 0;
                return false;
            }

            var newCountDownData = RefPool.Get<CountDownData>();
            newCountDownData.Reset(totalTime, interval, timerCallback, forever);
            guid = AddCountDownTimer(newCountDownData);

            if (startCallback)
                Callback(newCountDownData);

            return true;
        }

        /// <summary>
        /// 重新设置这个倒计时的已过去时间
        /// </summary>
        public bool SetElapseTime(int guid, double elapseTime)
        {
            var exist = m_AllCountDown.TryGetValue(guid, out CountDownData data);
            if (!exist)
                return false;

            data.ElapseTime       = elapseTime;
            data.LastCallBackTime = GetTime();
            return true;
        }

        /// <summary>
        /// 获取一个倒计时剩余的倒计时时间
        /// </summary>
        public double GetRemainTime(int guid)
        {
            var exist = m_AllCountDown.TryGetValue(guid, out CountDownData data);
            if (!exist)
                return 0;

            return data.TotalTime - data.ElapseTime;
        }

        /// <summary>
        /// 强制执行 一个倒计时到最后时间
        /// </summary>
        public bool ForceToEndTime(int guid)
        {
            var exist = m_AllCountDown.TryGetValue(guid, out CountDownData data);
            if (!exist)
                return false;

            data.ElapseTime = data.TotalTime;
            Callback(data);

            return Remove(guid);
        }

        /// <summary>
        /// 让倒计时重新开始
        /// </summary>
        public bool Restart(int guid)
        {
            var exist = m_AllCountDown.TryGetValue(guid, out CountDownData data);
            if (!exist)
                return false;

            return Restart(data);
        }
    }
}