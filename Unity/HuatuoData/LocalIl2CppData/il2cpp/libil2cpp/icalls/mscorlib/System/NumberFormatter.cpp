#include "il2cpp-config.h"
#include "icalls/mscorlib/System/NumberFormatter.h"
#include "il2cpp-number-formatter.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    void NumberFormatter::GetFormatterTables(uint64_t * * mantissas,
        int32_t * * exponents,
        int16_t * * digitLowerTable,
        int16_t * * digitUpperTable,
        int64_t * * tenPowersList,
        int32_t * * decHexDigits)
    {
        *mantissas = (uint64_t*)Formatter_MantissaBitsTable;
        *exponents = (int32_t*)Formatter_TensExponentTable;
        *digitLowerTable = (int16_t*)Formatter_DigitLowerTable;
        *digitUpperTable = (int16_t*)Formatter_DigitUpperTable;
        *tenPowersList = (int64_t*)Formatter_TenPowersList;
        *decHexDigits = (int32_t*)Formatter_DecHexDigits;
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
