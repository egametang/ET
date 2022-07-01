#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppObject;
struct Il2CppDelegate;
struct Il2CppReflectionType;
struct Il2CppReflectionMethod;
struct Il2CppReflectionField;
struct Il2CppArray;
struct Il2CppException;
struct Il2CppReflectionModule;
struct Il2CppAssembly;
struct Il2CppAssemblyName;
struct Il2CppAppDomain;

typedef int32_t MonoIOError;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace IO
{
    class LIBIL2CPP_CODEGEN_API DriveInfo
    {
    public:
        static uint32_t GetDriveTypeInternal(Il2CppString* rootPathName);
        static bool GetDiskFreeSpaceInternal(Il2CppString* pathName, uint64_t* freeBytesAvail, uint64_t* totalNumberOfBytes, uint64_t* totalNumberOfFreeBytes, MonoIOError* error);
        static Il2CppString* GetDriveFormat(Il2CppString* rootPathName);
    };
} /* namespace IO */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
