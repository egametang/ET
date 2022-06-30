#include "il2cpp-config.h"
#include "utils/dynamic_array.h"
#include "vm/Profiler.h"

#if IL2CPP_ENABLE_PROFILER

namespace il2cpp
{
namespace vm
{
    struct ProfilerDesc
    {
        Il2CppProfiler* profiler;
        Il2CppProfileFlags events;
        Il2CppProfileFunc shutdownCallback;

        Il2CppProfileMethodFunc methodEnterCallback;
        Il2CppProfileMethodFunc methodLeaveCallback;

        Il2CppProfileAllocFunc allocationCallback;

        Il2CppProfileGCFunc gcEventCallback;
        Il2CppProfileGCResizeFunc gcHeapResizeCallback;

        Il2CppProfileFileIOFunc fileioCallback;

        Il2CppProfileThreadFunc threadStartCallback;
        Il2CppProfileThreadFunc threadEndCallback;
    };

    typedef il2cpp::utils::dynamic_array<ProfilerDesc*> ProfilersVec;
    static ProfilersVec s_profilers;
    Il2CppProfileFlags Profiler::s_profilerEvents;

    void Profiler::Install(Il2CppProfiler *prof, Il2CppProfileFunc shutdownCallback)
    {
        ProfilerDesc* desc = (ProfilerDesc*)calloc(1, sizeof(ProfilerDesc));
        desc->profiler = prof;
        desc->shutdownCallback = shutdownCallback;
        s_profilers.push_back(desc);
    }

    void Profiler::SetEvents(Il2CppProfileFlags events)
    {
        Il2CppProfileFlags value = IL2CPP_PROFILE_NONE;
        if (s_profilers.size())
            s_profilers.back()->events = events;
        for (ProfilersVec::iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
            value = (Il2CppProfileFlags)(value | (*iter)->events);
        s_profilerEvents = value;
    }

    void Profiler::InstallEnterLeave(Il2CppProfileMethodFunc enter, Il2CppProfileMethodFunc fleave)
    {
        if (!s_profilers.size())
            return;
        s_profilers.back()->methodEnterCallback = enter;
        s_profilers.back()->methodLeaveCallback = fleave;
    }

    void Profiler::InstallAllocation(Il2CppProfileAllocFunc callback)
    {
        if (!s_profilers.size())
            return;
        s_profilers.back()->allocationCallback = callback;
    }

    void Profiler::InstallGC(Il2CppProfileGCFunc callback, Il2CppProfileGCResizeFunc heap_resize_callback)
    {
        if (!s_profilers.size())
            return;
        s_profilers.back()->gcEventCallback = callback;
        s_profilers.back()->gcHeapResizeCallback = heap_resize_callback;
    }

    void Profiler::InstallFileIO(Il2CppProfileFileIOFunc callback)
    {
        if (!s_profilers.size())
            return;
        s_profilers.back()->fileioCallback = callback;
    }

    void Profiler::InstallThread(Il2CppProfileThreadFunc start, Il2CppProfileThreadFunc end)
    {
        if (!s_profilers.size())
            return;
        s_profilers.back()->threadStartCallback = start;
        s_profilers.back()->threadEndCallback = end;
    }

    void Profiler::Allocation(Il2CppObject *obj, Il2CppClass *klass)
    {
        for (ProfilersVec::const_iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
        {
            if (((*iter)->events & IL2CPP_PROFILE_ALLOCATIONS) && (*iter)->allocationCallback)
                (*iter)->allocationCallback((*iter)->profiler, obj, klass);
        }
    }

    void Profiler::MethodEnter(const MethodInfo *method)
    {
        for (ProfilersVec::const_iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
        {
            if (((*iter)->events & IL2CPP_PROFILE_ENTER_LEAVE) && (*iter)->methodEnterCallback)
                (*iter)->methodEnterCallback((*iter)->profiler, method);
        }
    }

    void Profiler::MethodExit(const MethodInfo *method)
    {
        for (ProfilersVec::const_iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
        {
            if (((*iter)->events & IL2CPP_PROFILE_ENTER_LEAVE) && (*iter)->methodLeaveCallback)
                (*iter)->methodLeaveCallback((*iter)->profiler, method);
        }
    }

    void Profiler::GCEvent(Il2CppGCEvent eventType)
    {
        for (ProfilersVec::const_iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
        {
            if (((*iter)->events & IL2CPP_PROFILE_GC) && (*iter)->gcEventCallback)
                (*iter)->gcEventCallback((*iter)->profiler, eventType, 0);
        }
    }

    void Profiler::GCHeapResize(int64_t newSize)
    {
        for (ProfilersVec::const_iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
        {
            if (((*iter)->events & IL2CPP_PROFILE_GC) && (*iter)->gcEventCallback)
                (*iter)->gcHeapResizeCallback((*iter)->profiler, newSize);
        }
    }

    void Profiler::FileIO(Il2CppProfileFileIOKind kind, int count)
    {
        for (ProfilersVec::const_iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
        {
            if (((*iter)->events & IL2CPP_PROFILE_FILEIO) && (*iter)->fileioCallback)
                (*iter)->fileioCallback((*iter)->profiler, kind, count);
        }
    }

    void Profiler::ThreadStart(unsigned long tid)
    {
        for (ProfilersVec::const_iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
        {
            if (((*iter)->events & IL2CPP_PROFILE_THREADS) && (*iter)->threadStartCallback)
                (*iter)->threadStartCallback((*iter)->profiler, tid);
        }
    }

    void Profiler::ThreadEnd(unsigned long tid)
    {
        for (ProfilersVec::const_iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
        {
            if (((*iter)->events & IL2CPP_PROFILE_THREADS) && (*iter)->threadEndCallback)
                (*iter)->threadEndCallback((*iter)->profiler, tid);
        }
    }

    void Profiler::Shutdown()
    {
        for (ProfilersVec::iterator iter = s_profilers.begin(); iter != s_profilers.end(); iter++)
        {
            if ((*iter)->shutdownCallback)
                (*iter)->shutdownCallback((*iter)->profiler);
        }
    }
} /* namespace vm */
} /* namespace il2cpp */

#endif // IL2CPP_ENABLE_PROFILER
