#pragma once

#include "Time.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // Stopwatch
        // Simplistic stopwatch tool to take accurate time measurements using Baselib_Timer
        //
        // Usage example:
        // auto watch = Stopwatch::StartNew();
        // HeavyOperation();
        // printf("Time passed: %fs", watch.GetElapsedTime().ToSeconds());
        class Stopwatch
        {
        public:
            static Stopwatch StartNew() { return Stopwatch(); }

            high_precision_clock::duration GetElapsedTime() const
            {
                return high_precision_clock::duration_from_ticks(high_precision_clock::now_in_ticks() - m_StartTime);
            }

            high_precision_clock::duration Restart()
            {
                high_precision_clock::duration elapsed = GetElapsedTime();
                m_StartTime = high_precision_clock::now_in_ticks();
                return elapsed;
            }

        private:
            Stopwatch() : m_StartTime(high_precision_clock::now_in_ticks()) {}

            Baselib_Timer_Ticks m_StartTime;
        };
    }
}
