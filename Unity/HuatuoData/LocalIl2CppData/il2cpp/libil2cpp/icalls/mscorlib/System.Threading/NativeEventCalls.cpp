#include "il2cpp-config.h"
#include "icalls/mscorlib/System.Threading/NativeEventCalls.h"
#include "os/Event.h"
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
    bool NativeEventCalls::ResetEvent_internal(intptr_t handlePtr)
    {
        os::EventHandle* handle = (os::EventHandle*)handlePtr;
        os::ErrorCode result = handle->Get().Reset();

        return os::kErrorCodeSuccess == result;
    }

    bool NativeEventCalls::SetEvent_internal(intptr_t handlePtr)
    {
        os::EventHandle* handle = (os::EventHandle*)handlePtr;
        os::ErrorCode result = handle->Get().Set();

        return os::kErrorCodeSuccess == result;
    }

    intptr_t NativeEventCalls::CreateEvent_icall(bool manual, bool initial, Il2CppChar* name, int32_t name_length, int32_t* errorCode)
    {
        *errorCode = 0;
        il2cpp::os::Event* event = NULL;

        if (name == NULL)
            event = new os::Event(manual, initial);
        else
            NOT_SUPPORTED_IL2CPP(NativeEventCalls::CreateEvent_internal, "Named events are not supported.");

        return reinterpret_cast<intptr_t>(new os::EventHandle(event));
    }

    void NativeEventCalls::CloseEvent_internal(intptr_t handlePtr)
    {
        os::Handle* handle = (os::Handle*)handlePtr;
        // should we close or just delete
        //handle->Close ();
        delete handle;
    }
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
