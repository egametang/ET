#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "vm/Monitor.h"

#if IL2CPP_SUPPORT_THREADS

#include "os/Atomic.h"
#include "os/Event.h"
#include "os/Semaphore.h"
#include "os/Thread.h"
#include "vm/Exception.h"
#include "vm/Thread.h"

#include "utils/ThreadSafeFreeList.h"

#include <limits>
#include <exception>

#include "Baselib.h"
#include "Cpp/Atomic.h"


// Mostly follows the algorithm outlined in "Implementing Fast Java Monitors with Relaxed-Locks".

/// State of a lock associated with an object.
///
/// Allocated from the normal non-GC heap and kept on a free list. This means that an object that is
/// not unlocked before it is reclaimed will leak its monitor. However, it allows us to not have to
/// synchronize with the GC and to efficiently reuse a small number of monitor instances between an
/// arbitrary number of managed objects.
///
// NOTE: We do *NOT* allow deletion of monitors as threads may be hanging on to monitors even as they
//  are already back on the free list (and maybe even in use somewhere else already).
struct MonitorData : public il2cpp::utils::ThreadSafeFreeListNode
{
    static const il2cpp::os::Thread::ThreadId kCanBeAcquiredByOtherThread = il2cpp::os::Thread::kInvalidThreadId;
    static const il2cpp::os::Thread::ThreadId kHasBeenReturnedToFreeList = (il2cpp::os::Thread::ThreadId)-1;

    /// ID of thread that currently has the object locked or one of the two values above.
    ///
    /// This signals three possible states:
    ///
    /// 1) Contains a valid thread ID. Means the monitor is owned by that thread and only that thread can
    ///    change the value of this field.
    /// 2) Contains kCanBeAcquiredByOtherThread. Means monitor is still live and attached to an object
    ///    but is up for grabs by whichever thread manages to swap the value of this field for its own
    ///    thread ID first.
    /// 3) Contains kHasBeenReturnedToFreeList. Means monitor is not attached to any object and can be
    ///    acquired by any thread *but* only through the free list.
    baselib::atomic<il2cpp::os::Thread::ThreadId> owningThreadId;

    /// Number of times the object has been locked on the owning thread. Everything above 1 indicates
    /// a recursive lock.
    /// NOTE: This field is never reset to zero.
    uint32_t recursiveLockingCount;

    /// Semaphore used to signal other blocked threads that the monitor has become available.
    /// The "ready queue" is implicit in the list of threads waiting on this semaphore.
    il2cpp::os::Semaphore semaphore;

    /// Number of threads that are already waiting or are about to wait for a lock on the monitor.
    baselib::atomic<uint32_t> numThreadsWaitingForSemaphore;

    /// Event that a waiting thread fires to acknowledge that it has been kicked off a monitor by the thread
    /// already holding a lock on the object being waited for. This happens when the locking thread decides
    /// to deflate the locked object and thus kill the monitor but then some other thread comes along and
    /// decides to wait on the monitor-to-be-killed.
    il2cpp::os::Event flushAcknowledged;

    /// Node in list of waiting threads.
    ///
    /// Memory management is done the same way as for MonitorData itself. The same constraints apply.
    /// Wait nodes are returned to the free list by the threads that have created them except for abandoned
    /// nodes which may be returned by the pulsing thread.
    ///
    /// NOTE: Every wait node must be cleaned up by the wait thread that allocated it.
    struct PulseWaitingListNode : public il2cpp::utils::ThreadSafeFreeListNode
    {
        enum State
        {
            /// Node is waiting to be reused.
            kUnused,
            /// Node is waiting to be signaled.
            kWaiting
        };

        /// Next node in "threadsWaitingForPulse" list.
        /// NOTE: Once on the list, this field may only be modified by the thread holding a lock
        ///       on the respective monitor.
        PulseWaitingListNode* nextNode;

        /// Event to notify waiting thread of pulse.
        il2cpp::os::Event signalWaitingThread;

        /// Current usage state. This is not set atomically. Change this state only if you
        /// are at a known sequence point.
        int32_t state;

        static il2cpp::utils::ThreadSafeFreeList<PulseWaitingListNode> s_FreeList;

        PulseWaitingListNode()
            : nextNode(NULL)
            , state(kUnused) {}

        void Release()
        {
            state = kUnused;
            signalWaitingThread.Reset();
            s_FreeList.Release(this);
        }

        static PulseWaitingListNode* Allocate()
        {
            PulseWaitingListNode* node = s_FreeList.Allocate();
            IL2CPP_ASSERT(node->state == kUnused);
            return node;
        }
    };

    /// List of threads waiting for a pulse on the monitor.
    /// NOTE: This field may be modified concurrently by several threads (no lock).
    PulseWaitingListNode* threadsWaitingForPulse;

    static il2cpp::utils::ThreadSafeFreeList<MonitorData> s_FreeList;

    MonitorData()
        : owningThreadId(kHasBeenReturnedToFreeList)
        , recursiveLockingCount(1)
        , numThreadsWaitingForSemaphore(0)
        , threadsWaitingForPulse(NULL)
        , semaphore(0, std::numeric_limits<int32_t>::max())
    {
    }

    bool IsAcquired() const
    {
        return (owningThreadId != kCanBeAcquiredByOtherThread && owningThreadId != kHasBeenReturnedToFreeList);
    }

    bool TryAcquire(size_t threadId)
    {
        // The compare_exchange_strong method can change its first argument.
        // We don't care about the changed valuem, though, so ignore it.
        il2cpp::os::Thread::ThreadId local = kCanBeAcquiredByOtherThread;
        return owningThreadId.compare_exchange_strong(local, threadId);
    }

    void Unacquire()
    {
        IL2CPP_ASSERT(owningThreadId == il2cpp::os::Thread::CurrentThreadId());
        owningThreadId = kCanBeAcquiredByOtherThread;
    }

    /// Mark current thread as being blocked in Monitor.Enter(), i.e. as "ready to acquire monitor
    /// whenever it becomes available."
    void AddCurrentThreadToReadyList()
    {
        numThreadsWaitingForSemaphore++;
        il2cpp::vm::Thread::SetState(il2cpp::vm::Thread::Current(), il2cpp::vm::kThreadStateWaitSleepJoin);
    }

    /// Mark current thread is no longer being blocked on the monitor.
    int RemoveCurrentThreadFromReadyList()
    {
        int numRemainingWaitingThreads = --numThreadsWaitingForSemaphore;
        il2cpp::vm::Thread::ClrState(il2cpp::vm::Thread::Current(), il2cpp::vm::kThreadStateWaitSleepJoin);
        return numRemainingWaitingThreads;
    }

    /// Acknowledge that the owning thread has decided to kill the monitor (a.k.a. deflate the corresponding
    /// object) while we were waiting on it.
    void VacateDyingMonitor()
    {
        RemoveCurrentThreadFromReadyList();
        flushAcknowledged.Set();
    }

    void PushOntoPulseWaitingList(PulseWaitingListNode* node)
    {
        // Change state to waiting. Safe to not do this atomically as at this point,
        // the waiting thread is the only one with access to the node.
        node->state = PulseWaitingListNode::kWaiting;

        // Race other wait threads until we've successfully linked the
        // node into the list.
        while (true)
        {
            PulseWaitingListNode* nextNode = threadsWaitingForPulse;
            node->nextNode = nextNode;
            if (il2cpp::os::Atomic::CompareExchangePointer(&threadsWaitingForPulse, node, nextNode) == nextNode)
                break;
        }
    }

    /// Get the next wait node and remove it from the list.
    /// NOTE: Calling thread *must* have the monitor locked.
    PulseWaitingListNode* PopNextFromPulseWaitingList()
    {
        IL2CPP_ASSERT(owningThreadId == il2cpp::os::Thread::CurrentThreadId());

        PulseWaitingListNode* head = threadsWaitingForPulse;
        if (!head)
            return NULL;

        // Grab the node for ourselves. We take the node even if some other thread
        // changes "threadsWaitingForPulse" in the meantime. If that happens, we don't
        // unlink the node and the node will stay on the list until the waiting thread
        // cleans up the list.
        PulseWaitingListNode* next = head->nextNode;
        if (il2cpp::os::Atomic::CompareExchangePointer(&threadsWaitingForPulse, next, head) == head)
            head->nextNode = NULL;

        return head;
    }

    /// Remove the given waiting node from "threadsWaitingForPulse".
    /// NOTE: Calling thread *must* have the monitor locked.
    bool RemoveFromPulseWaitingList(PulseWaitingListNode* node)
    {
        IL2CPP_ASSERT(owningThreadId == il2cpp::os::Thread::CurrentThreadId());

        // This function works only because threads calling Wait() on the monitor will only
        // ever *prepend* nodes to the list. This means that only the "threadsWaitingForPulse"
        // variable is actually shared between threads whereas the list contents are owned
        // by the thread that has the monitor locked.

    tryAgain:
        PulseWaitingListNode * previous = NULL;
        for (PulseWaitingListNode* current = threadsWaitingForPulse; current != NULL;)
        {
            // Go through list looking for node.
            if (current != node)
            {
                previous = current;
                current = current->nextNode;
                continue;
            }

            // Node found. Remove.
            if (previous)
                previous->nextNode = node->nextNode;
            else
            {
                // We may have to change "threadsWaitingForPulse" and thus have to synchronize
                // with other threads.
                if (il2cpp::os::Atomic::CompareExchangePointer(&threadsWaitingForPulse, node->nextNode, node) != node)
                {
                    // One or more other threads have changed the list.
                    goto tryAgain;
                }
            }
            node->nextNode = NULL;

            return true;
        }

        // Not found in list.
        return false;
    }
};

il2cpp::utils::ThreadSafeFreeList<MonitorData> MonitorData::s_FreeList;
il2cpp::utils::ThreadSafeFreeList<MonitorData::PulseWaitingListNode> MonitorData::PulseWaitingListNode::s_FreeList;

static MonitorData* GetMonitorAndThrowIfNotLockedByCurrentThread(Il2CppObject* obj)
{
    // Fetch monitor data.
    MonitorData* monitor = il2cpp::os::Atomic::ReadPointer(&obj->monitor);
    if (!monitor)
    {
        // No one locked this object.
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetSynchronizationLockException("Object is not locked."));
    }

    // Throw SynchronizationLockException if we're not holding a lock.
    // NOTE: Unlike .NET, Mono simply ignores this and does not throw.
    uint64_t currentThreadId = il2cpp::os::Thread::CurrentThreadId();
    if (monitor->owningThreadId != currentThreadId)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetSynchronizationLockException
                ("Object has not been locked by this thread."));
    }

    return monitor;
}

namespace il2cpp
{
namespace vm
{
    void Monitor::Enter(Il2CppObject* object)
    {
        TryEnter(object, std::numeric_limits<uint32_t>::max());
    }

    bool Monitor::TryEnter(Il2CppObject* obj, uint32_t timeOutMilliseconds)
    {
        size_t currentThreadId = il2cpp::os::Thread::CurrentThreadId();

        while (true)
        {
            MonitorData* installedMonitor = il2cpp::os::Atomic::ReadPointer(&obj->monitor);
            if (!installedMonitor)
            {
                // Set up a new monitor.
                MonitorData* newlyAllocatedMonitorForThisThread = MonitorData::s_FreeList.Allocate();
                il2cpp::os::Thread::ThreadId previousOwnerThreadId = newlyAllocatedMonitorForThisThread->owningThreadId.exchange(currentThreadId);
                IL2CPP_ASSERT(previousOwnerThreadId == MonitorData::kHasBeenReturnedToFreeList && "Monitor on freelist cannot be owned by thread!");

                // Try to install the monitor on the object (aka "inflate" the object).
                if (il2cpp::os::Atomic::CompareExchangePointer(&obj->monitor, newlyAllocatedMonitorForThisThread, (MonitorData*)NULL) == NULL)
                {
                    // Done. There was no contention on this object. This is
                    // the fast path.
                    IL2CPP_ASSERT(obj->monitor);
                    IL2CPP_ASSERT(obj->monitor->recursiveLockingCount == 1);
                    IL2CPP_ASSERT(obj->monitor->owningThreadId == currentThreadId);
                    return true;
                }
                else
                {
                    // Some other thread raced us and won. Retry.
                    newlyAllocatedMonitorForThisThread->owningThreadId = MonitorData::kHasBeenReturnedToFreeList;
                    MonitorData::s_FreeList.Release(newlyAllocatedMonitorForThisThread);
                    continue;
                }
            }

            // Object was locked previously. See if we already have the lock.
            if (installedMonitor->owningThreadId == currentThreadId)
            {
                // Yes, recursive lock. Just increase count.
                ++installedMonitor->recursiveLockingCount;
                return true;
            }

            // Attempt to acquire lock if it's free
            if (installedMonitor->TryAcquire(currentThreadId))
            {
                // There is no locking around the sections of this logic to speed
                // things up, there is potential for race condition to reset the objects
                // monitor.  If it has been reset prior to successfully coming out of
                // TryAquire, dont return, unaquire the installedMonitor, go back through the logic again to grab a
                // a valid monitor.

                if (il2cpp::os::Atomic::ReadPointer(&obj->monitor) != installedMonitor)
                {
                    installedMonitor->Unacquire();
                    continue;
                }

                // Ownership of monitor passed from previously locking thread to us.
                IL2CPP_ASSERT(installedMonitor->recursiveLockingCount == 1);
                IL2CPP_ASSERT(obj->monitor == installedMonitor);

                return true;
            }

            // Getting an immediate lock failed, so if we have a zero timeout now,
            // entering the monitor failed.
            if (timeOutMilliseconds == 0)
                return false;

            // Object was locked by other thread. Let the monitor know we are waiting for a lock.
            installedMonitor->AddCurrentThreadToReadyList();
            if (il2cpp::os::Atomic::ReadPointer(&obj->monitor) != installedMonitor)
            {
                // Another thread deflated the object while we tried to lock it. Get off
                // the monitor.
                // NOTE: By now we may already be dealing with a monitor that is back on the free list
                //  or even installed on an object again.
                installedMonitor->VacateDyingMonitor();

                // NOTE: The "Implementing Fast Java Monitors with Relaxed-Locks" paper describes a path
                //  that may lead to monitors being leaked if the thread currently holding a lock sees our
                //  temporary increment of numWaitingThreads and ends up not deflating the object. However,
                //  we can only ever end up inside this branch here if the locking thread has already decided to
                //  deflate, so I don't see how we can leak here.

                // Retry.
                continue;
            }

            // NOTE: At this point, we are in the waiting line for the monitor. However, the thread currently
            //  locking the monitor may still have already made the decision to deflate the object so we may
            //  still get kicked off the monitor.

            // Wait for the locking thread to signal us.
            while (il2cpp::os::Atomic::ReadPointer(&obj->monitor) == installedMonitor)
            {
                // Try to grab the object for ourselves.
                if (installedMonitor->TryAcquire(currentThreadId))
                {
                    // Ownership of monitor passed from previously locking thread to us.
                    IL2CPP_ASSERT(installedMonitor->recursiveLockingCount == 1);
                    IL2CPP_ASSERT(obj->monitor == installedMonitor);
                    installedMonitor->RemoveCurrentThreadFromReadyList();
                    return true;
                }

                // Wait for owner to signal us.
                il2cpp::os::WaitStatus waitStatus;
                try
                {
                    if (timeOutMilliseconds != std::numeric_limits<uint32_t>::max())
                    {
                        // Perform a timed wait.
                        waitStatus = installedMonitor->semaphore.Wait(timeOutMilliseconds, true);
                    }
                    else
                    {
                        // Perform an infinite wait. We may still be interrupted, however.
                        waitStatus = installedMonitor->semaphore.Wait(true);
                    }
                }
                catch (...)
                {
                    // This is paranoid but in theory a user APC could throw an exception from within Wait().
                    // Just make sure we clean up properly.
                    installedMonitor->RemoveCurrentThreadFromReadyList();
                    throw;
                }

                ////TODO: adjust wait time if we have a Wait() failure and before going another round

                if (waitStatus == kWaitStatusTimeout)
                {
                    // Wait failed. Get us off the list.
                    int newNumWaitingThreads = installedMonitor->RemoveCurrentThreadFromReadyList();

                    // If there are no more waiting threads on this monitor, we need to check for leaking.
                    // This may happen if the locking thread has just been executing a Monitor.Exit(), seen
                    // the positive numWaitingThread count, and decided that it thus cannot deflate the object
                    // and will trigger the semaphore. However, we've just decided to give up waiting, so if
                    // we were the only thread waiting and no one ever attempts to lock the object again, the
                    // monitor will stick around with no one ever deflating the object.
                    //
                    // We solve this by simply trying to acquire ownership of the monitor if we were the last
                    // waiting thread and if that succeeds, we simply change from returning with a time out
                    // failure to returning with a successful lock.
                    if (!newNumWaitingThreads && il2cpp::os::Atomic::ReadPointer(&obj->monitor) == installedMonitor)
                    {
                        if (installedMonitor->TryAcquire(currentThreadId))
                        {
                            // We've successfully acquired a lock on the object.
                            IL2CPP_ASSERT(installedMonitor->recursiveLockingCount == 1);
                            IL2CPP_ASSERT(obj->monitor == installedMonitor);
                            return true;
                        }
                    }

                    // Catch the case where a timeout expired the very moment the owning thread decided to
                    // get us to vacate the monitor by sending an acknowledgement just to make sure.
                    if (il2cpp::os::Atomic::ReadPointer(&obj->monitor) != installedMonitor)
                        installedMonitor->flushAcknowledged.Set();

                    return false;
                }
            }

            // Owner has deflated the object and the monitor is no longer associated with the
            // object we're trying to lock. Signal to the owner that we acknowledge this and
            // move off the monitor.
            installedMonitor->VacateDyingMonitor();
        }

        return false;
    }

    void Monitor::Exit(Il2CppObject* obj)
    {
        // Fetch monitor data.
        MonitorData* monitor = GetMonitorAndThrowIfNotLockedByCurrentThread(obj);

        // We have the object lock. Undo one single invocation of Enter().
        int newLockingCount = monitor->recursiveLockingCount - 1;
        if (newLockingCount > 0)
        {
            // Was recursively locked. Lock still held by us.
            monitor->recursiveLockingCount = newLockingCount;
            return;
        }

        // See if there are already threads ready to take over the lock.
        if (monitor->numThreadsWaitingForSemaphore != 0)
        {
            // Yes, so relinquish ownership of the object and signal the next thread.
            monitor->Unacquire();
            monitor->semaphore.Post();
        }
        else if (monitor->threadsWaitingForPulse)
        {
            // No, but there's threads waiting for a pulse so we can't deflate the object.
            // The wait nodes may already have been abandoned but that is for the pulsing
            // and waiting threads to sort out. Either way, if there ever is going to be a
            // pulse, *some* thread will get around to looking at this monitor again so all
            // we do here is relinquish ownership.
            monitor->Unacquire();

            // there is a race as follows: T1 is our thread and we own monitor lock
            // T1 - checks numThreadsWaitingForSemaphore and sees 0
            // T2 - sees T1 has lock. Increments numThreadsWaitingForSemaphore
            // T2 - tries to acquire monitor, but we hold it
            // T2 - waits on semaphore
            // T1 - we unacquire and wait to be pulsed (if Exit is called from Wait)
            // Result: deadlock as semaphore is never posted
            // Fix: double check 'numThreadsWaitingForSemaphore' after we've unacquired
            // Worst case might be an extra post, which will just incur an additional
            // pass through the loop with an extra attempt to acquire the monitor with a CAS
            if (monitor->numThreadsWaitingForSemaphore != 0)
                monitor->semaphore.Post();
        }
        else
        {
            // Seems like no other thread is interested in the monitor. Deflate the object.
            il2cpp::os::Atomic::ExchangePointer(&obj->monitor, (MonitorData*)NULL);

            // At this point the monitor is no longer associated with the object and we cannot safely
            // "re-attach" it. We need to make sure that all threads still having a reference to the
            // monitor let go of it before we put the monitor back on the free list.
            //
            // IMPORTANT: We still *own* the monitor at this point. No other thread can acquire it and
            //  we must not let go of the monitor until we have kicked all other threads off of it.

            monitor->flushAcknowledged.Reset();
            while (monitor->numThreadsWaitingForSemaphore != 0)
            {
                monitor->semaphore.Post(monitor->numThreadsWaitingForSemaphore);
                // If a thread starts waiting right after we have read numThreadsWaitingForSemaphore,
                // we won't release the semaphore enough times. So don't wait spend a long time waiting
                // for acknowledgement here.
                monitor->flushAcknowledged.Wait(1, false);
            }

            // IMPORTANT: At this point, all other threads must have either already vacated the monitor or
            //   be on a path that makes them vacate the monitor next. The latter may happen if a thread
            //   is stopped right before adding itself to the ready list of our monitor in which case we
            //   will not see the thread on numThreadsWaitingForSemaphore. If we then put the monitor back
            //   on the freelist and then afterwards the other thread is resumed, it will still put itself
            //   on the ready list only to then realize it got the wrong monitor.
            //   So, even for monitors on the free list, we accept that a thread may temporarily add itself
            //   to the wrong monitor's ready list as long as all it does it simply remove itself right after
            //   realizing the mistake.

            // Release monitor back to free list.
            IL2CPP_ASSERT(monitor->owningThreadId == il2cpp::os::Thread::CurrentThreadId());
            monitor->owningThreadId = MonitorData::kHasBeenReturnedToFreeList;
            MonitorData::s_FreeList.Release(monitor);
        }
    }

    static void PulseMonitor(Il2CppObject* obj, bool all = false)
    {
        // Grab monitor.
        MonitorData* monitor = GetMonitorAndThrowIfNotLockedByCurrentThread(obj);

        bool isFirst = true;
        while (true)
        {
            // Grab next waiting thread, if any.
            MonitorData::PulseWaitingListNode* waitNode = monitor->PopNextFromPulseWaitingList();
            if (!waitNode)
                break;

            // Pulse thread.
            waitNode->signalWaitingThread.Set();

            // Stop if we're only supposed to pulse the one thread.
            if (isFirst && !all)
                break;

            isFirst = false;
        }
    }

    void Monitor::Pulse(Il2CppObject* object)
    {
        PulseMonitor(object, false);
    }

    void Monitor::PulseAll(Il2CppObject* object)
    {
        PulseMonitor(object, true);
    }

    void Monitor::Wait(Il2CppObject* object)
    {
        TryWait(object, std::numeric_limits<uint32_t>::max());
    }

    bool Monitor::TryWait(Il2CppObject* object, uint32_t timeoutMilliseconds)
    {
        MonitorData* monitor = GetMonitorAndThrowIfNotLockedByCurrentThread(object);

        // Undo any recursive locking but remember the count so we can restore it
        // after we have re-acquired the lock.
        uint32_t oldLockingCount = monitor->recursiveLockingCount;
        monitor->recursiveLockingCount = 1;

        // Add us to the pulse waiting list for the monitor (except if we won't be
        // waiting for a pulse at all).
        MonitorData::PulseWaitingListNode* waitNode = NULL;
        if (timeoutMilliseconds != 0)
        {
            waitNode = MonitorData::PulseWaitingListNode::Allocate();
            monitor->PushOntoPulseWaitingList(waitNode);
        }

        // Release the monitor.
        Exit(object);
        monitor = NULL;

        // Wait for pulse (if we either have a timeout or are supposed to
        // wait infinitely).
        il2cpp::os::WaitStatus pulseWaitStatus = kWaitStatusSuccess;
        std::exception_ptr exceptionThrownDuringWait = NULL;
        if (timeoutMilliseconds != 0)
        {
            pulseWaitStatus = kWaitStatusFailure;
            try
            {
                il2cpp::vm::ThreadStateSetter state(il2cpp::vm::kThreadStateWaitSleepJoin);
                pulseWaitStatus = waitNode->signalWaitingThread.Wait(timeoutMilliseconds, true);
            }
            catch (...)
            {
                // Exception occurred during wait. Remember exception but continue with reacquisition
                // and cleanup. We re-throw later.
                exceptionThrownDuringWait = std::current_exception();
                pulseWaitStatus = kWaitStatusFailure;
            }
        }

        ////TODO: deal with exception here
        // Reacquire the monitor.
        Enter(object);

        // Monitor *may* have changed.
        monitor = object->monitor;

        // Restore recursion count.
        monitor->recursiveLockingCount = oldLockingCount;

        // Get rid of wait list node.
        if (waitNode)
        {
            // Make sure the node is gone from the wait list. If the pulsing thread already did
            // that, this won't do anything.
            monitor->RemoveFromPulseWaitingList(waitNode);

            // And hand it back for reuse.
            waitNode->Release();
            waitNode = NULL;
        }

        // If the wait was interrupted by an exception (most likely a ThreadInterruptedException),
        // then re-throw now.
        //
        // NOTE: We delay this to until after we've gone through the reacquisition sequence as we
        //  have to guarantee that when Monitor.Wait() exits -- whether successfully or not --, it
        //  still holds a lock. Otherwise a lock() statement around the Wait() will throw an exception,
        //  for example.
        if (exceptionThrownDuringWait)
            std::rethrow_exception(exceptionThrownDuringWait);

        ////TODO: According to MSDN, the timeout indicates whether we reacquired the lock in time
        ////    and not just whether the pulse came in time. Thus the current code is imprecise.
        return (pulseWaitStatus != kWaitStatusTimeout);
    }

    bool Monitor::IsAcquired(Il2CppObject* object)
    {
        MonitorData* monitor = object->monitor;
        if (!monitor)
            return false;

        return monitor->IsAcquired();
    }
} /* namespace vm */
} /* namespace il2cpp */


#endif // IL2CPP_SUPPORT_THREADS
