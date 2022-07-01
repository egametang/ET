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
        static void InternalPopDomainRef();
        static Il2CppAppDomain* getCurDomain();
        static Il2CppAppDomain* getRootDomain();
        static int32_t ExecuteAssembly(Il2CppAppDomain* self, Il2CppAssembly* a, Il2CppArray* args);
        static Il2CppObject* GetData(Il2CppAppDomain* self, Il2CppString* name);
        static Il2CppAppContext* InternalGetContext(void);
        static Il2CppAppContext* InternalGetDefaultContext(void);
        static Il2CppString* InternalGetProcessGuid(Il2CppString* newguid);
        static bool InternalIsFinalizingForUnload(int32_t domain_id);
        static void InternalPushDomainRef(mscorlib_System_AppDomain * domain);
        static void InternalPushDomainRefByID(int32_t domain_id);
        static mscorlib_System_Runtime_Remoting_Contexts_Context * InternalSetContext(mscorlib_System_Runtime_Remoting_Contexts_Context * context);
        static mscorlib_System_AppDomain * InternalSetDomain(mscorlib_System_AppDomain * context);
        static mscorlib_System_AppDomain * InternalSetDomainByID(int32_t domain_id);
        static void InternalUnload(int32_t domain_id);
        static Il2CppReflectionAssembly* LoadAssembly(Il2CppAppDomain* ad, Il2CppString* assemblyRef, struct mscorlib_System_Security_Policy_Evidence* evidence, bool refOnly);
        // ==={{ huatuo
        // il2cpp bug! should return Il2CppReflectionAssembly*
        //static Il2CppAssembly* LoadAssemblyRaw(Il2CppAppDomain* self, Il2CppArray* rawAssembly, Il2CppArray* rawSymbolStore, void* /* System.Security.Policy.Evidence */ securityEvidence, bool refonly);
        static Il2CppReflectionAssembly* LoadAssemblyRaw(Il2CppAppDomain* self, Il2CppArray* rawAssembly, Il2CppArray* rawSymbolStore, void* /* System.Security.Policy.Evidence */ securityEvidence, bool refonly);
        // ===}} huatuo
        static void SetData(Il2CppAppDomain* self, Il2CppString* name, Il2CppObject* data);
        static Il2CppAppDomain* createDomain(Il2CppString*, mscorlib_System_AppDomainSetup*);
        static Il2CppString * getFriendlyName(Il2CppAppDomain* ad);
        static Il2CppAppDomainSetup* getSetup(Il2CppAppDomain* domain);
        static Il2CppArray* GetAssemblies(Il2CppAppDomain* ad, bool refonly);
        static void DoUnhandledException(Il2CppObject* _this, Il2CppException* e);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
