#include "il2cpp-config.h"
#include <stddef.h>
#include <vector>
#include <algorithm>
#include "icalls/mscorlib/System.Reflection/Assembly.h"
#include "icalls/mscorlib/System.Reflection/RuntimeModule.h"
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
#define CHECK_IF_NULL(v)    \
    if ( (v) == NULL && throwOnError ) \
        vm::Exception::Raise (vm::Exception::GetTypeLoadException (info)); \
    if ( (v) == NULL ) \
        return NULL;

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

    Il2CppReflectionAssembly* Assembly::GetCallingAssembly()
    {
        return vm::Reflection::GetAssemblyObject(vm::Image::GetCallingImage()->assembly);
    }

    Il2CppObject* Assembly::GetEntryAssembly()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(Assembly::GetEntryAssembly, "In the case of Unity this is always NULL. For a normal exe this is the assembly with Main.");
        return NULL;
    }

    Il2CppReflectionAssembly* Assembly::GetExecutingAssembly()
    {
        return vm::Reflection::GetAssemblyObject(vm::Image::GetExecutingImage()->assembly);
    }

    Il2CppReflectionAssembly* Assembly::load_with_partial_name(Il2CppString* name, Il2CppObject* e)
    {
        const Il2CppAssembly* assembly = vm::Assembly::GetLoadedAssembly(il2cpp::utils::StringUtils::Utf16ToUtf8(name->chars).c_str());
        if (assembly != NULL)
            return vm::Reflection::GetAssemblyObject(assembly);

        return NULL;
    }

    Il2CppReflectionAssembly* Assembly::LoadFile_internal(Il2CppString* assemblyFile, int32_t* stackMark)
    {
        return LoadFrom(assemblyFile, false, stackMark);
    }

    Il2CppReflectionAssembly* Assembly::LoadFrom(Il2CppString* assemblyFile, bool refOnly, int32_t* stackMark)
    {
        IL2CPP_ASSERT(!refOnly && "This icall is not supported by il2cpp when refOnly=true");

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

#define CHECK_IF_NULL(v)    \
    if ( (v) == NULL && throwOnError ) \
        vm::Exception::Raise (vm::Exception::GetTypeLoadException (info)); \
    if ( (v) == NULL ) \
        return NULL;

    Il2CppReflectionType* Assembly::InternalGetType(Il2CppReflectionAssembly* assembly, Il2CppObject* module, Il2CppString* name, bool throwOnError, bool ignoreCase)
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

    Il2CppArray* Assembly::GetTypes(Il2CppReflectionAssembly* thisPtr, bool exportedOnly)
    {
        const Il2CppImage* image = thisPtr->assembly->image;
        return il2cpp::vm::Image::GetTypes(image, exportedOnly);
    }

    void Assembly::InternalGetAssemblyName(Il2CppString* assemblyFile, void* aname, Il2CppString** codebase)
    {
        NOT_SUPPORTED_IL2CPP(Assembly::InternalGetAssemblyName, "This icall is not supported by il2cpp.");
    }
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
