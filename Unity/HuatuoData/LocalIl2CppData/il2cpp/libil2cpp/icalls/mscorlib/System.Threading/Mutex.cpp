#include "il2cpp-config.h"
#include "icalls/mscorlib/System.Threading/Mutex.h"
#include "os/Mutex.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Threading
{
    intptr_t Mutex::CreateMutex_internal(bool initiallyOwned, Il2CppString* name, bool* created)
    {
        *created = true;
        il2cpp::os::Mutex* mutex = NULL;

        if (name == NULL)
        {
            mutex = new il2cpp::os::Mutex();
        }
        else
        {
            NOT_SUPPORTED_IL2CPP(Mutex::CreateMutex_internal, "Named mutexes are not supported");
        }

        if (initiallyOwned)
            mutex->Lock();

        return reinterpret_cast<intptr_t>(new il2cpp::os::MutexHandle(mutex));
    }

    bool Mutex::ReleaseMutex_internal(intptr_t handle)
    {
        il2cpp::os::MutexHandle* mutex = (il2cpp::os::MutexHandle*)handle;
        mutex->Get()->Unlock();
        return true;
    }

    intptr_t Mutex::OpenMutex_internal(Il2CppString* name, MutexRights rights, MonoIOError* error)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Mutex::OpenMutex_internal);

        return intptr_t();
    }
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
