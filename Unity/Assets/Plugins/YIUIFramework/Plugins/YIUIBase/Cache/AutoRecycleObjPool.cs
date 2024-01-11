using System;
using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// 可以自动回收的对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutoRecycleObjPool<T>
    {
        private Action<T> m_clearCallback;
        private Func<T>   m_newCallback;

        private ITimeProvider m_timer;
        private int           m_lastUpdateTime;

        private List<PoolVo> m_uses;
        private List<PoolVo> m_frees;

        public AutoRecycleObjPool(ITimeProvider timer,
                                  Func<T>       newCallback,
                                  Action<T>     clearCallback)
        {
            m_timer         = timer;
            m_newCallback   = newCallback;
            m_clearCallback = clearCallback;

            m_uses  = new List<PoolVo>();
            m_frees = new List<PoolVo>();
        }

        public T Get()
        {
            Update();
            PoolVo result;
            if (m_frees.Count > 0)
            {
                result = m_frees.Pop();
            }
            else
            {
                result       = new PoolVo();
                result.Value = m_newCallback();
            }

            result.Timeout = m_timer.Time + 1;
            m_uses.Add(result);
            return result.Value;
        }

        public void Update()
        {
            var count = m_uses.Count;
            if (count < 1)
            {
                return;
            }

            var curTime = m_timer.Time;
            if (m_lastUpdateTime == curTime)
            {
                return;
            }

            m_lastUpdateTime = curTime;

            //找出过时的对象
            for (var i = 0; i < count; i++)
            {
                var poolVo = m_uses[i];
                if (curTime >= poolVo.Timeout)
                {
                    m_uses.FastRemoveAt(i);
                    m_clearCallback(poolVo.Value);
                    m_frees.Add(poolVo);

                    count--;
                    i--;
                }
            }
        }

        private class PoolVo
        {
            public int Timeout;
            public T   Value;
        }
    }
}