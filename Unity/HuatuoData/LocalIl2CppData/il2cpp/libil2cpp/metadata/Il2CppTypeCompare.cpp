#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppTypeCompare.h"

namespace il2cpp
{
namespace metadata
{
    template<typename T>
    static inline int Compare(const T& left, const T& right)
    {
        if (left == right)
            return 0;

        if (left < right)
            return -1;

        return 1;
    }

    static int Compare(const Il2CppType* t1, const Il2CppType* t2)
    {
        int result = Compare(t1->type, t2->type);
        if (result != 0)
            return result;

        result = Compare(t1->byref, t2->byref);
        if (result != 0)
            return result;

        switch (t1->type)
        {
            case IL2CPP_TYPE_VALUETYPE:
            case IL2CPP_TYPE_CLASS:
                return Compare(t1->data.typeHandle, t2->data.typeHandle);

            case IL2CPP_TYPE_PTR:
            case IL2CPP_TYPE_SZARRAY:
                return Compare(t1->data.type, t2->data.type);

            case IL2CPP_TYPE_ARRAY:
            {
                result = Compare(t1->data.array->rank, t2->data.array->rank);
                if (result != 0)
                    return result;

                return Compare(t1->data.array->etype, t2->data.array->etype);
            }
            case IL2CPP_TYPE_GENERICINST:
            {
                const Il2CppGenericInst *i1 = t1->data.generic_class->context.class_inst;
                const Il2CppGenericInst *i2 = t2->data.generic_class->context.class_inst;

                // this happens when maximum generic recursion is hit
                if (i1 == NULL || i2 == NULL)
                {
                    if (i1 == i2)
                        return 0;
                    return (i1 == NULL) ? -1 : 1;
                }

                result = Compare(i1->type_argc, i2->type_argc);
                if (result != 0)
                    return result;

                result = Compare(t1->data.generic_class->type, t2->data.generic_class->type);
                if (result != 0)
                    return result;

                /* FIXME: we should probably just compare the instance pointers directly.  */
                for (uint32_t i = 0; i < i1->type_argc; ++i)
                {
                    result = Compare(i1->type_argv[i], i2->type_argv[i]);
                    if (result != 0)
                        return result;
                }

                return 0;
            }
            case IL2CPP_TYPE_VAR:
            case IL2CPP_TYPE_MVAR:
                return Compare(t1->data.genericParameterHandle, t2->data.genericParameterHandle);
            default:
                return 0;
        }

        IL2CPP_NOT_IMPLEMENTED(Il2CppTypeEqualityComparer::compare);
        return Compare(static_cast<const void*>(t1), static_cast<const void*>(t2));
    }

    bool Il2CppTypeEqualityComparer::AreEqual(const Il2CppType* t1, const Il2CppType* t2)
    {
        return Compare(t1, t2) == 0;
    }

    bool Il2CppTypeLess::operator()(const Il2CppType * t1, const Il2CppType * t2) const
    {
        return Compare(t1, t2) < 0;
    }
} /* namespace vm */
} /* namespace il2cpp */
