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
        delete _this->longlived->synch_cs;
        _this->longlived->synch_cs = NULL;

        IL2CPP_FREE(_this->longlived);
        _this->longlived = NULL;

        IL2CPP_FREE(_this->name.chars);
        _this->name.chars = NULL;

        delete reinterpret_cast<il2cpp::os::Thread*>(_this->handle);
        _this->handle = NULL;
    }
} // namespace Threading
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
