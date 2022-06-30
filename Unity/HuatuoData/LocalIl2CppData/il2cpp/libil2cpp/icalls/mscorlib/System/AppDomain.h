#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct mscorlib_System_AppDomain;
struct mscorlib_System_AppDomainSetup;
struct mscorlib_System_Runtime_Remoting_Contexts_Context;
struct mscorlib_System_Security_Policy_Evidence;
struct mscorlib_System_Reflection_Assembly;

struct Il2CppObject;
struct Il2CppString;
struct Il2CppArray;
struct Il2CppAssembly;
struct Il2CppAppDomain;
struct Il2CppAppDomainSetup;
struct Il2CppReflectionAssembly;
struct Il2CppAppContext;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API AppDomain
    {
    public:
        static Il2CppObject* createDomain(Il2CppString* friendlyName, Il2CppObject* info);
        static Il2CppAppDomain* getCurDomain();
        static Il2CppAppDomain* getRootDomain();
        static Il2CppObject* InternalSetDomain(Il2CppObject* context);
        static Il2CppObject* InternalSetDomainByID(int32_t domain_id);
        static Il2CppAppDomainSetup* getSetup(Il2CppAppDomain* thisPtr);
        static bool InternalIsFinalizingForUnload(int32_t domain_id);
        static int32_t ExecuteAssembly(Il2CppObject* thisPtr, Il2CppObject* a, Il2CppArray* args);
        static Il2CppObject* GetData(Il2CppAppDomain* thisPtr, Il2CppString* name);
        static Il2CppReflectionAssembly* LoadAssembly(Il2CppAppDomain* thisPtr, Il2CppString* assemblyRef, Il2CppObject* securityEvidence, bool refOnly, int32_t* stackMark);
        static Il2CppObject* LoadAssemblyRaw(Il2CppObject* thisPtr, Il2CppArray* rawAssembly, Il2CppArray* rawSymbolStore, Il2CppObject* securityEvidence, bool refonly);
        static Il2CppArray* GetAssemblies(Il2CppObject* thisPtr, bool refOnly);
        static Il2CppAppContext* InternalGetContext();
        static Il2CppAppContext* InternalGetDefaultContext();
        static Il2CppObject* InternalSetContext(Il2CppObject* context);
        static Il2CppString* getFriendlyName(Il2CppAppDomain* thisPtr);
        static Il2CppString* InternalGetProcessGuid(Il2CppString* newguid);
        static void DoUnhandledException(Il2CppObject* thisPtr, Il2CppException* e);
        static void InternalPopDomainRef();
        static void InternalPushDomainRef(Il2CppObject* domain);
        static void InternalPushDomainRefByID(int32_t domain_id);
        static void InternalUnload(int32_t domain_id);
        static void SetData(Il2CppAppDomain* thisPtr, Il2CppString* name, Il2CppObject* data);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
