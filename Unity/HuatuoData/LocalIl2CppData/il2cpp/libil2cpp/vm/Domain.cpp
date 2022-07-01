#include "il2cpp-config.h"
#include "vm/Class.h"
#include "vm/Domain.h"
#include "vm/Object.h"
#include "vm/Runtime.h"
#include "vm/Thread.h"
#include "gc/GarbageCollector.h"
#include "gc/WriteBarrier.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"

namespace il2cpp
{
namespace vm
{
    Il2CppDomain* Domain::S_domain = NULL;

    Il2CppDomain* Domain::GetCurrent()
    {
        if (S_domain)
            return S_domain;

        // allocate using gc memory since we hold onto object references
        S_domain = (Il2CppDomain*)il2cpp::gc::GarbageCollector::AllocateFixed(sizeof(Il2CppDomain), NULL);

        return S_domain;
    }

    Il2CppDomain* Domain::GetRoot()
    {
        // just one domain for now
        return GetCurrent();
    }

    void Domain::ContextInit(Il2CppDomain *domain)
    {
        Il2CppClass* klass = Class::FromName(il2cpp_defaults.corlib, "System.Runtime.Remoting.Contexts", "Context");
        Il2CppAppContext* context = (Il2CppAppContext*)Object::New(klass);

        // To match Mono's implementation we do not call the constructor here. If we do, context_id will be 1, which
        // is not correct.
        context->domain_id = domain->domain_id;
        context->context_id = 0;

        gc::WriteBarrier::GenericStore(&domain->default_context, context);
    }

    void Domain::ContextSet(Il2CppAppContext* context)
    {
        IL2CPP_OBJECT_SETREF(il2cpp::vm::Thread::Current()->GetInternalThread(), current_appcontext, (Il2CppObject*)context);
    }

    Il2CppAppContext* Domain::ContextGet()
    {
        return (Il2CppAppContext*)il2cpp::vm::Thread::Current()->GetInternalThread()->current_appcontext;
    }
} /* namespace vm */
} /* namespace il2cpp */
