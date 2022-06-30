#pragma once
#include <utility>

#include <hstring.h>
#include <Unknwn.h>

namespace il2cpp
{
namespace winrt
{
    template<typename T, bool isIUnknown = std::is_base_of<IUnknown, std::remove_pointer<T>::type>::value>
    struct ReferenceCounter;

    template<typename T>
    struct ReferenceCounter<T, false>
    {
        static inline void AddRef(T& value)
        {
        }

        static inline void Release(T& value)
        {
        }
    };

    template<typename T>
    struct ReferenceCounter<T, true>
    {
        static inline void AddRef(T& value)
        {
            if (value == nullptr)
                return;

            value->AddRef();
        }

        static inline void Release(T& value)
        {
            if (value == nullptr)
                return;

            value->Release();
        }
    };

    template<>
    struct ReferenceCounter<HSTRING, false>
    {
        static inline void AddRef(HSTRING& value)
        {
            if (value == nullptr)
                return;

            auto hr = WindowsDuplicateString(value, &value);
            Assert(SUCCEEDED(hr));
        }

        static inline void Release(HSTRING& value)
        {
            if (value == nullptr)
                return;

            auto hr = WindowsDeleteString(value);
            Assert(SUCCEEDED(hr));
        }
    };
}
}
