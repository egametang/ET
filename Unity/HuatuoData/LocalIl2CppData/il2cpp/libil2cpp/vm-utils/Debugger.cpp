#include "il2cpp-config.h"

#if IL2CPP_MONO_DEBUGGER

#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-metadata.h"
#include "Debugger.h"
#include "os/Thread.h"
#include "os/c-api/Allocator.h"
#include "os/SocketBridge.h"

#include "vm/Assembly.h"
#include "vm/Image.h"
#include "vm/MetadataCache.h"
#include "vm/Method.h"
#include "vm/StackTrace.h"
#include "vm/Thread.h"
#include "utils/Environment.h"
#include "utils/dynamic_array.h"
#include "utils/StringUtils.h"
#include "utils/StringViewUtils.h"
#include "utils/Il2CppHashMap.h"
#include "utils/HashUtils.h"
#include "VmStringUtils.h"

#include <deque>
#include <string>
#include <algorithm>

il2cpp::os::ThreadLocalValue s_ExecutionContexts; // Il2CppThreadUnwindState*

struct MonoDebuggerRuntimeCallbacks
{
    void(*il2cpp_debugger_save_thread_context)(Il2CppThreadUnwindState* context, int frameCountAdjust);
    void(*il2cpp_debugger_free_thread_context)(Il2CppThreadUnwindState* context);
};

struct DebuggerTransport
{
    const char *name;
    void(*connect) (const char *address);
    int (*wait_for_attach) (void);
    void(*close1) (void);
    void(*close2) (void);
    int (*send) (void *buf, int len);
    int(*recv) (void *buf, int len);
};

struct MonoContext;
extern "C"
{
    void mono_debugger_agent_parse_options(const char *options);
    void mono_debugger_agent_init_minimal();
    void mono_debugger_agent_init();
    void mono_debugger_run_debugger_thread_func(void* arg);
    void debugger_agent_single_step_from_context(MonoContext *ctx, Il2CppSequencePoint* sequencePoint);
    void mono_debugger_il2cpp_init();
    void unity_debugger_agent_breakpoint(Il2CppSequencePoint* sequencePoint);
    void unity_debugger_agent_pausepoint();
    void mono_debugger_install_runtime_callbacks(MonoDebuggerRuntimeCallbacks* cbs);
    int32_t unity_debugger_agent_is_global_breakpoint_active();
    int32_t unity_debugger_agent_is_single_stepping();
    void unity_debugger_agent_handle_exception(Il2CppException *exc);
    int32_t il2cpp_mono_methods_match(const MethodInfo* left, const MethodInfo* right);
    void mono_debugger_agent_user_break();
    int32_t mono_debugger_agent_debug_log_is_enabled();
    void mono_debugger_agent_debug_log(int level, Il2CppString *category, Il2CppString *message);
    int32_t unity_pause_point_active();
    void il2cpp_save_current_thread_context_func_exit();
    void mono_debugger_agent_register_transport(DebuggerTransport *trans);

    void* il2cpp_malloc(size_t size)
    {
        return IL2CPP_MALLOC(size);
    }
}

static const Il2CppDebuggerMetadataRegistration *g_metadata;

namespace il2cpp
{
namespace utils
{
    static os::Thread* s_DebuggerThread;
    static bool s_IsDebuggerAttached = false;
    static bool s_IsDebuggerInitialized = false;
    static std::string s_AgentOptions;

    static os::Mutex s_Il2CppMonoLoaderLock(false);
    static uint64_t s_Il2CppMonoLoaderLockThreadId = 0;

    static Il2CppMonoInterpCallbacks s_InterpCallbacks;

    typedef dynamic_array<Il2CppSequencePoint*> SequencePointList;
    typedef Il2CppHashMap<const MethodInfo*, SequencePointList*, il2cpp::utils::PointerHash<MethodInfo> > MethodToSequencePointsMap;
    static MethodToSequencePointsMap s_methodToSequencePoints;

    typedef dynamic_array<Il2CppCatchPoint*> CatchPointList;
    typedef Il2CppHashMap<const MethodInfo*, CatchPointList*, il2cpp::utils::PointerHash<MethodInfo> > MethodToCatchPointsMap;
    static MethodToCatchPointsMap s_methodToCatchPoints;

    typedef Il2CppHashMap<const MethodInfo*, const MethodInfo*, il2cpp::utils::PointerHash<MethodInfo> > MethodToMethodMap;
    static MethodToMethodMap s_uninflatedMethodToInflated;
    SequencePointList s_sequencePoints;

    typedef dynamic_array<const char*> FileNameList;
    typedef Il2CppHashMap<const Il2CppClass*, FileNameList, il2cpp::utils::PointerHash<Il2CppClass> > TypeSourceFileMap;
    static TypeSourceFileMap *s_typeSourceFiles;

    static MethodToSequencePointsMap::const_iterator GetMethodSequencePointIterator(const MethodInfo *method);

    static void* FrameGetArg(Il2CppSequencePointExecutionContext* frame, int pos)
    {
        return frame->params[pos];
    }

    static void* FrameGetLocal(Il2CppSequencePointExecutionContext* frame, int pos)
    {
        return frame->locals[pos];
    }

    static void* FrameGetThis(Il2CppSequencePointExecutionContext* frame)
    {
        return *frame->thisArg;
    }

    static void InitializeInterpCallbacks()
    {
        s_InterpCallbacks.frame_get_arg = FrameGetArg;
        s_InterpCallbacks.frame_get_local = FrameGetLocal;
        s_InterpCallbacks.frame_get_this = FrameGetThis;
    }

    void Debugger::RegisterMetadata(const Il2CppDebuggerMetadataRegistration *data)
    {
        g_metadata = data;
    }

#if defined(RUNTIME_IL2CPP)
    void breakpoint_callback(Il2CppSequencePoint* sequencePoint)
    {
        unity_debugger_agent_breakpoint(sequencePoint);
    }

    void pausepoint_callback()
    {
        unity_debugger_agent_pausepoint();
    }

#endif

    static void InitializeMonoSoftDebugger(const char* options)
    {
#if defined(RUNTIME_IL2CPP)
        InitializeInterpCallbacks();

        os::SocketBridge::WaitForInitialization();

        mono_debugger_il2cpp_init();
        mono_debugger_agent_parse_options(options);
        mono_debugger_agent_init();

        s_typeSourceFiles = new TypeSourceFileMap();

        MonoDebuggerRuntimeCallbacks cbs;
        cbs.il2cpp_debugger_save_thread_context = Debugger::SaveThreadContext;
        cbs.il2cpp_debugger_free_thread_context = Debugger::FreeThreadContext;
        mono_debugger_install_runtime_callbacks(&cbs);

        il2cpp::utils::Debugger::RegisterCallbacks(breakpoint_callback, pausepoint_callback);

        register_allocator(il2cpp_malloc);

        s_IsDebuggerInitialized = true;
#else
        IL2CPP_ASSERT(0 && "The managed debugger is only supported for the libil2cpp runtime backend.");
#endif
    }

    void Debugger::SetAgentOptions(const char* options)
    {
        s_AgentOptions = options;
    }

    void Debugger::RegisterTransport(const Il2CppDebuggerTransport* transport)
    {
#if defined(RUNTIME_IL2CPP)
        DebuggerTransport mono_transport;
        mono_transport.name = transport->name;
        mono_transport.connect = transport->connect;
        mono_transport.wait_for_attach = transport->wait_for_attach;
        mono_transport.close1 = transport->close1;
        mono_transport.close2 = transport->close2;
        mono_transport.send = transport->send;
        mono_transport.recv = transport->recv;
        mono_debugger_agent_register_transport(&mono_transport);
#endif
    }

    void Debugger::InitializeTypeSourceFileMap()
    {
        Il2CppClass* lastKlass = NULL;
        Il2CppClass *klass = NULL;
        FileNameList files;

        vm::AssemblyVector* assemblies = vm::Assembly::GetAllAssemblies();
        for (vm::AssemblyVector::const_iterator  iter = assemblies->begin(); iter != assemblies->end(); ++iter)
        {
            const Il2CppImage* image = vm::Assembly::GetImage(*iter);
            const Il2CppDebuggerMetadataRegistration* debuggerMetadata = image->codeGenModule->debuggerMetadata;
            if (debuggerMetadata == NULL)
                continue;
            for (int i = 0; i < debuggerMetadata->numTypeSourceFileEntries; ++i)
            {
                Il2CppTypeSourceFilePair& pair = debuggerMetadata->typeSourceFiles[i];
                const char *file = debuggerMetadata->sequencePointSourceFiles[pair.sourceFileIndex].file;

                klass = il2cpp::vm::MetadataCache::GetTypeInfoFromTypeSourcePair(image, &pair);
                if (klass != lastKlass && lastKlass != NULL)
                {
                    s_typeSourceFiles->add(lastKlass, files);
                    files.clear();
                }
                lastKlass = klass;
                files.push_back(file);
            }
        }

        if (files.size() > 0)
            s_typeSourceFiles->add(klass, files);
    }

    void Debugger::Start()
    {
        if (s_IsDebuggerInitialized)
        {
            vm::MetadataCache::InitializeAllMethodMetadata();
            InitializeTypeSourceFileMap();
            InitializeMethodToSequencePointMap();
            InitializeMethodToCatchPointMap();
            Debugger::StartDebuggerThread();
        }
    }

    static bool TryInitializeDebugger(const std::string& options)
    {
        if (StringUtils::StartsWith(STRING_TO_STRINGVIEW(options), "--debugger-agent"))
        {
            InitializeMonoSoftDebugger(options.c_str() + options.find("=") + 1);
            return true;
        }

        return false;
    }

    void Debugger::Init()
    {
        bool debuggerIsInitialized = false;
        if (!s_AgentOptions.empty())
        {
            debuggerIsInitialized = TryInitializeDebugger(s_AgentOptions);
        }
        else
        {
            const std::vector<UTF16String>& args = Environment::GetMainArgs();
            for (std::vector<UTF16String>::const_iterator arg = args.begin(); arg != args.end(); ++arg)
            {
                std::string argument = StringUtils::Utf16ToUtf8(*arg);
                debuggerIsInitialized = TryInitializeDebugger(argument);
                if (debuggerIsInitialized)
                    break;
            }
        }

        if (!debuggerIsInitialized)
            mono_debugger_agent_init_minimal();
    }

    static Debugger::OnBreakPointHitCallback s_BreakCallback;
    static Debugger::OnPausePointHitCallback s_PauseCallback;
    void Debugger::RegisterCallbacks(OnBreakPointHitCallback breakCallback, OnPausePointHitCallback pauseCallback)
    {
        s_BreakCallback = breakCallback;
        s_PauseCallback = pauseCallback;
    }

    void Debugger::StartDebuggerThread()
    {
#if defined(RUNTIME_IL2CPP)
        // This thread is allocated here once and never deallocated.
        s_DebuggerThread = new os::Thread();
        s_DebuggerThread->Run(mono_debugger_run_debugger_thread_func, NULL);
#else
        IL2CPP_ASSERT(0 && "The managed debugger is only supported for the libil2cpp runtime backend.");
#endif
    }

    Il2CppThreadUnwindState* Debugger::GetThreadStatePointer()
    {
        if (!s_IsDebuggerInitialized)
            return NULL;

        Il2CppThreadUnwindState* unwindState;
        s_ExecutionContexts.GetValue(reinterpret_cast<void**>(&unwindState));

        return unwindState;
    }

    void Debugger::SaveThreadContext(Il2CppThreadUnwindState* context, int frameCountAdjust)
    {
        if (!s_IsDebuggerInitialized)
            return;

        IL2CPP_ASSERT(!IsDebuggerThread(os::Thread::GetCurrentThread()));
    }

    void Debugger::FreeThreadContext(Il2CppThreadUnwindState* context)
    {
        if (!s_IsDebuggerInitialized)
            return;

        IL2CPP_ASSERT(!IsDebuggerThread(os::Thread::GetCurrentThread()));
    }

    void Debugger::OnBreakPointHit(Il2CppSequencePoint *sequencePoint)
    {
#if defined(RUNTIME_IL2CPP)
        if (IsGlobalBreakpointActive() || unity_debugger_agent_is_single_stepping())
        {
            debugger_agent_single_step_from_context(NULL, sequencePoint);
        }
        else if (s_BreakCallback)
        {
            s_BreakCallback(sequencePoint);
        }
        else
            IL2CPP_DEBUG_BREAK();
#else
        IL2CPP_ASSERT(0 && "The managed debugger is only supported for the libil2cpp runtime backend.");
#endif
    }

    void Debugger::OnPausePointHit()
    {
#if defined(RUNTIME_IL2CPP)
        if (s_PauseCallback)
            s_PauseCallback();
#else
        IL2CPP_ASSERT(0 && "The managed debugger is only supported for the libil2cpp runtime backend.");
#endif
    }

    bool Debugger::IsGlobalBreakpointActive()
    {
        if (!Debugger::GetIsDebuggerAttached())
            return false;
#if defined(RUNTIME_IL2CPP)
        return unity_debugger_agent_is_global_breakpoint_active();
#else
        IL2CPP_ASSERT(0 && "The managed debugger is only supported for the libil2cpp runtime backend.");
        return false;
#endif
    }

    bool Debugger::GetIsDebuggerAttached()
    {
        return s_IsDebuggerAttached;
    }

    void Debugger::SetIsDebuggerAttached(bool attached)
    {
        s_IsDebuggerAttached = attached;
    }

    bool Debugger::IsDebuggerThread(os::Thread* thread)
    {
        return thread == s_DebuggerThread;
    }

    static void InitializeUnwindState(Il2CppThreadUnwindState* unwindState, uint32_t frameCapacity)
    {
        unwindState->frameCount = 0;
        unwindState->frameCapacity = frameCapacity;
        unwindState->executionContexts = (Il2CppSequencePointExecutionContext**)calloc(frameCapacity, sizeof(Il2CppSequencePointExecutionContext*));
    }

    void Debugger::AllocateThreadLocalData()
    {
        Il2CppThreadUnwindState* unwindState;
        s_ExecutionContexts.GetValue(reinterpret_cast<void**>(&unwindState));
        if (unwindState == NULL)
        {
            unwindState = (Il2CppThreadUnwindState*)calloc(1, sizeof(Il2CppThreadUnwindState));
            InitializeUnwindState(unwindState, 512);
            s_ExecutionContexts.SetValue(unwindState);
        }
    }

    void Debugger::GrowFrameCapacity(Il2CppThreadUnwindState* unwindState)
    {
        // Create a new unwind state object to hold the large array of execution context pointers
        Il2CppThreadUnwindState newUnwindState;
        InitializeUnwindState(&newUnwindState, unwindState->frameCapacity * 2);

        // Copy the existing execution context pointers into the new one
        memcpy(newUnwindState.executionContexts, unwindState->executionContexts, unwindState->frameCapacity * sizeof(Il2CppSequencePointExecutionContext*));

        // Free the existing one
        free(unwindState->executionContexts);

        // Set the new data into the existing one, so the client can keep its object reference
        unwindState->frameCapacity = newUnwindState.frameCapacity;
        unwindState->executionContexts = newUnwindState.executionContexts;
    }

    void Debugger::FreeThreadLocalData()
    {
        if (s_IsDebuggerInitialized)
        {
            Il2CppThreadUnwindState* unwindState;
            s_ExecutionContexts.GetValue(reinterpret_cast<void**>(&unwindState));
            s_ExecutionContexts.SetValue(NULL);
            if (unwindState != NULL)
            {
                free(unwindState->executionContexts);
                free(unwindState);
            }
        }
    }

    Il2CppSequencePoint* Debugger::GetSequencePoint(const Il2CppImage* image, size_t id)
    {
        if (image->codeGenModule->debuggerMetadata->numSequencePoints == 0)
            return NULL;

        return &image->codeGenModule->debuggerMetadata->sequencePoints[id];
    }

    struct SeqPointIter
    {
        SequencePointList::iterator iter, end;
    };

    Il2CppSequencePoint* Debugger::GetSequencePoints(const MethodInfo* method, void** iter)
    {
        if (!iter)
            return NULL;

        SeqPointIter *pIter = NULL;

        if (!*iter)
        {
            MethodToSequencePointsMap::const_iterator entry = GetMethodSequencePointIterator(method);
            if (entry == s_methodToSequencePoints.end())
                return NULL;

            pIter = new SeqPointIter();
            *iter = pIter;
            pIter->iter = entry->second->begin();
            pIter->end = entry->second->end();
            return *(pIter->iter);
        }

        pIter = (SeqPointIter*)*iter;
        pIter->iter++;
        if (pIter->iter != pIter->end)
        {
            return *(pIter->iter);
        }
        else
        {
            delete pIter;
            *iter = NULL;
        }

        return NULL;
    }

    Il2CppSequencePoint* Debugger::GetAllSequencePoints(void* *iter)
    {
        size_t index = (size_t)(intptr_t)*iter;

        if (index >= s_sequencePoints.size())
            return NULL;

        Il2CppSequencePoint* retVal = s_sequencePoints[index];
        *iter = (void*)(intptr_t)(index + 1);
        return retVal;
    }

    MethodToSequencePointsMap::const_iterator GetMethodSequencePointIterator(const MethodInfo *method)
    {
        if (method->is_inflated)
            method = method->genericMethod->methodDefinition;

        MethodToSequencePointsMap::const_iterator entry = s_methodToSequencePoints.find(method);
        if (entry == s_methodToSequencePoints.end())
        {
            //the sequence point map doesn't have uninflated methods, need to map the incoming method to
            //an inflated method.  il2cpp_mono_methods_match has a case for this.
            MethodToMethodMap::const_iterator inflated = s_uninflatedMethodToInflated.find(method);
            if (inflated != s_uninflatedMethodToInflated.end())
            {
                method = inflated->second;
            }
            else
            {
                for (MethodToSequencePointsMap::iterator mapIter = s_methodToSequencePoints.begin(); mapIter != s_methodToSequencePoints.end(); ++mapIter)
                {
                    if (il2cpp_mono_methods_match(method, mapIter->first))
                    {
                        s_uninflatedMethodToInflated.add(method, mapIter->first);
                        method = mapIter->first;
                        break;
                    }
                }
            }

            return s_methodToSequencePoints.find(method);
        }

        return entry;
    }

    Il2CppSequencePoint* Debugger::GetSequencePoint(const Il2CppImage* image, Il2CppCatchPoint* cp)
    {
        const MethodInfo *method = GetCatchPointMethod(image, cp);

        MethodToSequencePointsMap::const_iterator entry = GetMethodSequencePointIterator(method);
        if (entry == s_methodToSequencePoints.end())
            return NULL;

        SequencePointList::iterator iter = entry->second->begin();
        while (iter != entry->second->end())
        {
            if ((*iter)->ilOffset >= cp->ilOffset)
                return *iter;

            ++iter;
        }

        return NULL;
    }

    struct CatchPointIter
    {
        CatchPointList::iterator iter, end;
    };

    Il2CppCatchPoint* Debugger::GetCatchPoints(const MethodInfo* method, void** iter)
    {
        if (!iter)
            return NULL;

        CatchPointIter *pIter = NULL;

        if (!*iter)
        {
            MethodToCatchPointsMap::const_iterator entry = s_methodToCatchPoints.find(method);
            if (entry == s_methodToCatchPoints.end())
                return NULL;

            pIter = new CatchPointIter();
            *iter = pIter;
            pIter->iter = entry->second->begin();
            pIter->end = entry->second->end();
            return *(pIter->iter);
        }

        pIter = (CatchPointIter*)*iter;
        pIter->iter++;
        if (pIter->iter != pIter->end)
        {
            return *(pIter->iter);
        }
        else
        {
            delete pIter;
            *iter = NULL;
        }

        return NULL;
    }

    void Debugger::HandleException(Il2CppException *exc)
    {
        if (s_IsDebuggerInitialized)
            unity_debugger_agent_handle_exception(exc);
    }

    bool SequencePointOffsetLess(const Il2CppSequencePoint *s1, const Il2CppSequencePoint *s2)
    {
        return s1->ilOffset < s2->ilOffset;
    }

    bool CatchPointOffsetLess(const Il2CppCatchPoint *c1, const Il2CppCatchPoint *c2)
    {
        return c1->ilOffset < c2->ilOffset;
    }

    void Debugger::InitializeMethodToSequencePointMap()
    {
        size_t count = 0;
        vm::AssemblyVector* assemblies = vm::Assembly::GetAllAssemblies();
        for (vm::AssemblyVector::const_iterator iter = assemblies->begin(); iter != assemblies->end(); ++iter)
        {
            const Il2CppImage* image = vm::Assembly::GetImage(*iter);
            const Il2CppDebuggerMetadataRegistration* debuggerMetadata = image->codeGenModule->debuggerMetadata;
            if (debuggerMetadata == NULL)
                continue;
            for (int i = 0; i < debuggerMetadata->numSequencePoints; ++i)
            {
                Il2CppSequencePoint& seqPoint = debuggerMetadata->sequencePoints[i];
                const MethodInfo *method = GetSequencePointMethod(image, &seqPoint);

                if (method != NULL)
                {
                    IL2CPP_ASSERT(!method->is_inflated && "Only open generic methods should have sequence points");

                    SequencePointList* list;
                    MethodToSequencePointsMap::iterator existingList = s_methodToSequencePoints.find(method);
                    if (existingList == s_methodToSequencePoints.end())
                    {
                        list = new SequencePointList();
                        s_methodToSequencePoints.add(method, list);
                    }
                    else
                    {
                        list = existingList->second;
                    }
                    list->push_back(&seqPoint);
                    count++;
                }
            }
        }

        s_sequencePoints.reserve(count);

        for (MethodToSequencePointsMap::iterator methods = s_methodToSequencePoints.begin(); methods != s_methodToSequencePoints.end(); ++methods)
        {
            SequencePointList *seqPoints = methods->second;
            std::sort(seqPoints->begin(), seqPoints->end(), SequencePointOffsetLess);
            s_sequencePoints.insert(s_sequencePoints.end(), seqPoints->begin(), seqPoints->end());
        }
    }

    void Debugger::InitializeMethodToCatchPointMap()
    {
        vm::AssemblyVector* assemblies = vm::Assembly::GetAllAssemblies();
        for (vm::AssemblyVector::const_iterator iter = assemblies->begin(); iter != assemblies->end(); ++iter)
        {
            const Il2CppImage* image = vm::Assembly::GetImage(*iter);
            const Il2CppDebuggerMetadataRegistration* debuggerMetadata = image->codeGenModule->debuggerMetadata;
            if (debuggerMetadata == NULL)
                continue;
            for (int i = 0; i < debuggerMetadata->numCatchPoints; ++i)
            {
                Il2CppCatchPoint& catchPoint = debuggerMetadata->catchPoints[i];
                const MethodInfo *method = GetCatchPointMethod(image, &catchPoint);

                if (method != NULL)
                {
                    CatchPointList* list;
                    MethodToCatchPointsMap::iterator existingList = s_methodToCatchPoints.find(method);
                    if (existingList == s_methodToCatchPoints.end())
                    {
                        list = new CatchPointList();
                        s_methodToCatchPoints.add(method, list);
                    }
                    else
                    {
                        list = existingList->second;
                    }
                    list->push_back(&catchPoint);
                }
            }
        }

        for (MethodToCatchPointsMap::iterator methods = s_methodToCatchPoints.begin(); methods != s_methodToCatchPoints.end(); ++methods)
        {
            CatchPointList *catchPoints = methods->second;
            std::sort(catchPoints->begin(), catchPoints->end(), CatchPointOffsetLess);
        }
    }

    const char** Debugger::GetTypeSourceFiles(const Il2CppClass *klass, int& count)
    {
        TypeSourceFileMap::iterator it = s_typeSourceFiles->find(klass);
        if (it == s_typeSourceFiles->end())
        {
            count = 0;
            return NULL;
        }

        count = (int)it->second.size();
        return it->second.data();
    }

    void Debugger::UserBreak()
    {
        if (s_IsDebuggerAttached)
            mono_debugger_agent_user_break();
    }

    bool Debugger::IsLoggingEnabled()
    {
        return mono_debugger_agent_debug_log_is_enabled();
    }

    void Debugger::Log(int level, Il2CppString *category, Il2CppString *message)
    {
        if (s_IsDebuggerAttached)
            mono_debugger_agent_debug_log(level, category, message);
    }

    bool Debugger::IsPausePointActive()
    {
        return unity_pause_point_active();
    }

    void Debugger::CheckPausePoint()
    {
        if (il2cpp::utils::Debugger::IsPausePointActive())
            il2cpp::utils::Debugger::OnPausePointHit();
    }

    const MethodInfo* Debugger::GetSequencePointMethod(const Il2CppImage* image, Il2CppSequencePoint *seqPoint)
    {
        if (seqPoint == NULL)
            return NULL;

        return il2cpp::vm::MetadataCache::GetMethodInfoFromSequencePoint(image, seqPoint);
    }

    const MethodInfo* Debugger::GetCatchPointMethod(const Il2CppImage* image, Il2CppCatchPoint *catchPoint)
    {
        if (catchPoint == NULL)
            return NULL;

        return il2cpp::vm::MetadataCache::GetMethodInfoFromCatchPoint(image, catchPoint);
    }

    const char* Debugger::GetLocalName(const MethodInfo* method, int32_t index)
    {
        const Il2CppDebuggerMetadataRegistration* debuggerMetadata = method->klass->image->codeGenModule->debuggerMetadata;
        return debuggerMetadata->methodExecutionContextInfoStrings[index];
    }

    const Il2CppMethodScope* Debugger::GetLocalScope(const MethodInfo* method, int32_t index)
    {
        const Il2CppDebuggerMetadataRegistration* debuggerMetadata = method->klass->image->codeGenModule->debuggerMetadata;
        return &debuggerMetadata->methodScopes[index];
    }

    void Debugger::GetMethodExecutionContextInfo(const MethodInfo* method, uint32_t* executionContextInfoCount, const Il2CppMethodExecutionContextInfo **executionContextInfo, const Il2CppMethodHeaderInfo **headerInfo, const Il2CppMethodScope **scopes)
    {
        if (il2cpp::vm::Method::IsInflated(method))
            method = il2cpp::vm::MetadataCache::GetGenericMethodDefinition(method);
        const Il2CppDebuggerMetadataRegistration* debuggerMetadata = method->klass->image->codeGenModule->debuggerMetadata;

        Il2CppMethodExecutionContextInfoIndex *index = &debuggerMetadata->methodExecutionContextInfoIndexes[GetTokenRowId(method->token) - 1];
        if (index->count != -1)
        {
            *executionContextInfoCount = index->count;
            *executionContextInfo = &debuggerMetadata->methodExecutionContextInfos[index->startIndex];
        }
        else
        {
            *executionContextInfoCount = 0;
            *executionContextInfo = NULL;
        }
        *headerInfo = &debuggerMetadata->methodHeaderInfos[GetTokenRowId(method->token) - 1];
        *scopes = &debuggerMetadata->methodScopes[(*headerInfo)->startScope];
    }

    void Debugger::GetStackFrames(void* context)
    {
        il2cpp::vm::StackFrames* stackFrames = static_cast<il2cpp::vm::StackFrames*>(context);

        Il2CppThreadUnwindState* unwindState = GetThreadStatePointer();
        if (unwindState == NULL)
            return; // There might not be any managed code executing yet.

        for (uint32_t i = 0; i < unwindState->frameCount; ++i)
        {
            const MethodInfo* method = unwindState->executionContexts[i]->method;
            if (method != NULL)
            {
                Il2CppStackFrameInfo frameInfo = { 0 };
                frameInfo.method = method;
                if (unwindState->executionContexts[i]->currentSequencePoint != NULL)
                {
                    const Il2CppDebuggerMetadataRegistration* debuggerMetadata = method->klass->image->codeGenModule->debuggerMetadata;
                    if (debuggerMetadata != NULL)
                    {
                        int32_t sourceFileIndex = unwindState->executionContexts[i]->currentSequencePoint->sourceFileIndex;
                        frameInfo.filePath = debuggerMetadata->sequencePointSourceFiles[sourceFileIndex].file;
                        frameInfo.sourceCodeLineNumber = unwindState->executionContexts[i]->currentSequencePoint->lineStart;
                        frameInfo.ilOffset = unwindState->executionContexts[i]->currentSequencePoint->ilOffset;
                    }
                }
                stackFrames->push_back(frameInfo);
            }
        }
    }

    void Debugger::AcquireLoaderLock()
    {
        s_Il2CppMonoLoaderLock.Lock();
        s_Il2CppMonoLoaderLockThreadId = os::Thread::CurrentThreadId();
    }

    void Debugger::ReleaseLoaderLock()
    {
        s_Il2CppMonoLoaderLockThreadId = 0;
        s_Il2CppMonoLoaderLock.Unlock();
    }

    bool Debugger::LoaderLockIsOwnedByThisThread()
    {
        return s_Il2CppMonoLoaderLockThreadId == os::Thread::CurrentThreadId();
    }

    Il2CppMonoInterpCallbacks* Debugger::GetInterpCallbacks()
    {
        return &s_InterpCallbacks;
    }
}
}

#else

#include "Debugger.h"
#include "os/Debug.h"

namespace il2cpp
{
namespace utils
{
    bool Debugger::GetIsDebuggerAttached()
    {
        return os::Debug::IsDebuggerPresent();
    }
}
}

#endif // IL2CPP_MONO_DEBUGGER
