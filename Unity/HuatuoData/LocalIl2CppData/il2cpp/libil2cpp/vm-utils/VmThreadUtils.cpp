#include "VmThreadUtils.h"
#include "os/Thread.h"

namespace il2cpp
{
namespace utils
{
    Il2CppStackPointerResult VmThreadUtils::PointerIsOnCurrentThreadStack(void* ptr)
    {
        void* low;
        void* high;
        if (il2cpp::os::Thread::GetCurrentThreadStackBounds(&low, &high))
        {
            if ((uintptr_t)ptr >= (uintptr_t)low && (uintptr_t)ptr <= (uintptr_t)high)
                return Il2CppStackPointerIsOnStack;
            return Il2CppStackPointerIsNotOnStack;
        }

        return Il2CppStackPointerNotSupported;
    }
}
}
