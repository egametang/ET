#pragma once

#include <stdint.h>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Globalization
{
// System.Globalization.CompareOptions
    typedef enum
    {
        CompareOptions_None              = 0x00,
        CompareOptions_IgnoreCase        = 0x01,
        CompareOptions_IgnoreNonSpace    = 0x02,
        CompareOptions_IgnoreSymbols     = 0x04,
        CompareOptions_IgnoreKanaType    = 0x08,
        CompareOptions_IgnoreWidth       = 0x10,
        CompareOptions_StringSort        = 0x20000000,
        CompareOptions_Ordinal           = 0x40000000,
        CompareOptions_OrdinalIgnoreCase = 0x10000000
    } CompareOptions;
} /* namespace Globalization */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
