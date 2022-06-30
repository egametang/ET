#pragma once

#include "il2cpp-object-internals.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace Core
{
namespace System
{
namespace IO
{
namespace MemoryMappedFiles
{
    class LIBIL2CPP_CODEGEN_API MemoryMapImpl
    {
    public:
        static bool Unmap(intptr_t mmap_handle);
        static int32_t MapInternal(intptr_t handle, int64_t offset, int64_t* size, int32_t access, intptr_t* mmap_handle, intptr_t* base_address);
        static intptr_t OpenFileInternal(Il2CppChar* path, int32_t path_length, int32_t mode, Il2CppChar* mapName, int32_t mapName_length, int64_t* capacity, int32_t access, int32_t options, int32_t* error);
        static intptr_t OpenHandleInternal(intptr_t handle, Il2CppChar* mapName, int32_t mapName_length, int64_t* capacity, int32_t access, int32_t options, int32_t* error);
        static void CloseMapping(intptr_t handle);
        static void ConfigureHandleInheritability(intptr_t handle, int32_t inheritability);
        static void Flush(intptr_t file_handle);
    };
} // namespace MemoryMappedFiles
} // namespace IO
} // namespace System
} // namespace Core
} // namespace System
} // namespace icalls
} // namespace il2cpp
