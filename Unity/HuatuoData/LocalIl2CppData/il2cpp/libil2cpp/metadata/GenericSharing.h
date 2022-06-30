#pragma once

struct Il2CppGenericClass;
struct Il2CppGenericMethod;

namespace il2cpp
{
namespace metadata
{
    class GenericSharing
    {
    public:
        static bool IsShareable(Il2CppGenericClass* gclass);
        static bool IsShareable(Il2CppGenericMethod* gmethod);
    };
} /* namespace metadata */
} /* namespace il2cpp */
