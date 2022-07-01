#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "icalls/mscorlib/System/Object.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    int Object::InternalGetHashCode(Il2CppObject* obj)
    {
        return il2cpp::vm::Object::GetHash(obj);
    }

    Il2CppReflectionType* Object::GetType(Il2CppObject* obj)
    {
        return il2cpp::vm::Reflection::GetTypeObject(&obj->klass->byval_arg);
    }

    Il2CppObject* Object::MemberwiseClone(Il2CppObject* obj)
    {
        return il2cpp::vm::Object::Clone(obj);
    }

    intptr_t Object::obj_address(Il2CppObject* obj)
    {
        return reinterpret_cast<intptr_t>(obj);
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
