#include "il2cpp-config.h"

#include "gc/GarbageCollector.h"
#include "mono/ThreadPool/threadpool-ms.h"
#include "mono/ThreadPool/ThreadPoolDataStructures.h"
#include "mono/ThreadPool/ThreadPoolMacros.h"
#include "vm/String.h"
#include "vm/Object.h"
#include "vm/Random.h"
#include "vm/Runtime.h"
#include "vm/Thread.h"
#include "os/Time.h"

#define WORKER_CREATION_MAX_PER_SEC 10

static void remove_working_thread(Il2CppInternalThread *thread)
{
    int index = 0;
    for (unsigned i = 0; i < g_ThreadPool->working_threads.size(); ++i)
    {
        if (g_ThreadPool->working_threads[i] == thread)
        {
            index = i;
            break;
        }
    }
    g_ThreadPool->working_threads.erase(g_ThreadPool->working_threads.begin() + index);
}

/*
* mono_thread_info_install_interrupt: install an interruption token for the current thread.
*
*  - @callback: must be able to be called from another thread and always cancel the wait
*  - @data: passed to the callback
*  - @interrupted: will be set to TRUE if a token is already installed, FALSE otherwise
*     if set to TRUE, it must mean that the thread is in interrupted state
*/
static void thread_info_install_interrupt(void(*callback)(void* data), void* data, bool *interrupted)
{
    // We can get by without this for the time being.  Not needed until we have cooperative threading
}

static void thread_info_uninstall_interrupt(bool *interrupted)
{
    // We can get by without this for the time being.  Not needed until we have cooperative threading
}

static void worker_wait_interrupt(void* data)
{
    g_ThreadPool->active_threads_lock.Lock();
    g_ThreadPool->parked_threads_cond.Signal();
    g_ThreadPool->active_threads_lock.Unlock();
}

/* return true if timeout, false otherwise (worker unpark or interrupt) */
static bool worker_park(void)
{
    bool timeout = false;

    //mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] current worker parking", mono_native_thread_id_get ());

    il2cpp::gc::GarbageCollector::SetSkipThread(true);

    g_ThreadPool->active_threads_lock.Lock();

    if (!il2cpp::vm::Runtime::IsShuttingDown())
    {
        static void* rand_handle = NULL;
        Il2CppInternalThread *thread_internal;
        bool interrupted = false;

        if (!rand_handle)
            rand_handle = il2cpp::vm::Random::Create();
        IL2CPP_ASSERT(rand_handle);

        thread_internal = il2cpp::vm::Thread::CurrentInternal();
        IL2CPP_ASSERT(thread_internal);

        g_ThreadPool->parked_threads_count += 1;
        remove_working_thread(thread_internal);

        thread_info_install_interrupt(worker_wait_interrupt, NULL, &interrupted);
        if (interrupted)
            goto done;

        if (g_ThreadPool->parked_threads_cond.TimedWait(&g_ThreadPool->active_threads_lock, il2cpp::vm::Random::Next(&rand_handle, 5 * 1000, 60 * 1000)) != 0)
            timeout = true;

        thread_info_uninstall_interrupt(&interrupted);

    done:
        g_ThreadPool->working_threads.push_back(thread_internal);
        g_ThreadPool->parked_threads_count -= 1;
    }

    g_ThreadPool->active_threads_lock.Unlock();

    il2cpp::gc::GarbageCollector::SetSkipThread(false);

    //mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_THREADPOOL, "[%p] current worker unparking, timeout? %s", mono_native_thread_id_get (), timeout ? "yes" : "no");

    return timeout;
}

/* LOCKING: threadpool->domains_lock must be held */
static ThreadPoolDomain* domain_get_next(ThreadPoolDomain *current)
{
    ThreadPoolDomain *tpdomain = NULL;
    unsigned int len;

    len = (unsigned int)g_ThreadPool->domains.size();
    if (len > 0)
    {
        unsigned int i, current_idx = ~0u;
        if (current)
        {
            for (i = 0; i < len; ++i)
            {
                if (current == g_ThreadPool->domains[i])
                {
                    current_idx = i;
                    break;
                }
            }
            IL2CPP_ASSERT(current_idx != ~0u);
        }
        for (i = current_idx + 1; i < len + current_idx + 1; ++i)
        {
            ThreadPoolDomain *tmp = (ThreadPoolDomain*)g_ThreadPool->domains[i % len];
            if (tmp->outstanding_request > 0)
            {
                tpdomain = tmp;
                break;
            }
        }
    }

    return tpdomain;
}

struct WorkerThreadStateHolder
{
    Il2CppInternalThread *thread;
    ThreadPoolDomain* tpdomain;
    ThreadPoolDomain* previous_tpdomain;
    ThreadPoolCounter counter;
    bool retire;

    WorkerThreadStateHolder() :
        thread(il2cpp::vm::Thread::CurrentInternal()),
        tpdomain(NULL),
        previous_tpdomain(NULL),
        retire(false)
    {
        IL2CPP_ASSERT(thread);
        il2cpp::vm::Thread::SetName(thread, il2cpp::vm::String::New("IL2CPP Threadpool worker"));

        il2cpp::os::FastAutoLockOld activeThreadsLock(&g_ThreadPool->active_threads_lock);
        g_ThreadPool->working_threads.push_back(thread);
    }

    ~WorkerThreadStateHolder()
    {
        {
            il2cpp::os::FastAutoLockOld activeThreadsLock(&g_ThreadPool->active_threads_lock);
            remove_working_thread(thread);
        }

        COUNTER_ATOMIC(counter,
        {
            counter._.working--;
            counter._.active--;
        });
    }
};

struct WorkerThreadParkStateHolder
{
    ThreadPoolCounter& counter;
    il2cpp::os::FastAutoUnlock domainUnlock;

    WorkerThreadParkStateHolder(WorkerThreadStateHolder& workerThreadState) :
        counter(workerThreadState.counter),
        domainUnlock(&g_ThreadPool->domains_lock)
    {
        COUNTER_ATOMIC(counter,
        {
            counter._.working--;
            counter._.parked++;
        });
    }

    ~WorkerThreadParkStateHolder()
    {
        COUNTER_ATOMIC(counter,
        {
            counter._.working++;
            counter._.parked--;
        });
    }
};

struct WorkerThreadJobStateHolder
{
    ThreadPoolDomain* tpdomain;

    WorkerThreadJobStateHolder(const WorkerThreadStateHolder& workerThreadState) :
        tpdomain(workerThreadState.tpdomain)
    {
        tpdomain->outstanding_request--;
        IL2CPP_ASSERT(tpdomain->outstanding_request >= 0);

        IL2CPP_ASSERT(tpdomain->domain);
        IL2CPP_ASSERT(tpdomain->domain->threadpool_jobs >= 0);
        tpdomain->domain->threadpool_jobs++;
    }

    ~WorkerThreadJobStateHolder()
    {
        tpdomain->domain->threadpool_jobs--;
        IL2CPP_ASSERT(tpdomain->domain->threadpool_jobs >= 0);
    }
};

static void worker_thread(void* data)
{
    IL2CPP_ASSERT(g_ThreadPool);

    WorkerThreadStateHolder workerThreadState;
    il2cpp::os::FastAutoLock domainsLock(&g_ThreadPool->domains_lock);

    while (!il2cpp::vm::Runtime::IsShuttingDown())
    {
        workerThreadState.previous_tpdomain = workerThreadState.tpdomain;

        if (workerThreadState.retire || !(workerThreadState.tpdomain = domain_get_next(workerThreadState.previous_tpdomain)))
        {
            WorkerThreadParkStateHolder threadParkState(workerThreadState);

            if (worker_park())
                break;

            workerThreadState.retire = false;
            continue;
        }

        WorkerThreadJobStateHolder threadJobState(workerThreadState);
        il2cpp::os::FastAutoUnlock domainUnlock(&g_ThreadPool->domains_lock);

        Il2CppObject* res = il2cpp::vm::Runtime::InvokeWithThrow(il2cpp_defaults.threadpool_perform_wait_callback_method, NULL, NULL);
        if (res && *(bool*)il2cpp::vm::Object::Unbox(res) == false)
            workerThreadState.retire = true;

        il2cpp::vm::Thread::ClrState(workerThreadState.thread, static_cast<il2cpp::vm::ThreadState>(~il2cpp::vm::kThreadStateBackground));
        if (!il2cpp::vm::Thread::TestState(workerThreadState.thread, il2cpp::vm::kThreadStateBackground))
            il2cpp::vm::Thread::SetState(workerThreadState.thread, il2cpp::vm::kThreadStateBackground);
    }
}

bool worker_try_create()
{
    ThreadPoolCounter counter;
    Il2CppInternalThread *thread;
    int64_t current_ticks;
    int32_t now;

    il2cpp::os::FastAutoLock lock(&g_ThreadPool->worker_creation_lock);

    current_ticks = il2cpp::os::Time::GetTicks100NanosecondsMonotonic();
    now = (int32_t)(current_ticks / (10 * 1000 * 1000));

    if (current_ticks != 0)
    {
        if (g_ThreadPool->worker_creation_current_second != now)
        {
            g_ThreadPool->worker_creation_current_second = now;
            g_ThreadPool->worker_creation_current_count = 0;
        }
        else
        {
            IL2CPP_ASSERT(g_ThreadPool->worker_creation_current_count <= WORKER_CREATION_MAX_PER_SEC);
            if (g_ThreadPool->worker_creation_current_count == WORKER_CREATION_MAX_PER_SEC)
            {
                // Worker creation failed because maximum number of workers already created in the last second
                return false;
            }
        }
    }

    COUNTER_ATOMIC(counter,
    {
        if (counter._.working >= counter._.max_working)
        {
            // Worked creation failed because maximum number of workers are running
            return false;
        }
        counter._.working++;
        counter._.active++;
    });

    if ((thread = il2cpp::vm::Thread::CreateInternal(worker_thread, NULL, true, 0)) != NULL)
    {
        g_ThreadPool->worker_creation_current_count += 1;
        return true;
    }

    // Failed creating native thread :(
    COUNTER_ATOMIC(counter,
    {
        counter._.working--;
        counter._.active--;
    });

    return false;
}
