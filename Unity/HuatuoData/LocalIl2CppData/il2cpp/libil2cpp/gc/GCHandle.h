#pragma once

#include "utils/Expected.h"
#include "utils/Il2CppError.h"

#include <stdint.h>

struct Il2CppObject;

namespace il2cpp
{
namespace gc
{
    enum GCHandleType
    {
        HANDLE_WEAK,
        HANDLE_WEAK_TRACK,
        HANDLE_NORMAL,
        HANDLE_PINNED
    };

    class LIBIL2CPP_CODEGEN_API GCHandle
    {
    public:
        // external
        static uint32_t New(Il2CppObject *obj, bool pinned);
        static utils::Expected<uint32_t> NewWeakref(Il2CppObject *obj, bool track_resurrection);
        static Il2CppObject* GetTarget(uint32_t gchandle);
        static GCHandleType GetHandleType(uint32_t gcHandle);
        static void Free(uint32_t gchandle);
    public:
        //internal
        static utils::Expected<uint32_t> GetTargetHandle(Il2CppObject * obj, int32_t handle, int32_t type);
        typedef void(*WalkGCHandleTargetsCallback)(Il2CppObject* obj, void* context);
        static void WalkStrongGCHandleTargets(WalkGCHandleTargetsCallback callback, void* context);
    };
} /* gc */
} /* il2cpp */
