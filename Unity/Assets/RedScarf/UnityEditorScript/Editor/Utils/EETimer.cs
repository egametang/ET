using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// 计时器
/// </summary>
public class UESTimer : UESObject {

    internal float m_Delay;
    internal bool m_Running;
    internal int m_CurrentCount;
    internal int m_RepeatCount;
    internal double m_Time;
    internal bool m_Complete;

    public void Init(float delay,int repeatCount=-1)
    {
        m_Delay = delay;
        m_RepeatCount = repeatCount;
    }

    /// <summary>
    /// 开始
    /// </summary>
    public void Start()
    {
        ResetTime();
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        m_Running = false;
    }

    /// <summary>
    /// 重置时间
    /// </summary>
    public void ResetTime()
    {
        //使用Reset会引起命名冲突，Reset是内置函数？？
        m_CurrentCount = 0;
        m_Time = m_Window.TimeSinceStartup;
        m_Running = true;
    }

    /// <summary>
    /// 当前总次数
    /// </summary>
    public int CurrentCount { get { return m_CurrentCount; } }

    /// <summary>
    /// 触发间隔(秒)
    /// </summary>
    public float Delay { get { return m_Delay; } }

    /// <summary>
    /// 是否运行
    /// </summary>
    public bool Running { get {return m_Running; } }
}
