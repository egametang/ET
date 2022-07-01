#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS && IL2CPP_HAS_OS_SYNCHRONIZATION_CONTEXT

#include "os/SynchronizationContext.h"
#include "os/WindowsRuntime.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/RCW.h"
#include "WindowsHelpers.h"

#include <windows.ui.core.h>
#include <wrl.h>

using il2cpp::os::SynchronizationContext;
using Microsoft::WRL::Callback;
using Microsoft::WRL::ComPtr;
using Microsoft::WRL::Wrappers::HStringReference;

template<typename T>
using AgileCallback = Microsoft::WRL::Implements<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::ClassicCom>, T, Microsoft::WRL::FtmBase>;

#if !IL2CPP_TARGET_XBOXONE

namespace winrt_interfaces
{
    // NOTE: DispatcherQueue interface was added in Windows SDK 16299
    // That means that when we build libil2cpp against an older SDK, these interfaces
    // will not be part of the headers. In order to solve that, I defined them here locally.
    enum DispatcherQueuePriority : int
    {
        DispatcherQueuePriority_Low = -10,
        DispatcherQueuePriority_Normal = 0,
        DispatcherQueuePriority_High = 10,
    };

    MIDL_INTERFACE("DFA2DC9C-1A2D-4917-98F2-939AF1D6E0C8")
    IDispatcherQueueHandler : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE Invoke(void) = 0;
    };

    MIDL_INTERFACE("603E88E4-A338-4FFE-A457-A5CFB9CEB899")
    IDispatcherQueue : public IInspectable
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE CreateTimer(struct IDispatcherQueueTimer** result) = 0;
        virtual HRESULT STDMETHODCALLTYPE TryEnqueue(IDispatcherQueueHandler* callback, boolean* result) = 0;
        virtual HRESULT STDMETHODCALLTYPE TryEnqueueWithPriority(DispatcherQueuePriority priority, IDispatcherQueueHandler* callback, boolean* result) = 0;
        virtual HRESULT STDMETHODCALLTYPE add_ShutdownStarting(ABI::Windows::Foundation::ITypedEventHandler<struct DispatcherQueue*, struct DispatcherQueueShutdownStartingEventArgs*>* handler, EventRegistrationToken* token) = 0;
        virtual HRESULT STDMETHODCALLTYPE remove_ShutdownStarting(EventRegistrationToken token) = 0;
        virtual HRESULT STDMETHODCALLTYPE add_ShutdownCompleted(ABI::Windows::Foundation::ITypedEventHandler<struct DispatcherQueue*, IInspectable*>* handler, EventRegistrationToken* token) = 0;
        virtual HRESULT STDMETHODCALLTYPE remove_ShutdownCompleted(EventRegistrationToken token) = 0;
    };

    MIDL_INTERFACE("A96D83D7-9371-4517-9245-D0824AC12C74")
    IDispatcherQueueStatics : public IInspectable
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetForCurrentThread(IDispatcherQueue** result) = 0;
    };
}

static ComPtr<winrt_interfaces::IDispatcherQueueStatics> GetDispatcherQueueStatics()
{
    Il2CppHString className;
    Il2CppHStringHeader classNameHeader;
    auto hr = il2cpp::os::WindowsRuntime::CreateHStringReference(L"Windows.System.DispatcherQueue", &classNameHeader, &className);
    if (FAILED(hr))
        return nullptr;

    ComPtr<IActivationFactory> activationFactory;
    hr = il2cpp::os::WindowsRuntime::GetActivationFactory(className, reinterpret_cast<Il2CppIActivationFactory**>(activationFactory.ReleaseAndGetAddressOf()));
    if (FAILED(hr))
        return nullptr;

    ComPtr<winrt_interfaces::IDispatcherQueueStatics> result;
    hr = activationFactory.As(&result);
    if (FAILED(hr))
        return nullptr;

    return result;
}

#endif

Il2CppObject* SynchronizationContext::GetForCurrentThread()
{
    HRESULT hr;

#if !IL2CPP_TARGET_WINDOWS_DESKTOP
    ComPtr<ABI::Windows::UI::Core::ICoreWindowStatic> coreWindowStatics;

    static_assert(LINK_TO_WINDOWSRUNTIME_LIBS, "RoGetActivationFactory and HStringReference can only be used directly if we link to WindowsRuntime libraries");
    hr = RoGetActivationFactory(HStringReference(L"Windows.UI.Core.CoreWindow").Get(), __uuidof(coreWindowStatics), &coreWindowStatics);
    if (SUCCEEDED(hr))
    {
        ComPtr<ABI::Windows::UI::Core::ICoreWindow> currentThreadWindow;
        hr = coreWindowStatics->GetForCurrentThread(&currentThreadWindow);
        if (SUCCEEDED(hr) && currentThreadWindow != nullptr)
        {
            ComPtr<ABI::Windows::UI::Core::ICoreDispatcher> dispatcher;
            hr = currentThreadWindow->get_Dispatcher(&dispatcher);
            if (SUCCEEDED(hr))
                return vm::RCW::GetOrCreateFromIInspectable(reinterpret_cast<Il2CppIInspectable*>(dispatcher.Get()), il2cpp_defaults.il2cpp_com_object_class);
        }
    }
#endif

#if !IL2CPP_TARGET_XBOXONE
    ComPtr<winrt_interfaces::IDispatcherQueueStatics> dispatcherQueueStatics = GetDispatcherQueueStatics();
    if (dispatcherQueueStatics != nullptr)
    {
        ComPtr<winrt_interfaces::IDispatcherQueue> dispatcherQueue;
        hr = dispatcherQueueStatics->GetForCurrentThread(&dispatcherQueue);
        if (SUCCEEDED(hr) && dispatcherQueue != nullptr)
            return vm::RCW::GetOrCreateFromIInspectable(reinterpret_cast<Il2CppIInspectable*>(dispatcherQueue.Get()), il2cpp_defaults.il2cpp_com_object_class);
    }
#endif

    return nullptr;
}

void SynchronizationContext::Post(Il2CppObject* context, SynchronizationContextCallback callback, intptr_t arg)
{
    IL2CPP_ASSERT(vm::Class::HasParent(context->klass, il2cpp_defaults.il2cpp_com_object_class));

    HRESULT hr;
    auto dispatcherUnknown = reinterpret_cast<IUnknown*>(static_cast<Il2CppComObject*>(context)->identity);

#if !IL2CPP_TARGET_WINDOWS_DESKTOP
    ComPtr<ABI::Windows::UI::Core::ICoreDispatcher> dispatcher;
    hr = dispatcherUnknown->QueryInterface(__uuidof(dispatcher), &dispatcher);
    if (SUCCEEDED(hr))
    {
        ComPtr<ABI::Windows::Foundation::IAsyncAction> ignoredAction;
        hr = dispatcher->RunAsync(ABI::Windows::UI::Core::CoreDispatcherPriority_Normal, Callback<AgileCallback<ABI::Windows::UI::Core::IDispatchedHandler> >([callback, arg]() -> HRESULT
        {
            callback(arg);
            return S_OK;
        }).Get(), &ignoredAction);
        vm::Exception::RaiseIfFailed(hr, false);
    }
#endif

#if !IL2CPP_TARGET_XBOXONE
    ComPtr<winrt_interfaces::IDispatcherQueue> dispatcherQueue;
    hr = dispatcherUnknown->QueryInterface(__uuidof(dispatcherQueue), &dispatcherQueue);
    if (SUCCEEDED(hr))
    {
        boolean ignoredResult;
        hr = dispatcherQueue->TryEnqueueWithPriority(winrt_interfaces::DispatcherQueuePriority_Normal, Callback<AgileCallback<winrt_interfaces::IDispatcherQueueHandler> >([callback, arg]() -> HRESULT
        {
            callback(arg);
            return S_OK;
        }).Get(), &ignoredResult);
        vm::Exception::RaiseIfFailed(hr, false);
    }
#endif
}

#endif // IL2CPP_TARGET_WINDOWS && IL2CPP_HAS_OS_SYNCHRONIZATION_CONTEXT
