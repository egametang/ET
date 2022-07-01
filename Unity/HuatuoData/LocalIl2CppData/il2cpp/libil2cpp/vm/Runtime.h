#pragma once

#include <stdint.h>
#include <vector>
#include <string>
#include "il2cpp-config.h"
#include "il2cpp-metadata.h"
#include "il2cpp-object-internals.h"
#include "metadata/GenericMethod.h"
#include "vm/Exception.h"
#include "vm/Class.h"
#include "vm/MetadataCache.h"
#include "utils/StringUtils.h"

struct Il2CppArray;
struct Il2CppDelegate;
struct Il2CppObject;
struct MethodInfo;
struct Il2CppClass;

typedef void (*MetadataInitializerCleanupFunc)();

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Runtime
    {
    public:
        static bool Init(const char* domainName = "IL2CPP Root Domain");
        static void Shutdown();
        static bool IsShuttingDown();
        static void SetConfigDir(const char *path);
        static void SetConfigUtf16(const Il2CppChar* executablePath);
        static void SetConfig(const char* executablePath);
        static void SetUnityTlsInterface(const void* unitytlsInterface);
        static std::string GetConfigDir();
        static const void* GetUnityTlsInterface();
        static const char *GetFrameworkVersion();
        static const MethodInfo* GetDelegateInvoke(Il2CppClass* klass);
        static Il2CppObject* DelegateInvoke(Il2CppDelegate *obj, void **params, Il2CppException **exc);
        static Il2CppObject* Invoke(const MethodInfo *method, void *obj, void **params, Il2CppException **exc);
        static Il2CppObject* InvokeWithThrow(const MethodInfo *method, void *obj, void **params);
        static Il2CppObject* InvokeConvertArgs(const MethodInfo *method, void *obj, Il2CppObject **params, int paramCount, Il2CppException **exc);
        static Il2CppObject* InvokeArray(const MethodInfo *method, void *obj, Il2CppArray *params, Il2CppException **exc);
        static void ObjectInit(Il2CppObject* object);
        static void ObjectInitException(Il2CppObject* object, Il2CppException **exc);
        static void SetUnhandledExceptionPolicy(Il2CppRuntimeUnhandledExceptionPolicy value);

        static const MethodInfo* GetGenericVirtualMethod(const MethodInfo* methodDefinition, const MethodInfo* inflatedMethod);
        static void RaiseExecutionEngineExceptionIfMethodIsNotFound(const MethodInfo* method);
        static void AlwaysRaiseExecutionEngineException(const MethodInfo* method);

    public:
        // internal
        static Il2CppRuntimeUnhandledExceptionPolicy GetUnhandledExceptionPolicy();
        static void UnhandledException(Il2CppException* exc);
        static void ClassInit(Il2CppClass *klass);

        static const char *GetBundledMachineConfig();
        static void RegisterBundledMachineConfig(const char *config_xml);

        static int32_t GetExitCode();
        static void SetExitCode(int32_t value);

    private:
        static void CallUnhandledExceptionDelegate(Il2CppDomain* domain, Il2CppDelegate* delegate, Il2CppException* exc);
        static Il2CppObject* CreateUnhandledExceptionEventArgs(Il2CppException* exc);

        static void VerifyApiVersion();

        static inline void RaiseExecutionEngineExceptionIfMethodIsNotFound(const MethodInfo* method, const Il2CppGenericMethod* genericMethod)
        {
            if (method->methodPointer == NULL)
                RaiseExecutionEngineException(metadata::GenericMethod::GetFullName(genericMethod).c_str());
        }

        static inline void RaiseExecutionEngineException(const char* methodFullName)
        {
            Exception::Raise(Exception::GetExecutionEngineException(utils::StringUtils::Printf("Attempting to call method '%s' for which no ahead of time (AOT) code was generated.", methodFullName).c_str()));
        }
    };
} /* namespace vm */
} /* namespace il2cpp */
