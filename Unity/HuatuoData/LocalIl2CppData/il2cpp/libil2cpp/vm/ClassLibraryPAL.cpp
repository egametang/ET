#include "vm/Class.h"
#include "vm/ClassLibraryPAL.h"
#include "os/ClassLibraryPAL/pal_mirror_structs.h"

#if IL2CPP_DEBUG && !RUNTIME_TINY && !RUNTIME_NONE
#define IL2CPP_ENABLE_PAL_MIRROR_CHECK 1
#else
#define IL2CPP_ENABLE_PAL_MIRROR_CHECK 0
#endif

namespace il2cpp
{
namespace vm
{
#if IL2CPP_ENABLE_PAL_MIRROR_CHECK

    static Il2CppClass* GetSysKlass()
    {
        static Il2CppClass* sysKlass = NULL;
        if (!sysKlass)
        {
            Il2CppClass *interop = Class::FromName(il2cpp_defaults.corlib, "", "Interop");
            if (interop == NULL)
                return NULL;

            void* iter = NULL;
            while (auto nestedType = Class::GetNestedTypes(interop, &iter))
            {
                if (strcmp(nestedType->name, "Sys") == 0)
                {
                    sysKlass = nestedType;
                    break;
                }
            }
        }

        return sysKlass;
    }

    template<typename MirrorStructType>
    static void CheckStructSizeForInteropSys(const char* nameInClassLibrary)
    {
        do
        {
            Il2CppClass* sysClass = GetSysKlass();
            if (sysClass == NULL)
                return;

            void* iter = NULL;
            while (Il2CppClass* nestedType = Class::GetNestedTypes(sysClass, &iter))
            {
                if (!strcmp(nestedType->name, nameInClassLibrary))
                {
                    IL2CPP_ASSERT(nestedType->native_size == sizeof(MirrorStructType));
                    return;
                }
            }
            IL2CPP_ASSERT(0 && "Item not found");
        }
        while (0);
    }

#endif

    void ClassLibraryPAL::Initialize()
    {
#if IL2CPP_ENABLE_PAL_MIRROR_CHECK
        CheckStructSizeForInteropSys<struct FileStatus>("FileStatus");
#endif
    }
} // namespace vm
} // namepsace il2cpp
