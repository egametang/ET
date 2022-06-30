#pragma once

#include "../C/Baselib_CountdownTimer.h"
#include "Time.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        class CountdownTimer
        {
        public:
            //
            // Create a countdown timer that already expired.
            //
            // Guaranteed to not sample the system timer.
            //
            static CountdownTimer InitializeExpired()
            {
                return CountdownTimer();
            }

            //
            // Create and start a countdown timer.
            //
            static CountdownTimer StartNew(const high_precision_clock::duration timeout)
            {
                return CountdownTimer(timeout);
            }

            //
            // Get time left before timeout expires.
            //
            // This function is guaranteed to return zero once timeout expired.
            // It is also guaranteed that this function will not return zero until timeout expires.
            // Return the time left as a high precision duration.
            //
            high_precision_clock::duration GetTimeLeft() const
            {
                return high_precision_clock::duration_from_ticks(Baselib_CountdownTimer_GetTimeLeftInTicks(m_CountdownTimer));
            }

            //
            // Get time left before timeout expires.
            //
            // This function is guaranteed to return zero once timeout expired.
            // It is also guaranteed that this function will not return zero until timeout expires.
            // Return the time left as a millisecond integer duration.
            //
            timeout_ms GetTimeLeftInMilliseconds() const
            {
                return timeout_ms(Baselib_CountdownTimer_GetTimeLeftInMilliseconds(m_CountdownTimer));
            }

            //
            // Check if timout has been reached.
            //
            bool TimeoutExpired() const
            {
                return Baselib_CountdownTimer_TimeoutExpired(m_CountdownTimer);
            }

        private:
            CountdownTimer() : m_CountdownTimer{0, 0} {}
            CountdownTimer(const high_precision_clock::duration timeout) : m_CountdownTimer(Baselib_CountdownTimer_StartTicks(high_precision_clock::ticks_from_duration_roundup(timeout))) {}

            Baselib_CountdownTimer m_CountdownTimer;
        };
    }
}
