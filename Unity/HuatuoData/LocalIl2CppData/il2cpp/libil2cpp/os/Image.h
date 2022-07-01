#pragma once

namespace il2cpp
{
namespace os
{
namespace Image
{
    void Initialize();
    void* GetImageBase();
#if IL2CPP_ENABLE_NATIVE_INSTRUCTION_POINTER_EMISSION
    void GetImageUUID(char* uuid);
#endif
    bool IsInManagedSection(void*ip);
    bool ManagedSectionExists();
    void SetManagedSectionStartAndEnd(void* start, void* end);
}
}
}
