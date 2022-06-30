#include "il2cpp-config.h"
#include "COMEntryPoints.h"

#include "il2cpp-windowsruntime-types.h"
#include "os/Mutex.h"
#include "os/WindowsRuntime.h"
#include "utils/StringUtils.h"
#include "vm/Exception.h"
#include "vm/MetadataCache.h"
#include "vm/Runtime.h"

#include <map>

struct HStringLess
{
    bool operator()(Il2CppHString left, Il2CppHString right) const
    {
        uint32_t lengthLeft = 0;
        uint32_t lengthRight = 0;

        auto charsLeft = il2cpp::os::WindowsRuntime::GetHStringBuffer(left, &lengthLeft);
        il2cpp::vm::Exception::RaiseIfError(charsLeft.GetError());
        auto charsRight = il2cpp::os::WindowsRuntime::GetHStringBuffer(right, &lengthRight);
        il2cpp::vm::Exception::RaiseIfError(charsRight.GetError());

        if (lengthLeft != lengthRight)
            return lengthLeft < lengthRight;

        return memcmp(charsLeft.Get(), charsRight.Get(), sizeof(Il2CppChar) * lengthLeft) < 0;
    }
};

struct ActivationFactoryWrapper
{
    ActivationFactoryWrapper(const std::pair<Il2CppIActivationFactory*, Il2CppHString>& factoryNamePair) :
        m_Factory(factoryNamePair.first),
        m_Name(factoryNamePair.second)
    {
        // NOTE: No add ref here since this constructor is only called with newly created factory
        // Also, name is already pre-duplicated since we cannot deal with hresult failure in a constructor
    }

    ActivationFactoryWrapper(const ActivationFactoryWrapper&); // = delete;
    ActivationFactoryWrapper& operator=(const ActivationFactoryWrapper&); // = delete;

    ~ActivationFactoryWrapper()
    {
        m_Factory->Release();

        il2cpp_hresult_t hr = il2cpp::os::WindowsRuntime::DeleteHString(m_Name);
        IL2CPP_ASSERT(IL2CPP_HR_SUCCEEDED(hr));
    }

    operator Il2CppIActivationFactory*() const
    {
        return m_Factory;
    }

private:
    Il2CppIActivationFactory* m_Factory;
    Il2CppHString m_Name;
};

typedef std::map<Il2CppHString, ActivationFactoryWrapper, HStringLess> FactoryCache;
static FactoryCache s_FactoryCache;
static baselib::ReentrantLock s_FactoryCacheMutex;
static bool s_InitializedIl2CppFromWindowsRuntime;

typedef Il2CppIActivationFactory* (*FactoryCreationFunction)();

// Returns:
//    IL2CPP_S_OK - on success
//    IL2CPP_E_INVALIDARG - if className or factory is null
//    IL2CPP_REGDB_E_CLASSNOTREG - if class was not found
extern "C" IL2CPP_EXPORT il2cpp_hresult_t STDCALL DllGetActivationFactory(Il2CppHString className, Il2CppIActivationFactory** factory)
{
    if (className == nullptr || factory == nullptr)
        return IL2CPP_E_INVALIDARG;

    il2cpp::os::FastAutoLock lock(&s_FactoryCacheMutex);

    if (!s_InitializedIl2CppFromWindowsRuntime)
    {
        if (!il2cpp::vm::Runtime::Init())
            return IL2CPP_COR_E_EXECUTIONENGINE;

        s_InitializedIl2CppFromWindowsRuntime = true;
    }

    FactoryCache::iterator it = s_FactoryCache.find(className);
    if (it != s_FactoryCache.end())
    {
        Il2CppIActivationFactory* cachedFactory = it->second;
        cachedFactory->AddRef();
        *factory = cachedFactory;
        return IL2CPP_S_OK;
    }

    uint32_t classNameLength;
    auto classNameUtf16 = il2cpp::os::WindowsRuntime::GetHStringBuffer(className, &classNameLength);
    il2cpp::vm::Exception::RaiseIfError(classNameUtf16.GetError());
    std::string classNameUtf8 = il2cpp::utils::StringUtils::Utf16ToUtf8(classNameUtf16.Get(), classNameLength);
    FactoryCreationFunction factoryCreationFunction = reinterpret_cast<FactoryCreationFunction>(il2cpp::vm::MetadataCache::GetWindowsRuntimeFactoryCreationFunction(classNameUtf8.c_str()));

    if (factoryCreationFunction == NULL)
        return IL2CPP_REGDB_E_CLASSNOTREG;

    Il2CppHString duplicatedClassName;
    il2cpp_hresult_t hr = il2cpp::os::WindowsRuntime::DuplicateHString(className, &duplicatedClassName);
    if (IL2CPP_HR_FAILED(hr))
        return hr;

    std::pair<FactoryCache::iterator, bool> insertionResult = s_FactoryCache.insert(std::make_pair(duplicatedClassName, std::make_pair(factoryCreationFunction(), duplicatedClassName)));
    IL2CPP_ASSERT(insertionResult.second && "Factory was already in the hash map!");

    Il2CppIActivationFactory* createdFactory = insertionResult.first->second;
    createdFactory->AddRef();
    *factory = createdFactory;
    return IL2CPP_S_OK;
}

extern "C" IL2CPP_EXPORT long STDCALL DllCanUnloadNow()
{
    if (!s_InitializedIl2CppFromWindowsRuntime)
        return IL2CPP_S_OK;

    // TO DO: we need to track all instantiated COM objects in order to implement this correctly
    return IL2CPP_S_FALSE;
}

void il2cpp::vm::COMEntryPoints::FreeCachedData()
{
    s_FactoryCache.clear();
}

// Prevent function name mangling on Windows/x86
// The reason this needs to live here and not os layer is because if this file is not compiled,
// those linker directives will cause unresolved external symbol errors
#if IL2CPP_TARGET_WINDOWS && defined(_M_IX86)
#pragma comment(linker, "/EXPORT:DllGetActivationFactory=_DllGetActivationFactory@8")
#pragma comment(linker, "/EXPORT:DllCanUnloadNow=_DllCanUnloadNow@0")
#endif
