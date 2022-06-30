#include "il2cpp-config.h"

#include "gc/GarbageCollector.h"
#include "mono/ThreadPool/threadpool-ms.h"
#include "mono/ThreadPool/ThreadPoolDataStructures.h"
#include "mono/ThreadPool/ThreadPoolMacros.h"
#include "mono/ThreadPool/ThreadPoolMonitorThread.h"
#include "mono/ThreadPool/ThreadPoolWorkerThread.h"
#include "vm/Runtime.h"
#include "vm/Thread.h"
#include "os/Time.h"
#include "os/CpuInfo.h"

#define MONITOR_INTERVAL 500 // ms
#define MONITOR_MINIMAL_LIFETIME 60 * 1000 // ms

static int32_t s_MonitorStatus = MONITOR_STATUS_NOT_RUNNING;

MonitorStatus GetMonitorStatus()
{
    return static_cast<MonitorStatus>(s_MonitorStatus);
}

static int32_t cpu_info_usage(void* prev)
{
    // Note : Implementing CpuInfo on all platforms will be challenging, so for now we are going to cheat
    // and always say it's low
#if !IL2CPP_USE_GENERIC_CPU_INFO
    return il2cpp::os::CpuInfo::Usage(prev);
#else
    return CPU_USAGE_LOW;
#endif
}

static Il2CppException* mono_thread_interruption_checkpoint(void)
{
    // For now just do nothing.  The one place this is used doesn't care about the return value
    return NULL;
}

/* LOCKING: threadpool->domains_lock must be held */
static bool domain_any_has_request(void)
{
    unsigned int i;

    for (i = 0; i < g_ThreadPool->domains.size(); ++i)
    {
        ThreadPoolDomain *tmp = g_ThreadPool->domains[i];
        if (tmp->outstanding_request > 0)
            return true;
    }

    return false;
}

static bool monitor_sufficient_delay_since_last_dequeue(void)
{
    int64_t threshold;

    IL2CPP_ASSERT(g_ThreadPool);

    if (g_ThreadPool->cpu_usage < CPU_USAGE_LOW)
    {
        threshold = MONITOR_INTERVAL;
    }
    else
    {
        ThreadPoolCounter counter;
        counter.as_int64_t = COUNTER_READ();
        threshold = counter._.max_working * MONITOR_INTERVAL * 2;
    }

    return il2cpp::os::Time::GetTicksMillisecondsMonotonic() >= g_ThreadPool->heuristic_last_dequeue + threshold;
}

static bool monitor_should_keep_running(void)
{
    static int64_t last_should_keep_running = -1;

    IL2CPP_ASSERT(s_MonitorStatus == MONITOR_STATUS_WAITING_FOR_REQUEST || s_MonitorStatus == MONITOR_STATUS_REQUESTED);

    if (il2cpp::os::Atomic::Exchange(&s_MonitorStatus, MONITOR_STATUS_WAITING_FOR_REQUEST) == MONITOR_STATUS_WAITING_FOR_REQUEST)
    {
        bool should_keep_running = true, force_should_keep_running = false;

        if (il2cpp::vm::Runtime::IsShuttingDown())
        {
            should_keep_running = false;
        }
        else
        {
            g_ThreadPool->domains_lock.Acquire();
            if (!domain_any_has_request())
                should_keep_running = false;
            g_ThreadPool->domains_lock.Release();

            if (!should_keep_running)
            {
                if (last_should_keep_running == -1 || il2cpp::os::Time::GetTicks100NanosecondsMonotonic() - last_should_keep_running < MONITOR_MINIMAL_LIFETIME * 1000 * 10)
                {
                    should_keep_running = force_should_keep_running = true;
                }
            }
        }

        if (should_keep_running)
        {
            if (last_should_keep_running == -1 || !force_should_keep_running)
                last_should_keep_running = il2cpp::os::Time::GetTicks100NanosecondsMonotonic();
        }
        else
        {
            last_should_keep_running = -1;
            if (il2cpp::os::Atomic::CompareExchange(&s_MonitorStatus, MONITOR_STATUS_NOT_RUNNING, MONITOR_STATUS_WAITING_FOR_REQUEST) == MONITOR_STATUS_WAITING_FOR_REQUEST)
                return false;
        }
    }

    IL2CPP_ASSERT(s_MonitorStatus == MONITOR_STATUS_WAITING_FOR_REQUEST || s_MonitorStatus == MONITOR_STATUS_REQUESTED);

    return true;
}

static void monitor_thread(void* data)
{
    Il2CppInternalThread *current_thread = il2cpp::vm::Thread::CurrentInternal();
    unsigned int i;

    cpu_info_usage(g_ThreadPool->cpu_usage_state);

    //mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] monitor thread, started", mono_native_thread_id_get ());

    do
    {
        ThreadPoolCounter counter;
        bool limit_worker_max_reached;
        int32_t interval_left = MONITOR_INTERVAL;
        int32_t awake = 0; /* number of spurious awakes we tolerate before doing a round of rebalancing */

        IL2CPP_ASSERT(s_MonitorStatus != MONITOR_STATUS_NOT_RUNNING);

        il2cpp::gc::GarbageCollector::SetSkipThread(true);

        do
        {
            int64_t ts;
            bool alerted = false;

            if (il2cpp::vm::Runtime::IsShuttingDown())
                break;

            ts = il2cpp::os::Time::GetTicksMillisecondsMonotonic();
            il2cpp::vm::Thread::Sleep(interval_left);
            /*if (mono_thread_info_sleep (interval_left, &alerted) == 0)
            break;*/
            interval_left -= (int32_t)(il2cpp::os::Time::GetTicksMillisecondsMonotonic() - ts);

            il2cpp::gc::GarbageCollector::SetSkipThread(false);
            if ((current_thread->state & (il2cpp::vm::kThreadStateStopRequested | il2cpp::vm::kThreadStateSuspendRequested)) != 0)
                mono_thread_interruption_checkpoint();
            il2cpp::gc::GarbageCollector::SetSkipThread(true);
        }
        while (interval_left > 0 && ++awake < 10);

        il2cpp::gc::GarbageCollector::SetSkipThread(false);

        if (g_ThreadPool->suspended)
            continue;

        if (il2cpp::vm::Runtime::IsShuttingDown())
            continue;

        g_ThreadPool->domains_lock.Acquire();
        if (!domain_any_has_request())
        {
            g_ThreadPool->domains_lock.Release();
            continue;
        }
        g_ThreadPool->domains_lock.Release();

        g_ThreadPool->cpu_usage = cpu_info_usage(g_ThreadPool->cpu_usage_state);

        if (!monitor_sufficient_delay_since_last_dequeue())
            continue;

        limit_worker_max_reached = false;

        COUNTER_ATOMIC(counter,
        {
            if (counter._.max_working >= g_ThreadPool->limit_worker_max)
            {
                limit_worker_max_reached = true;
                break;
            }
            counter._.max_working++;
        });

        if (limit_worker_max_reached)
            continue;

        hill_climbing_force_change(counter._.max_working, TRANSITION_STARVATION);

        for (i = 0; i < 5; ++i)
        {
            if (il2cpp::vm::Runtime::IsShuttingDown())
                break;

            if (worker_try_unpark())
            {
                //mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] monitor thread, unparked", mono_native_thread_id_get ());
                break;
            }

            if (worker_try_create())
            {
                //mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] monitor thread, created", mono_native_thread_id_get ());
                break;
            }
        }
    }
    while (monitor_should_keep_running());

    //mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] monitor thread, finished", mono_native_thread_id_get ());
}

void monitor_ensure_running()
{
    for (;;)
    {
        switch (s_MonitorStatus)
        {
            case MONITOR_STATUS_REQUESTED:
                return;
            case MONITOR_STATUS_WAITING_FOR_REQUEST:
                il2cpp::os::Atomic::CompareExchange(&s_MonitorStatus, MONITOR_STATUS_REQUESTED, MONITOR_STATUS_WAITING_FOR_REQUEST);
                break;
            case MONITOR_STATUS_NOT_RUNNING:
                if (il2cpp::vm::Runtime::IsShuttingDown())
                    return;
                if (il2cpp::os::Atomic::CompareExchange(&s_MonitorStatus, MONITOR_STATUS_REQUESTED, MONITOR_STATUS_NOT_RUNNING) == MONITOR_STATUS_NOT_RUNNING)
                {
                    if (!il2cpp::vm::Thread::CreateInternal(monitor_thread, NULL, true, SMALL_STACK))
                        s_MonitorStatus = MONITOR_STATUS_NOT_RUNNING;

                    return;
                }
                break;
            default:
                IL2CPP_ASSERT(0 && "should not be reached");
        }
    }
}
