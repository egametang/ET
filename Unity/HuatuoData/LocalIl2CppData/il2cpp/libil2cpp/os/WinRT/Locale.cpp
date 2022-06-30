#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE || IL2CPP_TARGET_WINDOWS_GAMES

#include "os/Locale.h"
#include "os/Win32/WindowsHeaders.h"

#if IL2CPP_TARGET_WINRT
#include "utils/StringUtils.h"
#include "WinRTVector.h"

#include <Windows.ApplicationModel.Resources.Core.h>
#include <wrl.h>
#endif

namespace il2cpp
{
namespace os
{
    std::string Locale::GetLocale()
    {
        WCHAR wideLocaleName[LOCALE_NAME_MAX_LENGTH];
        if (GetUserDefaultLocaleName(wideLocaleName, LOCALE_NAME_MAX_LENGTH) == 0)
            return std::string();

        int length = static_cast<int>(wcslen(wideLocaleName));
        std::string multiLocaleName;
        multiLocaleName.resize(2 * length);
        int narrowLength = WideCharToMultiByte(CP_ACP, 0, &wideLocaleName[0], length, &multiLocaleName[0], 2 * length, NULL, NULL);
        multiLocaleName.resize(narrowLength);
        return multiLocaleName;
    }

#if IL2CPP_TARGET_WINRT
    static CultureInfoChangedCallback s_OnCultureInfoChangedInAppX;
    static EventRegistrationToken s_OnGlobalResourceContextChangedToken;
    static Microsoft::WRL::ComPtr<ABI::Windows::ApplicationModel::Resources::Core::IResourceContext> s_AppResourceContext;
    static Microsoft::WRL::ComPtr<ABI::Windows::Foundation::Collections::IObservableMap<HSTRING, HSTRING> > s_AppResourceContextValues;

    static il2cpp_hresult_t DispatchLanguageUpdateToManagedCode()
    {
        using namespace ABI::Windows::Foundation::Collections;
        using namespace Microsoft::WRL;
        using namespace Microsoft::WRL::Wrappers;

        ComPtr<IVectorView<HSTRING> > languages;
        auto hr = s_AppResourceContext->get_Languages(&languages);
        if (FAILED(hr))
            return hr;

        uint32_t languageCount;
        hr = languages->get_Size(&languageCount);
        if (FAILED(hr))
            return hr;

        for (uint32_t i = 0; i < languageCount; i++)
        {
            HString language;
            hr = languages->GetAt(i, language.GetAddressOf());
            if (SUCCEEDED(hr))
            {
                uint32_t languageLength;
                auto languagePtr = language.GetRawBuffer(&languageLength);

                // HSTRINGs aren't null terminated so we need to copy
                // it into a null terminated buffer before passing it to
                // win32 APIs or to managed code
                if (languageLength >= LOCALE_NAME_MAX_LENGTH)
                    languageLength = LOCALE_NAME_MAX_LENGTH - 1;

                wchar_t languageBuffer[LOCALE_NAME_MAX_LENGTH];
                memcpy(languageBuffer, languagePtr, languageLength * sizeof(wchar_t));
                languageBuffer[languageLength] = 0;

                if (IsValidLocaleName(languageBuffer))
                {
                    s_OnCultureInfoChangedInAppX(languageBuffer);
                    return S_OK;
                }

                wchar_t resolvedLanguage[LOCALE_NAME_MAX_LENGTH];
                if (ResolveLocaleName(languageBuffer, resolvedLanguage, IL2CPP_ARRAY_SIZE(resolvedLanguage)) != 0)
                {
                    s_OnCultureInfoChangedInAppX(resolvedLanguage);
                    return S_OK;
                }
            }
        }

        s_OnCultureInfoChangedInAppX(nullptr);
        return S_OK;
    }

    il2cpp_hresult_t Locale::InitializeUserPreferredCultureInfoInAppX(CultureInfoChangedCallback onCultureInfoChangedInAppX)
    {
        using namespace ABI::Windows::ApplicationModel::Resources::Core;
        using namespace ABI::Windows::Foundation::Collections;
        using namespace Microsoft::WRL;
        using namespace Microsoft::WRL::Wrappers;

        s_OnCultureInfoChangedInAppX = onCultureInfoChangedInAppX;

        if (s_OnGlobalResourceContextChangedToken.value == 0)
        {
            ComPtr<IResourceContextStatics2> resourceContextStatics;
            auto hr = RoGetActivationFactory(HStringReference(RuntimeClass_Windows_ApplicationModel_Resources_Core_ResourceContext).Get(), __uuidof(resourceContextStatics), &resourceContextStatics);
            if (FAILED(hr))
                return hr;

            hr = resourceContextStatics->GetForViewIndependentUse(&s_AppResourceContext);
            if (FAILED(hr))
                return hr;

            hr = s_AppResourceContext->get_QualifierValues(&s_AppResourceContextValues);
            if (FAILED(hr))
                return hr;

            hr = s_AppResourceContextValues->add_MapChanged(Callback<MapChangedEventHandler<HSTRING, HSTRING> >([](IObservableMap<HSTRING, HSTRING>* sender, IMapChangedEventArgs<HSTRING>* e)
            {
                DispatchLanguageUpdateToManagedCode();
                return S_OK;
            }).Get(), &s_OnGlobalResourceContextChangedToken);
            if (FAILED(hr))
                return hr;

            hr = DispatchLanguageUpdateToManagedCode();
            if (FAILED(hr))
                return hr;
        }

        return IL2CPP_S_OK;
    }

    static bool AreLanguagesEqual(HSTRING language, const Il2CppChar* name, uint32_t length)
    {
        uint32_t languageLength;
        auto languagePtr = WindowsGetStringRawBuffer(language, &languageLength);
        return CompareStringOrdinal(name, length, languagePtr, languageLength, TRUE) == CSTR_EQUAL;
    }

    il2cpp_hresult_t Locale::SetUserPreferredCultureInfoInAppX(const Il2CppChar* name)
    {
        using namespace ABI::Windows::Foundation::Collections;
        using namespace Microsoft::WRL;
        using namespace Microsoft::WRL::Wrappers;

        ComPtr<IVectorView<HSTRING> > languages;
        auto hr = s_AppResourceContext->get_Languages(&languages);
        if (FAILED(hr))
            return hr;

        uint32_t languageCount;
        hr = languages->get_Size(&languageCount);
        if (FAILED(hr))
            return hr;

        auto nameLength = static_cast<uint32_t>(utils::StringUtils::StrLen(name));
        if (languageCount > 0)
        {
            HString firstLanguage;
            if (SUCCEEDED(languages->GetAt(0, firstLanguage.GetAddressOf())) && AreLanguagesEqual(firstLanguage.Get(), name, nameLength))
                return S_OK; // Nothing to do, the language is already set
        }

        auto newLanguages = Make<winrt::Vector<HSTRING> >();
        newLanguages->Reserve(languageCount + 1);
        newLanguages->Append(HStringReference(name).Get());

        if (languageCount > 0)
        {
            HString language;
            if (SUCCEEDED(languages->GetAt(0, language.GetAddressOf())))
            {
                // No need to check for duplicates: we already checked that the new language doesn't match the first language above
                newLanguages->Append(language.Get());
            }

            for (uint32_t i = 1; i < languageCount; i++)
            {
                if (SUCCEEDED(languages->GetAt(i, language.GetAddressOf())) && !AreLanguagesEqual(language.Get(), name, nameLength))
                    newLanguages->Append(language.Get());
            }
        }

        return s_AppResourceContext->put_Languages(newLanguages.Get());
    }

    void Locale::UnInitializeWinRT()
    {
        if (s_OnGlobalResourceContextChangedToken.value != 0)
        {
            s_AppResourceContextValues->remove_MapChanged(s_OnGlobalResourceContextChangedToken);
            s_OnGlobalResourceContextChangedToken.value = 0;
        }

        s_OnCultureInfoChangedInAppX = nullptr;
        s_AppResourceContext = nullptr;
        s_AppResourceContextValues = nullptr;
    }

#endif
} /* namespace os */
} /* namespace il2cpp */

#endif
