using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UESTimerEvent : UESEvent
{
    public const string TIMER = "Timer";
    public const string TIMER_COMPLETE = "TimerComplete";

    public UESTimerEvent(UESObject target, string type, Action<UESEvent> callback)
        : base(target, type, callback)
    {

    }

    public override void Update()
    {
        var timer = (UESTimer)m_CurrentTarget;
        if (timer.m_Running)
        {
            if (timer.m_Window.TimeSinceStartup - timer.m_Time >= timer.m_Delay/1000)
            {
                if (timer.m_RepeatCount <= 0 || timer.m_CurrentCount < timer.m_RepeatCount)
                {
                    timer.m_CurrentCount++;
                    timer.m_Time = timer.m_Time = timer.m_Window.TimeSinceStartup;
                    if (timer.m_CurrentCount == timer.m_RepeatCount && timer.m_RepeatCount > 0)
                    {
                        timer.m_Complete = true;
                        timer.m_Running = false;
                    }

                    if (m_Type == TIMER) Invoke();
                }
            }
        }
        else timer.m_Time = timer.m_Window.TimeSinceStartup;

        if(timer.m_Complete&&timer.m_CurrentCount==timer.m_RepeatCount&& timer.m_RepeatCount > 0)
        {
            if (m_Type == TIMER_COMPLETE)
            {
                timer.m_Complete = false;
                Invoke();
            }
        }
    }
}
