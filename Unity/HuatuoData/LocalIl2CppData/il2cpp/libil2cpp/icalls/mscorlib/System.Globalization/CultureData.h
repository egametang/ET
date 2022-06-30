#pragma once

struct NumberFormatEntryManaged;
struct Il2CppCultureData;

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
    class LIBIL2CPP_CODEGEN_API CultureData
    {
    public:
        static const void* fill_number_data(int32_t number_index, NumberFormatEntryManaged* managed);
        static void fill_culture_data(Il2CppCultureData* _this, int32_t datetimeIndex);
    };
} // namespace Globalization
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
