#include "icalls/System/System.Threading/Semaphore.h"
#include "os/Semaphore.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace System
{
namespace Threading
{
    intptr_t Semaphore::CreateSemaphore_internal40(int32_t initialCount, int32_t maximumCount, Il2CppString* name, int32_t* errorCode)
    {
        *errorCode = true;
        os::Semaphore* semaphore = NULL;

        if (name == NULL)
            semaphore = new os::Semaphore(initialCount, maximumCount);
        else
            NOT_SUPPORTED_IL2CPP(Semaphore::CreateSemaphore_internal, "Named semaphores are not supported.");

        return reinterpret_cast<intptr_t>(new os::SemaphoreHandle(semaphore));
    }

    bool Semaphore::ReleaseSemaphore_internal40(intptr_t handlePtr, int32_t releaseCount, int32_t* previousCount)
    {
        os::SemaphoreHandle* handle = (os::SemaphoreHandle*)handlePtr;

        return handle->Get().Post(releaseCount, previousCount);
    }

    intptr_t Semaphore::OpenSemaphore_internal(Il2CppString* name, int32_t rights, int32_t* error)
    {
        NOT_SUPPORTED_IL2CPP(Semaphore::OpenSemaphore_internal, "Named semaphores are not supported.");

        return 0;
    }
} /* namespace Threading */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
