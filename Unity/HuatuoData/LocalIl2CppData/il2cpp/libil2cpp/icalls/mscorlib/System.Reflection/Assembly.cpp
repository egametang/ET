#include "il2cpp-config.h"
#include <stddef.h>
#include <vector>
#include <algorithm>
#include "icalls/mscorlib/System.Reflection/Assembly.h"
#include "icalls/mscorlib/System.Reflection/Module.h"
#include "utils/StringUtils.h"
#include "utils/PathUtils.h"
#include "os/File.h"
#include "os/Mutex.h"
#include "os/Path.h"
#include "utils/Memory.h"
#include "utils/MemoryMappedFile.h"
#include "utils/Runtime.h"
#include "vm/Array.h"
#include "vm/Assembly.h"
#include "vm/AssemblyName.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Field.h"
#include "vm/Image.h"
#include "vm/MetadataCache.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Type.h"
#include "vm/Array.h"
#include "il2cpp-class-internals.h"
#include "mono-structs.h"
#include <limits>

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
    Il2CppString* Assembly::get_fullname(Il2CppReflectionAssembly *assembly)
    {
        return vm::String::New(vm::AssemblyName::AssemblyNameToString(assembly->assembly->aname).c_str());
    }

    Il2CppString*  Assembly::get_location(Il2CppReflectionAssembly *assembly)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(Assembly::get_location, "Assembly::get_location is not functional on il2cpp");
        return vm::String::New("");
    }

    Il2CppReflectionAssembly* Assembly::GetEntryAssembly()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(Assembly::GetEntryAssembly, "In the case of Unity this is always NULL. For a normal exe this is the assembly with Main.");
        return NULL;
    }

    Il2CppReflectionAssembly* Assembly::GetExecutingAssembly()
    {
        return vm::Reflection::GetAssemblyObject(vm::Image::GetExecutingImage()->assembly);
    }

#define CHECK_IF_NULL(v)    \
    if ( (v) == NULL && throwOnError ) \
        vm::Exception::Raise (vm::Exception::GetTypeLoadException (info)); \
    if ( (v) == NULL ) \
        return NULL;

    Il2CppReflectionType* Assembly::InternalGetType(Il2CppReflectionAssembly *assembly, mscorlib_System_Reflection_Module *, Il2CppString* name, bool throwOnError, bool ignoreCase)
    {
        std::string str = utils::StringUtils::Utf16ToUtf8(utils::StringUtils::GetChars(name));

        il2cpp::vm::TypeNameParseInfo info;
        il2cpp::vm::TypeNameParser parser(str, info, false);

        if (!parser.Parse())
        {
            if (throwOnError)
                vm::Exception::Raise(vm::Exception::GetTypeLoadException());
            else
                return NULL;
        }

        CHECK_IF_NULL(assembly);

        Il2CppImage *image = (Il2CppImage*)vm::Assembly::GetImage(assembly->assembly);

        CHECK_IF_NULL(image);

        Il2CppClass *klass = vm::Image::FromTypeNameParseInfo(image, info, ignoreCase);

        CHECK_IF_NULL(klass);

        il2cpp::vm::Class::Init(klass);

        const Il2CppType *type = vm::Class::GetType(klass, info);

        CHECK_IF_NULL(type);

        return il2cpp::vm::Reflection::GetTypeObject(type);
    }

    Il2CppReflectionAssembly* Assembly::load_with_partial_name(Il2CppString* name, mscorlib_System_Security_Policy_Evidence*  evidence)
    {
        const Il2CppAssembly* assembly = vm::Assembly::GetLoadedAssembly(il2cpp::utils::StringUtils::Utf16ToUtf8(name->chars).c_str());
        if (assembly != NULL)
            return vm::Reflection::GetAssemblyObject(assembly);

        return NULL;
    }

    void Assembly::FillName(Il2CppReflectionAssembly * ass, mscorlib_System_Reflection_AssemblyName * aname)
    {
        Il2CppObject* assemblyNameObject = reinterpret_cast<Il2CppObject*>(aname);
        Il2CppClass* assemblyNameType = assemblyNameObject->klass;
        const Il2CppAssemblyName* assemblyName = &ass->assembly->aname;

        // System.Reflection.AssemblyName is not protected from stripping. Since this call will be used
        // very rarely, instead of including that type to stripper excludes, let's instead set fields only
        // if they're there.
        FieldInfo* assemblyNameField = vm::Class::GetFieldFromName(assemblyNameType, "name");
        FieldInfo* codebaseField = vm::Class::GetFieldFromName(assemblyNameType, "codebase");

        if (assemblyNameField != NULL)
            vm::Field::SetValue(assemblyNameObject, assemblyNameField, vm::String::New(assemblyName->name));

        if (codebaseField != NULL)
            vm::Field::SetValue(assemblyNameObject, codebaseField, get_code_base(ass, false));

        FieldInfo* field = vm::Class::GetFieldFromName(assemblyNameType, "major");
        if (field != NULL)
        {
            int32_t major = assemblyName->major;
            vm::Field::SetValue(assemblyNameObject, field, &major);
        }

        field = vm::Class::GetFieldFromName(assemblyNameType, "minor");
        if (field != NULL)
        {
            int32_t minor = assemblyName->minor;
            vm::Field::SetValue(assemblyNameObject, field, &minor);
        }

        field = vm::Class::GetFieldFromName(assemblyNameType, "build");
        if (field != NULL)
        {
            int32_t build = assemblyName->build;
            vm::Field::SetValue(assemblyNameObject, field, &build);
        }

        field = vm::Class::GetFieldFromName(assemblyNameType, "revision");
        if (field != NULL)
        {
            int32_t revision = assemblyName->revision;
            vm::Field::SetValue(assemblyNameObject, field, &revision);
        }

        field = vm::Class::GetFieldFromName(assemblyNameType, "cultureinfo");
        if (field != NULL)
        {
            Il2CppClass* cultureInfoType = vm::Class::FromIl2CppType(field->type);
            FieldInfo* invariantCultureField = vm::Class::GetFieldFromName(cultureInfoType, "invariant_culture_info");
            Il2CppObject* invariantCulture = NULL;

            if (invariantCultureField != NULL)
                vm::Field::StaticGetValue(invariantCultureField, &invariantCulture);

            vm::Field::SetValue(assemblyNameObject, field, invariantCulture);
        }

        field = vm::Class::GetFieldFromName(assemblyNameType, "flags");
        if (field != NULL)
            vm::Field::SetValue(assemblyNameObject, field, const_cast<void*>((const void*)&assemblyName->flags));

        field = vm::Class::GetFieldFromName(assemblyNameType, "hashalg");
        if (field != NULL)
            vm::Field::SetValue(assemblyNameObject, field, const_cast<void*>((const void*)&assemblyName->hash_alg));

        field = vm::Class::GetFieldFromName(assemblyNameType, "keypair");
        if (field != NULL)
            vm::Field::SetValue(assemblyNameObject, field, NULL);

        field = vm::Class::GetFieldFromName(assemblyNameType, "publicKey");
        if (field != NULL)
            vm::Field::SetValue(assemblyNameObject, field, vm::Array::New(il2cpp_defaults.byte_class, 0));

        field = vm::Class::GetFieldFromName(assemblyNameType, "keyToken");
        if (field != NULL)
        {
            Il2CppArray* keyTokenManaged = NULL;

            // Set it to non-null only if public key token is not all zeroes
            for (int i = 0; i < kPublicKeyByteLength; i++)
            {
                if (assemblyName->public_key_token[i] != 0)
                {
                    keyTokenManaged = vm::Array::New(il2cpp_defaults.byte_class, kPublicKeyByteLength);
                    memcpy(il2cpp::vm::Array::GetFirstElementAddress(keyTokenManaged), assemblyName->public_key_token, kPublicKeyByteLength);
                    break;
                }
            }

            vm::Field::SetValue(assemblyNameObject, field, keyTokenManaged);
        }

        field = vm::Class::GetFieldFromName(assemblyNameType, "versioncompat");
        if (field != NULL)
        {
            int32_t kSameProcess = 2;
            vm::Field::SetValue(assemblyNameObject, field, &kSameProcess);
        }

        field = vm::Class::GetFieldFromName(assemblyNameType, "version");
        if (field != NULL)
        {
            Il2CppClass* versionType = vm::Class::FromIl2CppType(field->type);
            Il2CppObject* version = vm::Object::New(versionType);

            FieldInfo* versionField = vm::Class::GetFieldFromName(versionType, "_Major");
            if (versionField != NULL)
            {
                int32_t major = assemblyName->major;
                vm::Field::SetValue(version, versionField, &major);
            }

            versionField = vm::Class::GetFieldFromName(versionType, "_Minor");
            if (versionField != NULL)
            {
                int32_t minor = assemblyName->minor;
                vm::Field::SetValue(version, versionField, &minor);
            }

            versionField = vm::Class::GetFieldFromName(versionType, "_Build");
            if (versionField != NULL)
            {
                int32_t build = assemblyName->build;
                vm::Field::SetValue(version, versionField, &build);
            }

            versionField = vm::Class::GetFieldFromName(versionType, "_Revision");
            if (versionField != NULL)
            {
                int32_t revision = assemblyName->revision;
                vm::Field::SetValue(version, versionField, &revision);
            }

            vm::Field::SetValue(assemblyNameObject, field, version);
        }

        field = vm::Class::GetFieldFromName(assemblyNameType, "processor_architecture");
        if (field != NULL)
        {
            int32_t kMSILArchitecture = 1;
            vm::Field::SetValue(assemblyNameObject, field, &kMSILArchitecture);
        }
    }

    Il2CppArray* Assembly::GetModulesInternal(Il2CppReflectionAssembly * thisPtr)
    {
        Il2CppArray* arr = vm::Array::New(il2cpp_defaults.module_class, 1);
        il2cpp_array_setref(arr, 0, vm::Reflection::GetModuleObject(vm::Assembly::GetImage(thisPtr->assembly)));
        return arr;
    }

    bool Assembly::LoadPermissions(mscorlib_System_Reflection_Assembly * a, intptr_t* minimum, int32_t* minLength, intptr_t* optional, int32_t* optLength, intptr_t* refused, int32_t* refLength)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Assembly::LoadPermissions);
        return false;
    }

    Il2CppReflectionAssembly* Assembly::GetCallingAssembly()
    {
        return vm::Reflection::GetAssemblyObject(vm::Image::GetCallingImage()->assembly);
    }

    Il2CppString* Assembly::get_code_base(Il2CppReflectionAssembly * assembly, bool escaped)
    {
        std::string executableDirectory = utils::PathUtils::DirectoryName(os::Path::GetExecutablePath());
        std::replace(executableDirectory.begin(), executableDirectory.end(), '\\', '/');
        return vm::String::New(utils::StringUtils::Printf("file://%s/%s.dll", executableDirectory.c_str(), assembly->assembly->aname.name).c_str());
    }

    Il2CppArray* Assembly::GetTypes(Il2CppReflectionAssembly* thisPtr, bool exportedOnly)
    {
        const Il2CppImage* image = thisPtr->assembly->image;
        return Module::InternalGetTypes(vm::Reflection::GetModuleObject(image));
    }

    Il2CppString* Assembly::InternalImageRuntimeVersion(Il2CppAssembly* self)
    {
        NOT_SUPPORTED_IL2CPP(Assembly::InternalImageRuntimeVersion, "This icall is not supported by il2cpp.");

        return 0;
    }

    Il2CppReflectionMethod* Assembly::get_EntryPoint(Il2CppReflectionAssembly* self)
    {
        const MethodInfo* method = vm::Image::GetEntryPoint(self->assembly->image);
        if (method == NULL)
            return NULL;

        return il2cpp::vm::Reflection::GetMethodObject(method, NULL);
    }

    bool Assembly::get_global_assembly_cache(Il2CppAssembly* self)
    {
        return false;
    }

    Il2CppObject* Assembly::GetFilesInternal(Il2CppAssembly* self, Il2CppString* name, bool getResourceModules)
    {
        // Some code paths in mscorlib (e.g. Encoding.GetEncoding) will expect this icall to return NULL. If it
        // instead throws a NotSupportedException, the mscorlib code path changes, and we see some IL2CPP-specific bugs.
        return NULL;
    }

    void Assembly::InternalGetAssemblyName(Il2CppString* assemblyFile, Il2CppAssemblyName* aname)
    {
        NOT_SUPPORTED_IL2CPP(Assembly::InternalGetAssemblyName, "This icall is not supported by il2cpp.");
    }

    void Assembly::InternalGetAssemblyName40(Il2CppString* assemblyFile, Il2CppMonoAssemblyName* aname, Il2CppString** codebase)
    {
        NOT_SUPPORTED_IL2CPP(Assembly::InternalGetAssemblyName, "This icall is not supported by il2cpp.");
    }

    Il2CppReflectionAssembly* Assembly::LoadFrom(Il2CppString* assemblyFile, bool refonly)
    {
        IL2CPP_ASSERT(!refonly && "This icall is not supported by il2cpp when refonly=true");

        // ==={{ huatuo 
        // Our implementation is going to behave a bit different.  We can't actually load any assembly.  If we didn't know about the assembly at conversion time,
        // then we won't be able to do anything.
        // On the other hand, if the name of the assembly matches the name of an assembly that we converted, then lets return the assembly that we know about.
        std::string utf8Path = utils::StringUtils::Utf16ToUtf8(utils::StringUtils::GetChars(assemblyFile));
        //std::string fileName = utils::PathUtils::BasenameNoExtension(utf8Path);

        //const Il2CppAssembly* foundAssembly = vm::MetadataCache::GetAssemblyByName(fileName.c_str());

        const Il2CppAssembly* foundAssembly = vm::Assembly::Load(utf8Path.c_str());

        if (!foundAssembly)
        {
            /*vm::Exception::Raise(vm::Exception::GetFileLoadException(utf8Path.c_str()));
            IL2CPP_UNREACHABLE;*/
            return nullptr;
        }
        // ===}} huatuo

        return vm::Reflection::GetAssemblyObject(foundAssembly);
    }

    Il2CppArray* Assembly::GetNamespaces(Il2CppAssembly* self)
    {
        NOT_SUPPORTED_IL2CPP(Assembly::GetNamespaces, "This icall is not supported by il2cpp.");

        return 0;
    }

    Il2CppArray* Assembly::GetReferencedAssemblies(Il2CppReflectionAssembly* self)
    {
        vm::AssemblyNameVector referencedAssemblies;
        vm::Assembly::GetReferencedAssemblies(self->assembly, &referencedAssemblies);
        Il2CppArray* result = vm::Array::New(il2cpp_defaults.assembly_name_class, (il2cpp_array_size_t)referencedAssemblies.size());
        size_t index = 0;
        for (vm::AssemblyNameVector::const_iterator aname = referencedAssemblies.begin(); aname != referencedAssemblies.end(); ++aname)
        {
            Il2CppReflectionAssemblyName* reflectionAssemblyName = vm::Reflection::GetAssemblyNameObject(*aname);
            il2cpp_array_setref(result, index, reflectionAssemblyName);
            index++;
        }

        return result;
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

    Il2CppArray* Assembly::GetManifestResourceNames(Il2CppReflectionAssembly* assembly)
    {
        std::vector<vm::EmbeddedResourceRecord> resourceRecords = GetResourceRecords(assembly);

        IL2CPP_ASSERT(resourceRecords.size() <= static_cast<size_t>(std::numeric_limits<il2cpp_array_size_t>::max()));
        Il2CppArray* resourceNameArray = vm::Array::New(il2cpp_defaults.string_class, static_cast<il2cpp_array_size_t>(resourceRecords.size()));
        for (size_t i = 0; i < resourceRecords.size(); ++i)
            il2cpp_array_setref(resourceNameArray, i, vm::String::New(resourceRecords[i].name.c_str()));

        return resourceNameArray;
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

    bool Assembly::GetManifestResourceInfoInternal(Il2CppReflectionAssembly* assembly, Il2CppString* name, Il2CppManifestResourceInfo* info)
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

    intptr_t Assembly::GetManifestResourceInternal(Il2CppReflectionAssembly* assembly, Il2CppString* name, int* size, Il2CppReflectionModule** module)
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

    int32_t Assembly::MonoDebugger_GetMethodToken(void* /* System.Reflection.MethodBase */ method)
    {
        NOT_SUPPORTED_IL2CPP(Assembly::MonoDebugger_GetMethodToken, "This icall is not supported by il2cpp.");

        return 0;
    }

    Il2CppReflectionModule* Assembly::GetManifestModuleInternal(Il2CppAssembly* self)
    {
        NOT_SUPPORTED_IL2CPP(Assembly::GetManifestModuleInternal, "This icall is not supported by il2cpp.");

        return 0;
    }

    bool Assembly::get_ReflectionOnly(Il2CppAssembly* self)
    {
        // It doesn't mean anything to have a reflection only assembly in il2cpp since we can't load a managed assembly that we didn't convert.  So let's always return false.
        return false;
    }

    Il2CppString* Assembly::GetAotId()
    {
        return NULL;
    }

    intptr_t Assembly::InternalGetReferencedAssemblies(Il2CppReflectionAssembly* module)
    {
        VoidPtrArray assemblyPointers;
        vm::AssemblyNameVector referencedAssemblies;
        vm::Assembly::GetReferencedAssemblies(module->assembly, &referencedAssemblies);
        for (vm::AssemblyNameVector::const_iterator aname = referencedAssemblies.begin(); aname != referencedAssemblies.end(); ++aname)
        {
            Il2CppMonoAssemblyName* monoAssemblyName = (Il2CppMonoAssemblyName*)IL2CPP_MALLOC_ZERO(sizeof(Il2CppMonoAssemblyName));
            il2cpp::vm::AssemblyName::FillNativeAssemblyName(*(*aname), monoAssemblyName);
            assemblyPointers.push_back(monoAssemblyName);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(assemblyPointers));
    }
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
