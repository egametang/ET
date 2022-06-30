#pragma once

#include "Baselib_Timer.h"
#include <math.h>

typedef struct Baselib_CountdownTimer
{
    Baselib_Timer_Ticks startTimeInTicks;
    Baselib_Timer_Ticks timeoutInTicks;
} Baselib_CountdownTimer;

BASELIB_INLINE_API Baselib_Timer_Ticks Detail_MillisecondsToTicks(double milliseconds)
{
    return (Baselib_Timer_Ticks)(milliseconds * Baselib_NanosecondsPerMillisecond / Baselib_Timer_TickToNanosecondsConversionFactor);
}

BASELIB_INLINE_API double Detail_TicksToMilliseconds(Baselib_Timer_Ticks ticks)
{
    return ticks * Baselib_Timer_TickToNanosecondsConversionFactor / Baselib_NanosecondsPerMillisecond;
}

// Create and start a countdown timer
BASELIB_INLINE_API Baselib_CountdownTimer Baselib_CountdownTimer_StartMs(uint32_t timeoutInMilliseconds)
{
    const Baselib_CountdownTimer timer = {Baselib_Timer_GetHighPrecisionTimerTicks(), Detail_MillisecondsToTicks(timeoutInMilliseconds)};
    return timer;
}

BASELIB_INLINE_API Baselib_CountdownTimer Baselib_CountdownTimer_StartTicks(Baselib_Timer_Ticks timeoutInTicks)
{
    const Baselib_CountdownTimer timer = {Baselib_Timer_GetHighPrecisionTimerTicks(), timeoutInTicks};
    return timer;
}

// Get the number of ticks left before countdown expires.
//
// This function is guaranteed to return zero once timeout expired.
// It is also guaranteed that this function will not return zero until timeout expires.
BASELIB_INLINE_API Baselib_Timer_Ticks Baselib_CountdownTimer_GetTimeLeftInTicks(Baselib_CountdownTimer timer)
{
    const Baselib_Timer_Ticks then     = timer.startTimeInTicks;
    const Baselib_Timer_Ticks now      = Baselib_Timer_GetHighPrecisionTimerTicks();
    const Baselib_Timer_Ticks timeLeft = timer.timeoutInTicks - (now - then);
    return timeLeft <= timer.timeoutInTicks ? timeLeft : 0;
}

// Get the number of milliseconds left before countdown expires.
//
// This function is guaranteed to return zero once timeout expired.
// It is also guaranteed that this function will not return zero until timeout expires.
BASELIB_INLINE_API uint32_t Baselib_CountdownTimer_GetTimeLeftInMilliseconds(Baselib_CountdownTimer timer)
{
    const Baselib_Timer_Ticks timeLeft = Baselib_CountdownTimer_GetTimeLeftInTicks(timer);
    return (uint32_t)ceil(Detail_TicksToMilliseconds(timeLeft));
}

// Check if timout has been reached.
BASELIB_INLINE_API bool Baselib_CountdownTimer_TimeoutExpired(Baselib_CountdownTimer timer)
{
    return Baselib_CountdownTimer_GetTimeLeftInTicks(timer) == 0;
}
