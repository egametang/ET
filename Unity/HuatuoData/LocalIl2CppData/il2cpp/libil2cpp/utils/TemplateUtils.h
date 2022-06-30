#pragma once

#include <stdint.h>

namespace il2cpp
{
namespace utils
{
namespace TemplateUtils
{
    template<typename T, typename U>
    struct IsSame
    {
        static const bool value = false;
    };

    template<typename T>
    struct IsSame<T, T>
    {
        static const bool value = true;
    };

    template<typename B, typename D>
    struct IsBaseOf
    {
    private:
        template<typename BT, typename DT>
        struct Converter
        {
            operator BT*() const;
            operator DT*();
        };

        typedef int16_t Derived;
        typedef int8_t NotDerived;

        template<typename T>
        static Derived IsDerived(D*, T);
        static NotDerived IsDerived(B*, int);

    public:
        static const bool value = IsSame<B, D>::value || sizeof(IsDerived(Converter<B, D>(), int())) == sizeof(Derived);
    };
}
}
}
