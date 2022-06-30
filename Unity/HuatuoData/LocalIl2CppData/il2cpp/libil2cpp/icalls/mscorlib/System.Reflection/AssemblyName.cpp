#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "mono-structs.h"

#include "icalls/mscorlib/System.Reflection/AssemblyName.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Object.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Type.h"
#include "vm/Reflection.h"
#include "vm/AssemblyName.h"
#include "utils/StringUtils.h"
#include "vm-utils/VmStringUtils.h"

#include "utils/sha1.h"

using il2cpp::vm::Array;
using il2cpp::vm::Class;
using il2cpp::vm::Object;
using il2cpp::vm::Runtime;
using il2cpp::vm::String;

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
    void AssemblyName::get_public_token(uint8_t* token, uint8_t* pubkey, int32_t len)
    {
        uint8_t digest[20];

        IL2CPP_ASSERT(token != NULL);
        sha1_get_digest(pubkey, len, digest);
        for (int i = 0; i < 8; ++i)
            token[i] = digest[19 - i];
    }

    Il2CppMonoAssemblyName* AssemblyName::GetNativeName(intptr_t assembly_ptr)
    {
        Il2CppAssembly *assembly = (Il2CppAssembly*)assembly_ptr;

        Il2CppMonoAssemblyName *aname = (Il2CppMonoAssemblyName*)il2cpp::vm::Reflection::GetMonoAssemblyName(assembly);
        if (aname)
        {
            return aname;
        }
        else
        {
            aname = (Il2CppMonoAssemblyName*)IL2CPP_MALLOC_ZERO(sizeof(Il2CppMonoAssemblyName));
            il2cpp::vm::AssemblyName::FillNativeAssemblyName(assembly->aname, aname);

            il2cpp::vm::Reflection::SetMonoAssemblyName(assembly, aname);
            return aname;
        }
    }

    bool AssemblyName::ParseAssemblyName(intptr_t namePtr, Il2CppMonoAssemblyName* aname, bool* is_version_defined, bool* is_token_defined)
    {
        std::string name((char*)namePtr);

        il2cpp::vm::TypeNameParseInfo info;
        il2cpp::vm::TypeNameParser parser(name, info, false);

        if (!parser.ParseAssembly())
            return false;

        if (is_version_defined)
        {
            *is_version_defined = false;
            size_t index = name.find("Version");
            if (index != std::string::npos)
                *is_version_defined = true;
        }

        if (is_token_defined)
        {
            *is_token_defined = false;
            size_t index = name.find("PublicKeyToken");
            if (index != std::string::npos)
                *is_token_defined = true;
        }

        const il2cpp::vm::TypeNameParseInfo::AssemblyName& parsedName = info.assembly_name();

        aname->name = il2cpp::utils::StringUtils::StringDuplicate(parsedName.name.c_str());
        if (utils::VmStringUtils::CaseInsensitiveEquals(parsedName.culture.c_str(), "neutral")) // culture names are case insensitive
            aname->culture = 0;
        else
            aname->culture = il2cpp::utils::StringUtils::StringDuplicate(parsedName.culture.c_str());

        aname->public_key = reinterpret_cast<uint8_t*>(il2cpp::utils::StringUtils::StringDuplicate(parsedName.public_key.c_str()));

        for (int i = 0; i < il2cpp::vm::kPublicKeyTokenLength; ++i)
            aname->public_key_token.padding[i] = parsedName.public_key_token[i];

        aname->hash_alg = parsedName.hash_alg;
        aname->hash_len = parsedName.hash_len;
        aname->flags = parsedName.flags;
        aname->major = parsedName.major;
        aname->minor = parsedName.minor;
        aname->build = parsedName.build;
        aname->revision = parsedName.revision;

        return true;
    }
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
