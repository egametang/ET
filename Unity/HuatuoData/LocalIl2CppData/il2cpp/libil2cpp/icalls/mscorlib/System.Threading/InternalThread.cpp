#include "il2cpp-config.h"
#include "InternalThread.h"
#include "utils/Memory.h"
#include "os/Mutex.h"
#include "os/Thread.h"

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
    void InternalThread::Thread_free_internal(Il2CppInternalThread* _this)
    {
        delete _this->synch_cs;
        _this->synch_cs = NULL;

        IL2CPP_FREE(_this->name);

        delete reinterpret_cast<il2cpp::os::Thread*>(_this->handle);
    }
} // namespace Threading
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
