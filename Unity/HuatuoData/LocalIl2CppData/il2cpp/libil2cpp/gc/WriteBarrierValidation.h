#pragma once

#if IL2CPP_ENABLE_WRITE_BARRIER_VALIDATION

namespace il2cpp
{
namespace gc
{
    class WriteBarrierValidation
    {
    public:
        typedef void(*ExternalAllocationTrackerFunction)(void*, size_t, int);
        static void SetExternalAllocationTracker(ExternalAllocationTrackerFunction func);
        typedef void(*ExternalWriteBarrierTrackerFunction)(void**);
        static void SetExternalWriteBarrierTracker(ExternalWriteBarrierTrackerFunction func);

        static void Setup();
        static void Run();
    };
} /* gc */
} /* il2cpp */
#endif
