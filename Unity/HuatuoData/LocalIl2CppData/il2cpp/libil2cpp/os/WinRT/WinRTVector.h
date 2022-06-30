#pragma once
// This class was copied from Unity's source code and modified to not depend on Unity's MemoryManager

#include "ReferenceCounter.h"

#include <vector>

#include <Windows.Foundation.Collections.h>
#include <wrl.h>

namespace Internal
{
    template<typename T>
    inline void AddItemRef(T& value)
    {
        il2cpp::winrt::ReferenceCounter<T>::AddRef(value);
    }

    template<typename T>
    inline void ReleaseItem(T& value)
    {
        il2cpp::winrt::ReferenceCounter<T>::Release(value);
        value = T();
    }
}

namespace il2cpp
{
namespace winrt
{
    template<typename T>
    struct Vector :
        public Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::WinRtClassicComMix>,
                                            Microsoft::WRL::FtmBase,
                                            ABI::Windows::Foundation::Collections::IVector<T>,
                                            ABI::Windows::Foundation::Collections::IVectorView<T>,
                                            ABI::Windows::Foundation::Collections::IIterable<T> >
    {
    private:
        struct Iterator :
            public Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::WinRtClassicComMix>, Microsoft::WRL::FtmBase, ABI::Windows::Foundation::Collections::IIterator<T> >
        {
        private:
            Microsoft::WRL::ComPtr<Vector<T> > m_Vector;
            size_t m_Position;

        public:
            Iterator(Vector<T>* vector) :
                m_Vector(vector), m_Position(0)
            {
            }

            virtual HRESULT STDCALL get_Current(T* current) override
            {
                if (m_Position >= m_Vector->m_Vector.size())
                {
                    return E_BOUNDS;
                }

                *current = m_Vector->m_Vector[m_Position];
                ::Internal::AddItemRef(*current);
                return S_OK;
            }

            virtual HRESULT STDCALL get_HasCurrent(boolean* hasCurrent) override
            {
                *hasCurrent = m_Position < m_Vector->m_Vector.size();
                return S_OK;
            }

            virtual HRESULT STDCALL MoveNext(boolean* hasCurrent) override
            {
                if (m_Position < m_Vector->m_Vector.size())
                {
                    m_Position++;
                    *hasCurrent = m_Position < m_Vector->m_Vector.size();
                }
                else
                {
                    *hasCurrent = false;
                }

                return S_OK;
            }

            virtual HRESULT STDCALL GetMany(uint32_t capacity, T* dest, uint32_t* actualCount) override
            {
                return m_Vector->GetMany(0, capacity, dest, actualCount);
            }
        };

        friend struct Iterator;

        std::vector<T> m_Vector;

        inline void ClearInternal()
        {
            for (size_t i = 0; i < m_Vector.size(); i++)
                ::Internal::ReleaseItem(m_Vector[i]);

            m_Vector.clear();
        }

    public:
        Vector()
        {
        }

        virtual ~Vector()
        {
            ClearInternal();
        }

        virtual HRESULT STDCALL GetAt(uint32_t index, T* item) override
        {
            *item = m_Vector[index];
            ::Internal::AddItemRef(*item);
            return S_OK;
        }

        virtual HRESULT STDCALL GetMany(uint32_t startIndex, uint32_t capacity, T* dest, uint32_t* actualCount) override
        {
            ZeroMemory(dest, sizeof(T) * capacity);

            if (startIndex > m_Vector.size())
                return E_BOUNDS;

            uint32_t count = std::min(capacity, static_cast<uint32_t>(m_Vector.size()) - startIndex);

            for (uint32_t i = 0; i < count; i++)
            {
                dest[i] = m_Vector[i + startIndex];
                ::Internal::AddItemRef(dest[i]);
            }

            *actualCount = count;
            return S_OK;
        }

        virtual HRESULT STDCALL get_Size(uint32_t* size) override
        {
            *size = static_cast<unsigned>(m_Vector.size());
            return S_OK;
        }

        virtual HRESULT STDCALL GetView(ABI::Windows::Foundation::Collections::IVectorView<T>** view) override
        {
            AddRef();
            *view = this;
            return S_OK;
        }

        virtual HRESULT STDCALL IndexOf(T value, uint32_t* index, boolean* found) override
        {
            *found = false;

            for (*index = 0; *index < m_Vector.size(); (*index)++)
            {
                if (value == m_Vector[*index])
                {
                    *found = true;
                    break;
                }
            }

            return S_OK;
        }

        virtual HRESULT STDCALL SetAt(uint32_t index, T item) override
        {
            ::Internal::ReleaseItem(m_Vector[index]);
            m_Vector[index] = item;
            ::Internal::AddItemRef(m_Vector[index]);
            return S_OK;
        }

        virtual HRESULT STDCALL InsertAt(uint32_t index, T item) override
        {
            m_Vector.insert(m_Vector.begin() + index, item);
            ::Internal::AddItemRef(m_Vector[index]);
            return S_OK;
        }

        virtual HRESULT STDCALL RemoveAt(uint32_t index) override
        {
            if (m_Vector.size() <= index)
                return E_FAIL;

            ::Internal::ReleaseItem(m_Vector[index]);
            m_Vector.erase(m_Vector.begin() + index);
            return S_OK;
        }

        virtual HRESULT STDCALL Append(T item) override
        {
            m_Vector.push_back(item);
            ::Internal::AddItemRef(m_Vector.back());
            return S_OK;
        }

        virtual HRESULT STDCALL RemoveAtEnd() override
        {
            if (m_Vector.empty())
                return E_FAIL;

            ::Internal::ReleaseItem(m_Vector.back());
            m_Vector.pop_back();
            return S_OK;
        }

        virtual HRESULT STDCALL ReplaceAll(uint32_t count, T* values) override
        {
            ClearInternal();
            m_Vector.reserve(count);

            for (uint32_t i = 0; i < count; i++)
            {
                m_Vector.push_back(values[i]);
                ::Internal::AddItemRef(m_Vector.back());
            }

            return S_OK;
        }

        virtual HRESULT STDCALL Clear() override
        {
            ClearInternal();
            return S_OK;
        }

        virtual HRESULT STDCALL First(ABI::Windows::Foundation::Collections::IIterator<T>** first) override
        {
            *first = Microsoft::WRL::Make<Iterator>(this).Detach();
            return S_OK;
        }

        void Reserve(size_t count)
        {
            m_Vector.reserve(count);
        }
    };
}
}
