#pragma once

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
        static bool GetDiskFreeSpaceInternal(Il2CppChar* pathName, int32_t pathName_length, uint64_t* freeBytesAvail, uint64_t* totalNumberOfBytes, uint64_t* totalNumberOfFreeBytes, int32_t* error);
        static Il2CppString* GetDriveFormatInternal(Il2CppChar* rootPathName, int32_t rootPathName_length);
        static uint32_t GetDriveTypeInternal(Il2CppChar* rootPathName, int32_t rootPathName_length);
    };
} /* namespace IO */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
