#pragma once

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// Time conversion factors.
//
// (not an enum since Int32 can't represent Baselib_NanosecondsPerMinute)
static const uint64_t Baselib_SecondsPerMinute           =                               60ULL;
static const uint64_t Baselib_MillisecondsPerSecond      =                             1000ULL;
static const uint64_t Baselib_MillisecondsPerMinute      =                     60ULL * 1000ULL;
static const uint64_t Baselib_MicrosecondsPerMillisecond =                             1000ULL;
static const uint64_t Baselib_MicrosecondsPerSecond      =                   1000ULL * 1000ULL;
static const uint64_t Baselib_MicrosecondsPerMinute      =           60ULL * 1000ULL * 1000ULL;
static const uint64_t Baselib_NanosecondsPerMicrosecond  =                             1000ULL;
static const uint64_t Baselib_NanosecondsPerMillisecond  =                   1000ULL * 1000ULL;
static const uint64_t Baselib_NanosecondsPerSecond       =         1000ULL * 1000ULL * 1000ULL;
static const uint64_t Baselib_NanosecondsPerMinute       = 60ULL * 1000ULL * 1000ULL * 1000ULL;

// Timer specific representation of time progression
typedef uint64_t Baselib_Timer_Ticks;

// Baselib_Timer_Ticks are guaranteed to be more granular than this constant.
static const uint64_t Baselib_Timer_MaxNumberOfNanosecondsPerTick = 1000ULL;

// Baselib_Timer_Ticks are guaranteed to be less granular than this constant.
static const double Baselib_Timer_MinNumberOfNanosecondsPerTick = 0.01;

// Defines the conversion ratio from Baselib_Timer_Ticks to nanoseconds as a fraction.
typedef struct Baselib_Timer_TickToNanosecondConversionRatio
{
    uint64_t ticksToNanosecondsNumerator;
    uint64_t ticksToNanosecondsDenominator;
} Baselib_Timer_TickToNanosecondConversionRatio;

// Returns the conversion ratio between ticks and nanoseconds.
//
// The conversion factor is guaranteed to be constant for the entire application for its entire lifetime.
// However, it may be different on every start of the application.
//
// \returns The conversion factor from ticks to nanoseconds as an integer fraction.
BASELIB_API Baselib_Timer_TickToNanosecondConversionRatio Baselib_Timer_GetTicksToNanosecondsConversionRatio(void);

// The fraction of Baselib_Timer_GetTicksToNanosecondsConversionRatio as a precomputed double value. It is subject to precision loss.
//
// Attention:
// This value is determined during static initialization of baselib. As such it should not be used if it is not guaranteed that baselib is fully loaded.
// Prefer Baselib_Timer_GetTicksToNanosecondsConversionRatio when in doubt.
extern BASELIB_API const double Baselib_Timer_TickToNanosecondsConversionFactor;

// Get the current tick count of the high precision timer.
//
// Accuracy:
// It is assumed that the accuracy corresponds to the granularity of Baselib_Timer_Ticks (which is determined by Baselib_Timer_GetTicksToNanosecondsConversionRatio).
// However, there are no strict guarantees on the accuracy of the timer.
//
// Monotony:
// ATTENTION: On some platforms this clock is suspended during application/device sleep states.
// The timer is not susceptible to wall clock time changes by the user.
// Different threads are guaranteed to be on the same timeline.
//
// Known issues:
// * Some web browsers impose Spectre mitigation which can introduce jitter in this timer.
// * Some web browsers may have different timelines per thread/webworker if they are not spawned on startup (this is a bug according to newest W3C specification)
//
// \returns Current tick value of the high precision timer.
BASELIB_API Baselib_Timer_Ticks Baselib_Timer_GetHighPrecisionTimerTicks(void);

// This function will wait for at least the requested amount of time before returning.
//
// Unlike some implementations of 'sleep', passing 0 does NOT guarantee a thread yield and may return immediately! Use the corresponding functionality in Baselib_Thread instead.
//
// \param timeInMilliseconds      Time to wait in milliseconds
BASELIB_API void Baselib_Timer_WaitForAtLeast(uint32_t timeInMilliseconds);

// Time since application startup in seconds.
//
// Disregarding potential rounding errors, all threads are naturally on the same timeline (i.e. time since process start).
BASELIB_API double Baselib_Timer_GetTimeSinceStartupInSeconds(void);

#ifdef __cplusplus
} // extern "C"
#endif
