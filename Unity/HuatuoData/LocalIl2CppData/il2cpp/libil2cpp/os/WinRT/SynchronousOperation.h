#pragma once

#include "ReferenceCounter.h"

#include "os/Win32/WindowsHeaders.h"
#include <windows.foundation.h>
#include <windows.foundation.collections.h>
#include <wrl.h>

#include <vector>

namespace il2cpp
{
namespace winrt
{
    template<typename T>
    struct ResultTypeTraits
    {
        typedef typename ABI::Windows::Foundation::Internal::GetLogicalType<typename T::TResult_complex>::type LogicalType;
        typedef typename ABI::Windows::Foundation::Internal::GetAbiType<typename T::TResult_complex>::type ResultType;
    };

    template<>
    struct ResultTypeTraits<ABI::Windows::Foundation::IAsyncAction>
    {
        typedef void LogicalType;
        typedef void ResultType;
    };

    template<typename OperationType, typename HandlerType>
    class SynchronousOperationBase : public Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::WinRtClassicComMix>, Microsoft::WRL::FtmBase, HandlerType>
    {
    protected:
        HANDLE m_Event;
        HRESULT m_HR;

        typedef OperationType OperationType;

        inline SynchronousOperationBase(OperationType* op)
        {
            m_Event = CreateEventExW(nullptr, nullptr, CREATE_EVENT_MANUAL_RESET, EVENT_ALL_ACCESS);
            Assert(m_Event && "CreateEventExW failed.");
        }

        inline ~SynchronousOperationBase()
        {
            CloseHandle(m_Event);

            // This will be not 0 if it's allocated on the stack
            // This class must be allocated on the heap for correct COM ref counting!
            IL2CPP_ASSERT(GetRefCount() == 0);
        }

    public:
        inline HRESULT Wait()
        {
            auto waitResult = WaitForSingleObjectEx(m_Event, INFINITE, FALSE);

            if (waitResult != WAIT_OBJECT_0)
                return E_FAIL;

            return m_HR;
        }
    };

    template<typename T>
    class SynchronousOperation : public SynchronousOperationBase<ABI::Windows::Foundation::IAsyncOperation<T>, ABI::Windows::Foundation::IAsyncOperationCompletedHandler<T> >
    {
    private:
        typedef typename ResultTypeTraits<OperationType>::ResultType ResultType;
        ResultType m_Result;

    public:
        inline SynchronousOperation(OperationType* op) :
            SynchronousOperationBase(op),
            m_Result(ResultType())
        {
            // NOTE: this is in the derived class because it might immediately complete.
            // If this was called from the base class, you'd get a callback on a partially
            // constructed vtable and crash.
            auto hr = op->put_Completed(this);
            Assert(SUCCEEDED(hr) && "IAsyncOperation<T>::put_Completed failed.");
        }

        ~SynchronousOperation()
        {
            ReferenceCounter<ResultType>::Release(m_Result);
        }

        HRESULT GetResults(ResultType* result)
        {
            auto hr = Wait();
            if (FAILED(hr))
                return hr;

            *result = m_Result;
            ReferenceCounter<ResultType>::AddRef(*result);
            return S_OK;
        }

        virtual HRESULT STDMETHODCALLTYPE Invoke(ABI::Windows::Foundation::IAsyncOperation<T>* asyncInfo, ABI::Windows::Foundation::AsyncStatus status) override
        {
            m_HR = asyncInfo->GetResults(&m_Result);
            SetEvent(m_Event);
            return S_OK;
        }

        static Microsoft::WRL::ComPtr<SynchronousOperation<T> > Make(OperationType* op)
        {
            return Microsoft::WRL::Make<SynchronousOperation<T> >(op);
        }
    };

    template<>
    class SynchronousOperation<void> : public SynchronousOperationBase<ABI::Windows::Foundation::IAsyncAction, ABI::Windows::Foundation::IAsyncActionCompletedHandler>
    {
    public:
        inline SynchronousOperation(OperationType* op) :
            SynchronousOperationBase(op)
        {
            // NOTE: this is in the derived class because it might immediately complete.
            // If this was called from the base class, you'd get a callback on a partially
            // constructed vtable and crash.
            auto hr = op->put_Completed(this);
            Assert(SUCCEEDED(hr) && "IAsyncAction::put_Completed failed.");
        }

        virtual HRESULT STDMETHODCALLTYPE Invoke(ABI::Windows::Foundation::IAsyncAction* asyncInfo, ABI::Windows::Foundation::AsyncStatus status) override
        {
            m_HR = asyncInfo->GetResults();
            SetEvent(m_Event);
            return S_OK;
        }
    };

    template<typename T>
    Microsoft::WRL::ComPtr<SynchronousOperation<typename ResultTypeTraits<T>::LogicalType> > MakeSynchronousOperation(T* op)
    {
        return Microsoft::WRL::Make<SynchronousOperation<typename ResultTypeTraits<T>::LogicalType> >(op);
    }
}
}
