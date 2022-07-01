#include "il2cpp-config.h"

#include "icalls/mscorlib/System/AppDomain.h"

#include "gc/Allocator.h"
#include "gc/GarbageCollector.h"
#include "gc/WriteBarrier.h"
#include "os/Mutex.h"
#include "utils/StringUtils.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

#include "vm/Array.h"
#include "vm/Assembly.h"
#include "vm/Class.h"
#include "vm/Domain.h"
#include "vm/Exception.h"
#include "vm/Image.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "vm/Thread.h"
#include "vm/Type.h"

#include "il2cpp-tabledefs.h"
#include "il2cpp-string-types.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-api.h"

#include <map>
#include <string>
#include <vector>

// ==={{ huatuo
#include "huatuo/metadata/Assembly.h"
// ===}} huatuo

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    Il2CppAppDomain* AppDomain::getCurDomain()
    {
        Il2CppDomain *add = vm::Domain::GetCurrent();

        if (add->domain)
            return add->domain;

        add->domain = (Il2CppAppDomain*)il2cpp::vm::Object::New(il2cpp_defaults.appdomain_class);
        return add->domain;
    }

    Il2CppAppDomain* AppDomain::getRootDomain()
    {
        return vm::Domain::GetRoot()->domain;
    }

    Il2CppAppDomain* AppDomain::createDomain(Il2CppString*, mscorlib_System_AppDomainSetup*)
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::createDomain, "IL2CPP only supports one app domain, others cannot be created.");
        return NULL;
    }

    Il2CppArray* AppDomain::GetAssemblies(Il2CppAppDomain* ad, bool refonly)
    {
        static Il2CppClass *System_Reflection_Assembly;

        if (!System_Reflection_Assembly)
        {
            System_Reflection_Assembly = il2cpp_defaults.mono_assembly_class;
        }

        vm::AssemblyVector* assemblies = vm::Assembly::GetAllAssemblies();

        int c = 0;
        Il2CppArray *res = vm::Array::New(System_Reflection_Assembly, (il2cpp_array_size_t)assemblies->size());
        for (vm::AssemblyVector::const_iterator assembly = assemblies->begin(); assembly != assemblies->end(); ++assembly)
            il2cpp_array_setref(res, c++, vm::Reflection::GetAssemblyObject(*assembly));

        return res;
    }

    Il2CppString *  AppDomain::getFriendlyName(Il2CppAppDomain* ad)
    {
        return il2cpp_string_new(ad->data->friendly_name);
    }

    Il2CppAppDomainSetup* AppDomain::getSetup(Il2CppAppDomain* domain)
    {
        IL2CPP_ASSERT(domain != NULL);
        IL2CPP_ASSERT(domain->data != NULL);

        return domain->data->setup;
    }

    Il2CppAppContext* AppDomain::InternalGetDefaultContext(void)
    {
        return vm::Domain::ContextGet();
    }

    Il2CppAppContext* AppDomain::InternalGetContext(void)
    {
        return vm::Domain::ContextGet();
    }

    Il2CppString* AppDomain::InternalGetProcessGuid(Il2CppString* newguid)
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::InternalGetProcessGuid, "This icall is only used in System.Runtime.Remoting.RemotingConfiguraiton.ProcessId.");

        return NULL;
    }

    void AppDomain::InternalPopDomainRef()
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::InternalPopDomainRef, "This icall is only used in the System.Runtime.Remoting namespace.");
    }

    void AppDomain::InternalPushDomainRef(mscorlib_System_AppDomain * domain)
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::InternalPushDomainRef, "This icall is only used in the System.Runtime.Remoting namespace.");
    }

    void AppDomain::InternalPushDomainRefByID(int32_t domain_id)
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::InternalPushDomainRefByID, "This icall is only used in the System.Runtime.Remoting namespace.");
    }

    mscorlib_System_Runtime_Remoting_Contexts_Context * AppDomain::InternalSetContext(mscorlib_System_Runtime_Remoting_Contexts_Context * context)
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::InternalSetContext, "This icall is only used in the System.Runtime.Remoting namespace.");

        return 0;
    }

    mscorlib_System_AppDomain * AppDomain::InternalSetDomain(mscorlib_System_AppDomain * context)
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::InternalSetDomain, "This icall is only used in the System.Runtime.Remoting namespace.");

        return 0;
    }

    mscorlib_System_AppDomain * AppDomain::InternalSetDomainByID(int32_t domain_id)
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::InternalSetDomainByID, "This icall is only used in the System.Runtime.Remoting namespace.");

        return 0;
    }

    Il2CppReflectionAssembly* AppDomain::LoadAssembly(Il2CppAppDomain* ad, Il2CppString* assemblyRef, struct mscorlib_System_Security_Policy_Evidence* evidence, bool refOnly)
    {
        il2cpp::vm::TypeNameParseInfo info;
        std::string name = il2cpp::utils::StringUtils::Utf16ToUtf8(assemblyRef->chars);
        il2cpp::vm::TypeNameParser parser(name, info, false);

        if (!parser.ParseAssembly())
            return NULL;

        //assemblyRef might have been a fullname like System, CultureInfo=bla, Version=4.0, PublicKeyToken=23423423423423
        //we ignore all that info except the name. (mono ignores the keytoken, and I'm quite sure it also ignores the version).
        //il2cpp does not pack multiple assemblies with the same name, and even if that one is not the exact one that is asked for,
        //it's more useful to return it than not to. (like cases where you want to Deserialize a BinaryFormatter blob that was serialized
        //on 4.0)
        // ==={{ huatuo
        const Il2CppAssembly* assembly = vm::Assembly::Load(info.assembly_name().name.c_str());
        // ===}} huatuo 
        if (assembly != NULL)
            return vm::Reflection::GetAssemblyObject(assembly);

        return NULL;
    }

    int32_t AppDomain::ExecuteAssembly(Il2CppAppDomain* self, Il2CppAssembly* a, Il2CppArray* args)
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::ExecuteAssembly, "This icall is not supported by il2cpp.");

        return 0;
    }

    // ==={{ huatuo
    Il2CppReflectionAssembly* AppDomain::LoadAssemblyRaw(Il2CppAppDomain* self, Il2CppArray* rawAssembly, Il2CppArray* rawSymbolStore, void* /* System.Security.Policy.Evidence */ securityEvidence, bool refonly)
    {
        //NOT_SUPPORTED_IL2CPP(AppDomain::LoadAssemblyRaw, "This icall is not supported by il2cpp.");
        // return 0;
        if (!rawAssembly)
        {
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentNullException("rawAssembly is null"));
        }
        const Il2CppAssembly* assembly = il2cpp::vm::MetadataCache::LoadAssemblyFromBytes(il2cpp::vm::Array::GetFirstElementAddress(rawAssembly),
            il2cpp::vm::Array::GetByteLength(rawAssembly));
        return vm::Reflection::GetAssemblyObject(assembly);
    }
    // ===}} huatuo

    bool AppDomain::InternalIsFinalizingForUnload(int32_t domain_id)
    {
        return false;
    }

    void AppDomain::InternalUnload(int32_t domain_id)
    {
        NOT_SUPPORTED_IL2CPP(AppDomain::InternalUnload, "This icall is not supported by il2cpp.");
    }

    baselib::ReentrantLock s_DomainDataMutex;
    typedef std::vector<std::pair<UTF16String, Il2CppObject*>, il2cpp::gc::Allocator<std::pair<UTF16String, Il2CppObject*> > > DomainDataStorage;
    DomainDataStorage* s_DomainData;

    static inline void InitializeDomainData()
    {
        void* memory = utils::Memory::Malloc(sizeof(DomainDataStorage));
        s_DomainData = new(memory) DomainDataStorage;
    }

    Il2CppObject* AppDomain::GetData(Il2CppAppDomain* self, Il2CppString* name)
    {
        os::FastAutoLock lock(&s_DomainDataMutex);

        if (s_DomainData == NULL)
            InitializeDomainData();

        for (DomainDataStorage::iterator it = s_DomainData->begin(); it != s_DomainData->end(); it++)
        {
            if (it->first.compare(name->chars) == 0)
                return it->second;
        }

        return NULL;
    }

    void AppDomain::SetData(Il2CppAppDomain* self, Il2CppString* name, Il2CppObject* data)
    {
        os::FastAutoLock lock(&s_DomainDataMutex);

        if (s_DomainData == NULL)
            InitializeDomainData();

        for (DomainDataStorage::iterator it = s_DomainData->begin(); it != s_DomainData->end(); it++)
        {
            if (it->first.compare(0, it->first.size(), name->chars, name->length) == 0)
            {
                gc::WriteBarrier::GenericStore(&it->second, data);
                return;
            }
        }

        s_DomainData->push_back(std::make_pair(UTF16String(name->chars, name->length), data));
        gc::GarbageCollector::SetWriteBarrier((void**)&s_DomainData->back().second);
    }

    void AppDomain::DoUnhandledException(Il2CppObject* _this, Il2CppException* e)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(AppDomain::DoUnhandledException);
        IL2CPP_UNREACHABLE;
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
