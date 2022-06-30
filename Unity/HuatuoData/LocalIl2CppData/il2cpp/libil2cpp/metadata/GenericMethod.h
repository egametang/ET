#pragma once

#include <string>
#include "il2cpp-class-internals.h"
#include "os/Mutex.h"

struct MethodInfo;
struct Il2CppGenericMethod;
struct Il2CppGenericContext;
struct Il2CppGenericInst;

namespace il2cpp
{
namespace metadata
{
    class GenericMethod
    {
    public:
        // exported

    public:
        //internal
        static const MethodInfo* GetMethod(const MethodInfo* methodDefinition, const Il2CppGenericInst* classInst, const Il2CppGenericInst* methodInst);
        static const MethodInfo* GetMethod(const Il2CppGenericMethod* gmethod);
        static MethodInfo* AllocateNewMethodInfo(const MethodInfo* methodDefinition, const Il2CppGenericInst* classInst, const Il2CppGenericInst* methodInst);
        static void GetVirtualInvokeData(const MethodInfo* methodDefinition, const Il2CppGenericInst* classInst, const Il2CppGenericInst* methodInst, VirtualInvokeData* invokeData);
        static bool IsGenericAmbiguousMethodInfo(const MethodInfo* method);
        static Il2CppMethodPointer GetVirtualCallMethodPointer(const MethodInfo* method);
        static const Il2CppGenericContext* GetContext(const Il2CppGenericMethod* gmethod);
        static std::string GetFullName(const Il2CppGenericMethod* gmethod);

        static void ClearStatics();
        static const Il2CppRGCTXData* InflateRGCTX(const MethodInfo* method);

    private:
        static const MethodInfo* GetMethod(const Il2CppGenericMethod* gmethod, bool copyMethodPtr);
        static const Il2CppRGCTXData* InflateRGCTXLocked(const Il2CppGenericMethod* gmethod, const il2cpp::os::FastAutoLock& lock);
    };
} /* namespace vm */
} /* namespace il2cpp */
