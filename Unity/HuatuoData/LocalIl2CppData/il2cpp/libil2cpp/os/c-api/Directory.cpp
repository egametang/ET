#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/Directory.h"
#include "os/c-api/Directory-c-api.h"
#include "Allocator.h"
#include "utils/StringUtils.h"
#include "utils/StringViewUtils.h"

#include <string>

extern "C"
{
    const char* UnityPalDirectoryGetCurrent(int* error)
    {
        return Allocator::CopyToAllocatedStringBuffer(il2cpp::os::Directory::GetCurrent(error));
    }

    int32_t UnityPalDirectorySetCurrent(const char* path, int* error)
    {
        return il2cpp::os::Directory::SetCurrent(path, error);
    }

    int32_t UnityPalDirectoryCreate(const char* path, int *error)
    {
        return il2cpp::os::Directory::Create(path, error);
    }

    int32_t UnityPalDirectoryRemove(const char* path, int *error)
    {
        return il2cpp::os::Directory::Remove(path, error);
    }

    void UnityPalDirectoryGetFileSystemEntries(const char* path, const char* pathWithPattern, int32_t attrs, int32_t mask, int* error, char*** entries, int32_t* numEntries)
    {
        std::set<std::string> localSet = il2cpp::os::Directory::GetFileSystemEntries(path, pathWithPattern, attrs, mask, error);

        if (localSet.empty())
        {
            *numEntries = 0;
            (*entries) = NULL;
            return;
        }
        // First allocate a new array that can hold all of the entries in the std::set
        char** entryArray = NULL;
        *numEntries = static_cast<int32_t>(localSet.size());
        entryArray = (char**)Allocator::Allocate(*numEntries * sizeof(char*));

        IL2CPP_ASSERT(entryArray);

        // Copy each string in the std::set into a newly allocated char* slot of the
        // array.
        std::set<std::string>::const_iterator it;
        int arrayIndex = 0;
        for (it = localSet.begin(); it != localSet.end(); ++it, ++arrayIndex)
        {
            entryArray[arrayIndex] = (char*)Allocator::Allocate(it->length() * sizeof(char) + 1);

            IL2CPP_ASSERT(entryArray[arrayIndex]);

            strncpy(entryArray[arrayIndex], it->c_str(), it->length() + 1);
        }

        (*entries) = entryArray;
    }

    UnityPalFindHandle* UnityPalDirectoryFindHandleNew(const char* searchPathWithPattern)
    {
        Il2CppNativeString pattern(il2cpp::utils::StringUtils::Utf8ToNativeString(searchPathWithPattern));
        return new il2cpp::os::Directory::FindHandle(STRING_TO_STRINGVIEW(pattern));
    }

    void UnityPalDirectoryFindHandleDelete(UnityPalFindHandle* object)
    {
        IL2CPP_ASSERT(object);
        delete object;
    }

    int32_t UnityPalDirectoryCloseOSHandle(UnityPalFindHandle* object)
    {
        IL2CPP_ASSERT(object);
        return object->CloseOSHandle();
    }

    void* UnityPalDirectoryGetOSHandle(UnityPalFindHandle* object)
    {
        IL2CPP_ASSERT(object);
        return object->osHandle;
    }

    UnityPalErrorCode UnityPalDirectoryFindFirstFile(UnityPalFindHandle* findHandle, const char* searchPathWithPattern, char** resultFileName, int32_t* resultAttributes)
    {
        Il2CppNativeString pattern(il2cpp::utils::StringUtils::Utf8ToNativeString(searchPathWithPattern));
        Il2CppNativeString nativeFileName;
        UnityPalErrorCode retVal = il2cpp::os::Directory::FindFirstFile(findHandle, STRING_TO_STRINGVIEW(pattern), &nativeFileName, resultAttributes);
        *resultFileName = Allocator::CopyToAllocatedStringBuffer(il2cpp::utils::StringUtils::NativeStringToUtf8(nativeFileName));
        return retVal;
    }

    UnityPalErrorCode UnityPalDirectoryFindNextFile(UnityPalFindHandle*  findHandle, char** resultFileName, int32_t* resultAttributes)
    {
        Il2CppNativeString nativeFileName;
        UnityPalErrorCode retVal = il2cpp::os::Directory::FindNextFile(findHandle, &nativeFileName, resultAttributes);
        *resultFileName = Allocator::CopyToAllocatedStringBuffer(il2cpp::utils::StringUtils::NativeStringToUtf8(nativeFileName));
        return retVal;
    }
}

#endif
