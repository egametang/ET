#pragma once

#include <type_traits>

struct Il2CppObject;

namespace il2cpp
{
namespace gc
{
    class WriteBarrier
    {
    public:
        static void GenericStore(void** ptr, void* value);

        template<typename TPtr, typename TValue>
        static void GenericStore(TPtr** ptr, TValue* value)
        {
            static_assert((std::is_assignable<TPtr*&, TValue*>::value), "Pointers types are not assignment compatible");
            GenericStore((void**)ptr, (void*)value);
        }

        template<typename TPtr>
        static void GenericStoreNull(TPtr** ptr)
        {
            *ptr = NULL;
        }
    };
} /* gc */
} /* il2cpp */

#define IL2CPP_OBJECT_SETREF(obj, fieldname, value) do {\
        il2cpp::gc::WriteBarrier::GenericStore(&(obj)->fieldname, (value));\
    } while (0)

/* This should be used if 's' can reside on the heap */
#define IL2CPP_STRUCT_SETREF(s, fieldname, value) do {\
        il2cpp::gc::WriteBarrier::GenericStore(&(s)->fieldname, (value));\
    } while (0)

#define IL2CPP_OBJECT_SETREF_NULL(obj, fieldname) do {\
        il2cpp::gc::WriteBarrier::GenericStoreNull(&(obj)->fieldname);\
    } while (0)
