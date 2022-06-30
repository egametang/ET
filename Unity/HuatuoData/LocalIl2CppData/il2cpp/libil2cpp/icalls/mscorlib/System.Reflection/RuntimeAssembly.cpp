#include "il2cpp-config.h"
#include "RuntimeAssembly.h"
#include "os/File.h"
#include "os/Mutex.h"
#include "os/Path.h"
#include "vm/Array.h"
#include "vm/Assembly.h"
#include "vm/AssemblyName.h"
#include "vm/Exception.h"
#include "vm/Image.h"
#include "vm/Reflection.h"
#include "vm/String.h"
#include "utils/MemoryMappedFile.h"
#include "utils/PathUtils.h"
#include "utils/Runtime.h"
#include "utils/StringUtils.h"

#include <vector>
#include <algorithm>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Reflection
{
    bool RuntimeAssembly::get_global_assembly_cache(Il2CppObject* thisPtr)
    {
        return false;
    }

    bool RuntimeAssembly::get_ReflectionOnly(Il2CppObject* thisPtr)
    {
        // It doesn't mean anything to have a reflection only assembly in il2cpp since we can't load a managed assembly that we didn't convert.  So let's always return false.
        return false;
    }

    bool RuntimeAssembly::GetAotIdInternal(Il2CppArray* aotid)
    {
        return false;
    }

    static void* LoadResourceFile(Il2CppReflectionAssembly* assembly)
    {
        std::string resourcesDirectory = utils::PathUtils::Combine(utils::Runtime::GetDataDir(), utils::StringView<char>("Resources"));

        std::string resourceFileName(assembly->assembly->image->name);
        resourceFileName += "-resources.dat";

        std::string resourceFilePath = utils::PathUtils::Combine(resourcesDirectory, resourceFileName);

        int error = 0;
        os::FileHandle* handle = os::File::Open(resourceFilePath, kFileModeOpen, kFileAccessRead, kFileShareRead, kFileOptionsNone, &error);
        if (error != 0)
            return NULL;

        void* fileBuffer = utils::MemoryMappedFile::Map(handle);

        os::File::Close(handle, &error);
        if (error != 0)
        {
            utils::MemoryMappedFile::Unmap(fileBuffer);
            fileBuffer = NULL;
            return NULL;
        }

        return fileBuffer;
    }

    static os::Mutex s_ResourceDataMutex;

    static void* LoadResourceData(Il2CppReflectionAssembly* assembly, vm::EmbeddedResourceRecord record)
    {
        os::AutoLock lock(&s_ResourceDataMutex);

        void* resourceData = vm::Image::GetCachedResourceData(record.image, record.name);
        if (resourceData != NULL)
            return resourceData;

        void* fileBuffer = vm::Image::GetCachedMemoryMappedResourceFile(assembly);
        if (fileBuffer == NULL)
        {
            fileBuffer = LoadResourceFile(assembly);
            if (fileBuffer == NULL)
                return NULL;

            vm::Image::CacheMemoryMappedResourceFile(assembly, fileBuffer);
        }

        resourceData = (uint8_t*)fileBuffer + record.offset;

        vm::Image::CacheResourceData(record, resourceData);

        return resourceData;
    }

    static int ReadFromBuffer(uint8_t* buffer, int offset, int size, void* output)
    {
        memcpy(output, buffer + offset, size);

        return size;
    }

    static std::vector<vm::EmbeddedResourceRecord> GetResourceRecords(Il2CppReflectionAssembly* assembly)
    {
        std::vector<vm::EmbeddedResourceRecord> resourceRecords;

        void* fileBuffer = vm::Image::GetCachedMemoryMappedResourceFile(assembly);
        if (fileBuffer == NULL)
        {
            fileBuffer = LoadResourceFile(assembly);
            if (fileBuffer == NULL)
                return resourceRecords;

            vm::Image::CacheMemoryMappedResourceFile(assembly, fileBuffer);
        }

        int32_t resourceRecordsSize = 0;
        uint32_t bytesRead = ReadFromBuffer((uint8_t*)fileBuffer, 0, sizeof(int32_t), &resourceRecordsSize);

        int32_t currentResourceDataOffset = bytesRead + resourceRecordsSize;

        int32_t numberOfResources = 0;
        bytesRead += ReadFromBuffer((uint8_t*)fileBuffer, bytesRead, sizeof(int32_t), &numberOfResources);
        for (int resourceIndex = 0; resourceIndex < numberOfResources; ++resourceIndex)
        {
            uint32_t resourceDataSize = 0;
            bytesRead += ReadFromBuffer((uint8_t*)fileBuffer, bytesRead, sizeof(int32_t), &resourceDataSize);

            int32_t resourceNameSize = 0;
            bytesRead += ReadFromBuffer((uint8_t*)fileBuffer, bytesRead, sizeof(int32_t), &resourceNameSize);

            std::vector<char> resourceName(resourceNameSize);
            bytesRead += ReadFromBuffer((uint8_t*)fileBuffer, bytesRead, resourceNameSize, &resourceName[0]);

            resourceRecords.push_back(vm::EmbeddedResourceRecord(assembly->assembly->image, std::string(resourceName.begin(), resourceName.end()), currentResourceDataOffset, resourceDataSize));

            currentResourceDataOffset += resourceDataSize;
        }

        return resourceRecords;
    }

    class ResourceNameMatcher
    {
    public:
        ResourceNameMatcher(const std::string& resourceNameToFind) : needle(resourceNameToFind)
        {}

        bool operator()(const vm::EmbeddedResourceRecord& data) const
        {
            return data.name == needle;
        }

    private:
        std::string needle;
    };

    bool RuntimeAssembly::GetManifestResourceInfoInternal(Il2CppReflectionAssembly* assembly, Il2CppString* name, Il2CppManifestResourceInfo* info)
    {
        std::vector<vm::EmbeddedResourceRecord> resourceRecords = GetResourceRecords(assembly);
        if (std::find_if(resourceRecords.begin(), resourceRecords.end(), ResourceNameMatcher(utils::StringUtils::Utf16ToUtf8(name->chars))) != resourceRecords.end())
        {
            info->location = IL2CPP_RESOURCE_LOCATION_EMBEDDED | IL2CPP_RESOURCE_LOCATION_IN_MANIFEST;

            IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(Assembly::GetManifestResourceInfoInternal, "We have not yet implemented file or assembly resources.");

            return true;
        }

        return false;
    }

    intptr_t RuntimeAssembly::GetManifestResourceInternal(Il2CppReflectionAssembly* assembly, Il2CppString* name, int* size, Il2CppReflectionModule** module)
    {
        std::vector<vm::EmbeddedResourceRecord> resourceRecords = GetResourceRecords(assembly);
        std::vector<vm::EmbeddedResourceRecord>::iterator resource = std::find_if(resourceRecords.begin(), resourceRecords.end(), ResourceNameMatcher(utils::StringUtils::Utf16ToUtf8(name->chars)));
        if (resource != resourceRecords.end())
        {
            *module = vm::Reflection::GetModuleObject(assembly->assembly->image);
            *size = resource->size;
            intptr_t result;
            result = (intptr_t)LoadResourceData(assembly, *resource);
            return result;
        }

        return 0;
    }

    Il2CppObject* RuntimeAssembly::GetFilesInternal(Il2CppObject* thisPtr, Il2CppString* name, bool getResourceModules)
    {
        // Some code paths in mscorlib (e.g. Encoding.GetEncoding) will expect this icall to return NULL. If it
        // instead throws a NotSupportedException, the mscorlib code path changes, and we see some IL2CPP-specific bugs.
        return NULL;
    }

    Il2CppReflectionMethod* RuntimeAssembly::get_EntryPoint(Il2CppReflectionAssembly* self)
    {
        const MethodInfo* method = vm::Image::GetEntryPoint(self->assembly->image);
        if (method == NULL)
            return NULL;

        return il2cpp::vm::Reflection::GetMethodObject(method, NULL);
    }

    Il2CppObject* RuntimeAssembly::GetManifestModuleInternal(Il2CppObject* thisPtr)
    {
        NOT_SUPPORTED_IL2CPP(Assembly::GetManifestModuleInternal, "This icall is not supported by il2cpp.");

        return 0;
    }

    Il2CppArray* RuntimeAssembly::GetModulesInternal(Il2CppReflectionAssembly * thisPtr)
    {
        Il2CppArray* arr = vm::Array::New(il2cpp_defaults.module_class, 1);
        il2cpp_array_setref(arr, 0, vm::Reflection::GetModuleObject(vm::Assembly::GetImage(thisPtr->assembly)));
        return arr;
    }

    Il2CppString* RuntimeAssembly::get_code_base(Il2CppReflectionAssembly* assembly, bool escaped)
    {
        std::string executableDirectory = utils::PathUtils::DirectoryName(os::Path::GetExecutablePath());
        std::replace(executableDirectory.begin(), executableDirectory.end(), '\\', '/');
        return vm::String::New(utils::StringUtils::Printf("file://%s/%s.dll", executableDirectory.c_str(), assembly->assembly->aname.name).c_str());
    }

    Il2CppString* RuntimeAssembly::get_fullname(Il2CppReflectionAssembly* assembly)
    {
        return vm::String::New(vm::AssemblyName::AssemblyNameToString(assembly->assembly->aname).c_str());
    }

    Il2CppString* RuntimeAssembly::get_location(Il2CppObject* thisPtr)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(Assembly::get_location, "Assembly::get_location is not functional on il2cpp");
        return vm::String::New("");
    }

    Il2CppString* RuntimeAssembly::InternalImageRuntimeVersion(Il2CppObject* a)
    {
        NOT_SUPPORTED_IL2CPP(Assembly::InternalImageRuntimeVersion, "This icall is not supported by il2cpp.");

        return 0;
    }

    Il2CppArray* RuntimeAssembly::GetManifestResourceNames(Il2CppReflectionAssembly* assembly)
    {
        std::vector<vm::EmbeddedResourceRecord> resourceRecords = GetResourceRecords(assembly);

        IL2CPP_ASSERT(resourceRecords.size() <= static_cast<size_t>(std::numeric_limits<il2cpp_array_size_t>::max()));
        Il2CppArray* resourceNameArray = vm::Array::New(il2cpp_defaults.string_class, static_cast<il2cpp_array_size_t>(resourceRecords.size()));
        for (size_t i = 0; i < resourceRecords.size(); ++i)
            il2cpp_array_setref(resourceNameArray, i, vm::String::New(resourceRecords[i].name.c_str()));

        return resourceNameArray;
    }
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
