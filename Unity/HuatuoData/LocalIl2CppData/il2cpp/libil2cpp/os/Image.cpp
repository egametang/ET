#include "il2cpp-config.h"

#include "Image.h"

namespace il2cpp
{
namespace os
{
namespace Image
{
    static void* s_ManagedSectionStart = NULL;
    static void* s_ManagedSectionEnd = NULL;

    bool ManagedSectionExists()
    {
        return s_ManagedSectionStart != NULL && s_ManagedSectionEnd != NULL;
    }

    bool IsInManagedSection(void* ip)
    {
        if (!ManagedSectionExists())
            return false;

        return s_ManagedSectionStart <= ip && ip <= s_ManagedSectionEnd;
    }

    void SetManagedSectionStartAndEnd(void* start, void* end)
    {
        s_ManagedSectionStart = start;
        s_ManagedSectionEnd = end;
    }
}
}
}
