#pragma once
#include "il2cpp-class-internals.h"

struct Il2CppSequencePoint;
struct Il2CppCatchPoint;
struct Il2CppSequencePointExecutionContext;
struct Il2CppThreadUnwindState;

typedef void(*DebugInfoInitialization)();
typedef void(*ThreadCallback)(void*, uintptr_t);

typedef struct Il2CppSequencePointExecutionContext
{
    const MethodInfo* method;
    void** thisArg;
    void** params;
    void** locals;
    Il2CppSequencePoint* currentSequencePoint;
    int32_t tryId;

#ifdef __cplusplus
    Il2CppSequencePointExecutionContext(const MethodInfo* method, void** thisArg, void** params, void** locals);
    ~Il2CppSequencePointExecutionContext();
#endif //__cplusplus
} Il2CppSequencePointExecutionContext;


typedef struct Il2CppThreadUnwindState
{
    Il2CppSequencePointExecutionContext** executionContexts;
    uint32_t frameCount;
    uint32_t frameCapacity;
} Il2CppThreadUnwindState;

typedef int32_t (*Il2CppMonoInternalStackWalk) (void /*MonoStackFrameInfo*/ *frame, void /*MonoContext*/ *ctx, void* data);

struct Il2CppMonoInterpCallbacks
{
    void* (*create_method_pointer) (MethodInfo *method, void /*MonoError*/ *error);
    Il2CppObject* (*runtime_invoke) (MethodInfo *method, void *obj, void **params, Il2CppObject **exc, void /*MonoError*/  *error);
    void (*init_delegate) (Il2CppDelegate *del);
#ifndef DISABLE_REMOTING
    void* (*get_remoting_invoke) (void* imethod, void /*MonoError*/  *error);
#endif
    void* (*create_trampoline) (Il2CppDomain *domain, MethodInfo *method, void /*MonoError*/  *error);
    void (*walk_stack_with_ctx) (Il2CppMonoInternalStackWalk func, void /*MonoContext*/  *ctx, int32_t /*MonoUnwindOptions*/ options, void *user_data);
    void (*set_resume_state) (void /*MonoJitTlsData*/ *jit_tls, Il2CppException *ex, void /*MonoJitExceptionInfo*/ *ei, Il2CppSequencePointExecutionContext* interp_frame, void* handler_ip);
    int32_t (*run_finally) (void /*MonoStackFrameInfo*/  *frame, int clause_index, void* handler_ip);
    int32_t (*run_filter) (void /*MonoStackFrameInfo*/  *frame, Il2CppException *ex, int clause_index, void* handler_ip);
    void (*frame_iter_init) (void /*MonoInterpStackIter*/ *iter, void* interp_exit_data);
    int32_t (*frame_iter_next) (void /*MonoInterpStackIter*/ *iter, void /*MonoStackFrameInfo*/  *frame);
    void* /*MonoJitInfo*/ (*find_jit_info) (Il2CppDomain *domain, MethodInfo *method);
    void (*set_breakpoint) (void /*MonoJitInfo*/ *jinfo, void* ip);
    void (*clear_breakpoint) (void /*MonoJitInfo*/ *jinfo, void* ip);
    void* /*MonoJitInfo*/ (*frame_get_jit_info) (Il2CppSequencePointExecutionContext* frame);
    void* (*frame_get_ip) (Il2CppSequencePointExecutionContext* frame);
    void* (*frame_get_arg) (Il2CppSequencePointExecutionContext* frame, int pos);
    void* (*frame_get_local) (Il2CppSequencePointExecutionContext* frame, int pos);
    void* (*frame_get_this) (Il2CppSequencePointExecutionContext* frame);
    Il2CppSequencePointExecutionContext* (*frame_get_parent) (Il2CppSequencePointExecutionContext* frame);
    void (*start_single_stepping) ();
    void (*stop_single_stepping) ();
};

#ifdef __cplusplus
extern "C"
{
    int32_t unity_sequence_point_active_entry(Il2CppSequencePoint *seqPoint);
    int32_t unity_sequence_point_active_exit(Il2CppSequencePoint *seqPoint);
    extern int32_t g_unity_pause_point_active;
}
#include <stdint.h>
#include "os/Atomic.h"
#include "os/ThreadLocalValue.h"

#undef IsLoggingEnabled

extern il2cpp::os::ThreadLocalValue s_ExecutionContexts;

namespace il2cpp
{
namespace os
{
    class Thread;
}
namespace utils
{
    class Debugger
    {
    public:
        static void RegisterMetadata(const Il2CppDebuggerMetadataRegistration *data);
        static void SetAgentOptions(const char* options);
        static void RegisterTransport(const Il2CppDebuggerTransport* transport);
        static void Init();
        static void Start();
        static void StartDebuggerThread();

        static inline void PushExecutionContext(Il2CppSequencePointExecutionContext* executionContext)
        {
            Il2CppThreadUnwindState* unwindState;
            s_ExecutionContexts.GetValue(reinterpret_cast<void**>(&unwindState));

            if (unwindState->frameCount == unwindState->frameCapacity)
                GrowFrameCapacity(unwindState);

            unwindState->executionContexts[unwindState->frameCount] = executionContext;
            unwindState->frameCount++;
        }

        static inline void PopExecutionContext()
        {
            Il2CppThreadUnwindState* unwindState;
            s_ExecutionContexts.GetValue(reinterpret_cast<void**>(&unwindState));

            IL2CPP_ASSERT(unwindState->frameCount > 0);
            unwindState->frameCount--;
        }

        typedef void(*OnBreakPointHitCallback) (Il2CppSequencePoint* sequencePoint);
        typedef void (*OnPausePointHitCallback) ();
        static void RegisterCallbacks(OnBreakPointHitCallback breakCallback, OnPausePointHitCallback pauseCallback);
        static Il2CppThreadUnwindState* GetThreadStatePointer();
        static void SaveThreadContext(Il2CppThreadUnwindState* context, int frameCountAdjust);
        static void FreeThreadContext(Il2CppThreadUnwindState* context);
        static void OnBreakPointHit(Il2CppSequencePoint *sequencePoint);
        static void OnPausePointHit();
        static bool IsGlobalBreakpointActive();
        static bool GetIsDebuggerAttached();
        static void SetIsDebuggerAttached(bool attached);
        static bool IsDebuggerThread(os::Thread* thread);
        static void AllocateThreadLocalData();
        static void FreeThreadLocalData();
        static Il2CppSequencePoint* GetSequencePoint(const Il2CppImage* image, size_t id);
        static Il2CppSequencePoint* GetSequencePoints(const MethodInfo* method, void**iter);
        static Il2CppSequencePoint* GetSequencePoint(const Il2CppImage* image, Il2CppCatchPoint* cp);
        static Il2CppCatchPoint* GetCatchPoints(const MethodInfo* method, void**iter);
        static Il2CppSequencePoint* GetAllSequencePoints(void* *iter);
        static void HandleException(Il2CppException *exc);
        static const char** GetTypeSourceFiles(const Il2CppClass *klass, int& count);
        static void UserBreak();
        static bool IsLoggingEnabled();
        static void Log(int level, Il2CppString *category, Il2CppString *message);

        static inline bool IsSequencePointActive(Il2CppSequencePoint *seqPoint)
        {
            return il2cpp::os::Atomic::LoadRelaxed(&seqPoint->isActive) || g_unity_pause_point_active;
        }

        static inline bool IsSequencePointActiveEntry(Il2CppSequencePoint *seqPoint)
        {
            return unity_sequence_point_active_entry(seqPoint);
        }

        static inline bool IsSequencePointActiveExit(Il2CppSequencePoint *seqPoint)
        {
            return unity_sequence_point_active_exit(seqPoint);
        }

        static bool IsPausePointActive();
        static const MethodInfo* GetSequencePointMethod(const Il2CppImage* image, Il2CppSequencePoint *seqPoint);
        static const MethodInfo* GetCatchPointMethod(const Il2CppImage* image, Il2CppCatchPoint *catchPoint);

        static inline void CheckSequencePoint(Il2CppSequencePointExecutionContext* executionContext, Il2CppSequencePoint* seqPoint)
        {
            if (IsSequencePointActive(seqPoint))
            {
                executionContext->currentSequencePoint = seqPoint;
                OnBreakPointHit(seqPoint);
            }
        }

        static inline void CheckSequencePointEntry(Il2CppSequencePointExecutionContext* executionContext, Il2CppSequencePoint* seqPoint)
        {
            if (IsSequencePointActiveEntry(seqPoint))
            {
                executionContext->currentSequencePoint = seqPoint;
                OnBreakPointHit(seqPoint);
            }
        }

        static inline void CheckSequencePointExit(Il2CppSequencePointExecutionContext* executionContext, Il2CppSequencePoint* seqPoint)
        {
            if (IsSequencePointActiveExit(seqPoint))
            {
                executionContext->currentSequencePoint = seqPoint;
                OnBreakPointHit(seqPoint);
            }
        }

        static void CheckPausePoint();

        static const char* GetLocalName(const MethodInfo* method, int32_t index);
        static const Il2CppMethodScope* GetLocalScope(const MethodInfo* method, int32_t index);

        static void GetMethodExecutionContextInfo(const MethodInfo* method, uint32_t* executionContextInfoCount, const Il2CppMethodExecutionContextInfo **executionContextInfo, const Il2CppMethodHeaderInfo **headerInfo, const Il2CppMethodScope **scopes);

        // The context parameter here is really il2cpp::vm::StackFrames*. We don't want to include vm/StackTrace.h in this file,
        // as this one is included in generated code.
        static void GetStackFrames(void* context);

        static void AcquireLoaderLock();
        static void ReleaseLoaderLock();
        static bool LoaderLockIsOwnedByThisThread();

        static Il2CppMonoInterpCallbacks* GetInterpCallbacks();

    private:
        static os::ThreadLocalValue s_IsGlobalBreakpointActive;
        static void InitializeMethodToSequencePointMap();
        static void InitializeTypeSourceFileMap();
        static void InitializeMethodToCatchPointMap();
        static void GrowFrameCapacity(Il2CppThreadUnwindState* unwindState);
    };
}
}

inline Il2CppSequencePointExecutionContext::Il2CppSequencePointExecutionContext(const MethodInfo* method, void** thisArg, void** params, void** locals)
    : method(method),
    thisArg(thisArg),
    params(params),
    locals(locals),
    currentSequencePoint(NULL),
    tryId(-1)
{
    il2cpp::utils::Debugger::PushExecutionContext(this);
}

inline Il2CppSequencePointExecutionContext::~Il2CppSequencePointExecutionContext()
{
    il2cpp::utils::Debugger::PopExecutionContext();
    // il2cpp_save_current_thread_context_func_exit();
}

#endif //__cplusplus
