#pragma once

#include <il2cpp-object-internals.h>
#include "utils/StringView.h"

namespace il2cpp
{
namespace vm
{
    class Il2CppHStringReference
    {
    private:
        Il2CppHString m_String;
        Il2CppHStringHeader m_Header;

    public:
        Il2CppHStringReference(const utils::StringView<Il2CppNativeChar>& str);

        inline operator Il2CppHString() const
        {
            return m_String;
        }
    };
}
}
