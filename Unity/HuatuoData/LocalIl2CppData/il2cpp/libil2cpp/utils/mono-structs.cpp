#include "mono-structs.h"
#include "os/c-api/il2cpp-config-platforms.h"
#include "os/c-api/Allocator.h"

MonoGPtrArray* void_ptr_array_to_gptr_array(const VoidPtrArray& array)
{
    MonoGPtrArray *pRetVal;

    pRetVal = (MonoGPtrArray*)Allocator::Allocate(sizeof(MonoGPtrArray));

    pRetVal->len = (unsigned int)array.size();
    if (pRetVal->len > 0)
    {
        size_t numBytes = sizeof(void*) * pRetVal->len;
        pRetVal->pdata = Allocator::Allocate(numBytes);
        memcpy(pRetVal->pdata, array.data(), numBytes);
    }
    else
    {
        pRetVal->pdata = NULL;
    }

    return pRetVal;
}

MonoGPtrArray* empty_gptr_array()
{
    MonoGPtrArray *pRetVal = (MonoGPtrArray*)Allocator::Allocate(sizeof(MonoGPtrArray));
    pRetVal->len = 0;
    pRetVal->pdata = NULL;
    return pRetVal;
}

void free_gptr_array(MonoGPtrArray *pArray)
{
    if (!pArray)
        return;

    if (pArray->pdata)
    {
        IL2CPP_FREE(pArray->pdata);
        pArray->pdata = NULL;
    }

    IL2CPP_FREE(pArray);
}
