using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YIUIFramework
{
    public partial class RedDotData
    {
        //已改变的脏标记
        private bool m_ChangeDirty;

        //即将改变的值
        private int m_DirtyCount = -1;

        //脏数据堆栈
        private RedDotStack m_DirtyStack;

        //脏数据第一个改变的数据
        private FirstRedDotChangeData m_DirtyFirstRedDotChangeData;

        /// <summary>
        /// 脏数据设置数量
        /// </summary>
        internal bool TryDirtySetCount(int count)
        {
            if (ChildList.Count <= 0)
            {
                if (RealCount == count) //与现在真实数据相同 不需要改变
                {
                    ResetDirty();
                    return false;
                }

                m_ChangeDirty = true;
                m_DirtyCount  = count;
                SetDirtyOS(count);
                return true;
            }

            Debug.LogError($"{Key} 配置 不是最后一级红点 请不要直接修改");
            return false;
        }

        //刷新脏数据
        internal void RefreshDirtyCount()
        {
            if (!m_ChangeDirty)
            {
                Debug.LogError($"{Key} 没有脏标 请勿调用此方法");
                return;
            }

            SetCount(m_DirtyCount, m_DirtyStack);

            ResetDirty();
        }

        private void SetDirtyOS(int count)
        {
            #if UNITY_EDITOR || YIUIMACRO_REDDOT_STACK

            m_DirtyFirstRedDotChangeData ??= new FirstRedDotChangeData();
            {
                m_DirtyFirstRedDotChangeData.ChangeData    = this;
                m_DirtyFirstRedDotChangeData.OriginalCount = RealCount;
                m_DirtyFirstRedDotChangeData.ChangeCount   = count;
                m_DirtyFirstRedDotChangeData.ChangeTips    = Tips;
            }

            m_DirtyStack ??= new RedDotStack();
            {
                m_DirtyStack.Id            = StackList.Count + 1;
                m_DirtyStack.DataTime      = DateTime.Now;
                m_DirtyStack.StackTrace    = new System.Diagnostics.StackTrace(true);
                m_DirtyStack.RedDotOSType  = ERedDotOSType.Count;
                m_DirtyStack.OriginalCount = RealCount;
                m_DirtyStack.ChangeCount   = count;
                m_DirtyStack.ChangeTips    = Tips;
                m_DirtyStack.FirstData     = m_DirtyFirstRedDotChangeData;
            }

            #endif
        }

        //初始化脏标数据
        private void ResetDirty()
        {
            m_ChangeDirty                = false;
            m_DirtyCount                 = -1;
            m_DirtyStack                 = null;
            m_DirtyFirstRedDotChangeData = null;
        }
    }
}