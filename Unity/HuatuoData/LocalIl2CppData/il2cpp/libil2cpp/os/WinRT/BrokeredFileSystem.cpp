#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINRT

#include "BrokeredFileSystem.h"
#include "os/Atomic.h"
#include "os/Win32/WindowsHelpers.h"
#include "SynchronousOperation.h"
#include "utils/PathUtils.h"
#include "utils/StringUtils.h"

#include <windows.storage.h>
#include <windows.storage.search.h>

using il2cpp::winrt::MakeSynchronousOperation;
using Microsoft::WRL::ComPtr;
using Microsoft::WRL::Wrappers::HStringReference;

namespace winrt_interfaces
{
    enum HANDLE_CREATION_OPTIONS
    {
        HCO_CREATE_NEW = 0x1,
        HCO_CREATE_ALWAYS = 0x2,
        HCO_OPEN_EXISTING = 0x3,
        HCO_OPEN_ALWAYS = 0x4,
        HCO_TRUNCATE_EXISTING = 0x5
    };

    enum HANDLE_ACCESS_OPTIONS
    {
        HAO_NONE = 0,
        HAO_READ_ATTRIBUTES = 0x80,
        HAO_READ = 0x120089,
        HAO_WRITE = 0x120116,
        HAO_DELETE = 0x10000
    };

    enum HANDLE_SHARING_OPTIONS
    {
        HSO_SHARE_NONE = 0,
        HSO_SHARE_READ = 0x1,
        HSO_SHARE_WRITE = 0x2,
        HSO_SHARE_DELETE = 0x4
    };

    enum HANDLE_OPTIONS
    {
        HO_NONE = 0,
        HO_OPEN_REQUIRING_OPLOCK = 0x40000,
        HO_DELETE_ON_CLOSE = 0x4000000,
        HO_SEQUENTIAL_SCAN = 0x8000000,
        HO_RANDOM_ACCESS = 0x10000000,
        HO_NO_BUFFERING = 0x20000000,
        HO_OVERLAPPED = 0x40000000,
        HO_WRITE_THROUGH = 0x80000000,

        HO_ALL_POSSIBLE_OPTIONS = HO_OPEN_REQUIRING_OPLOCK | HO_DELETE_ON_CLOSE | HO_SEQUENTIAL_SCAN | HO_RANDOM_ACCESS | HO_NO_BUFFERING | HO_OVERLAPPED | HO_WRITE_THROUGH,
    };

    MIDL_INTERFACE("DF19938F-5462-48A0-BE65-D2A3271A08D6")
    IStorageFolderHandleAccess : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE Create(
            LPCWSTR fileName,
            HANDLE_CREATION_OPTIONS creationOptions,
            HANDLE_ACCESS_OPTIONS accessOptions,
            HANDLE_SHARING_OPTIONS sharingOptions,
            HANDLE_OPTIONS options,
            struct IOplockBreakingHandler* oplockBreakingHandler,
            HANDLE* interopHandle) = 0;
    };

    MIDL_INTERFACE("5CA296B2-2C25-4D22-B785-B885C8201E6A")
    IStorageItemHandleAccess : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE Create(
            HANDLE_ACCESS_OPTIONS accessOptions,
            HANDLE_SHARING_OPTIONS sharingOptions,
            HANDLE_OPTIONS options,
            struct IOplockBreakingHandler* oplockBreakingHandler,
            HANDLE* interopHandle) = 0;
    };
}

namespace il2cpp
{
namespace os
{
    template<typename T, const wchar_t* className>
    struct StaticsStorage
    {
        ~StaticsStorage()
        {
            Assert(!s_Initialized && "StaticsStorage was not properly disposed before destruction!");
            Assert(s_Statics == nullptr && "StaticsStorage was not properly disposed before destruction!");
        }

        T* Get()
        {
            if (s_Initialized)
                return s_Statics;

            T* statics;
            auto hr = RoGetActivationFactory(HStringReference(className).Get(), __uuidof(T), reinterpret_cast<void**>(&statics));
            if (FAILED(hr))
            {
                s_Initialized = true;
                return nullptr;
            }

            // The reason this is atomic isn't to prevent multiple RoGetActivationFactory invocations,
            // it's there to make sure we don't mess up reference counting
            if (Atomic::CompareExchangePointer<T>(&s_Statics, statics, nullptr) != nullptr)
            {
                statics->Release();
                return s_Statics;
            }

            s_Initialized = true;
            return statics;
        }

        void Release()
        {
            s_Initialized = false;

            if (s_Statics != nullptr)
            {
                s_Statics->Release();
                s_Statics = nullptr;
            }
        }

    private:
        // Note: It is not a smart pointer for atomicity
        T* s_Statics;
        volatile bool s_Initialized;
    };

    static StaticsStorage<ABI::Windows::Storage::IStorageFileStatics, RuntimeClass_Windows_Storage_StorageFile> s_StorageFileStatics;
    static StaticsStorage<ABI::Windows::Storage::IStorageFolderStatics, RuntimeClass_Windows_Storage_StorageFolder> s_StorageFolderStatics;

    static int HResultToWin32OrAccessDenied(HRESULT hr)
    {
        if (SUCCEEDED(hr))
            return ERROR_SUCCESS;

        if ((hr & 0xFFFF0000) == MAKE_HRESULT(SEVERITY_ERROR, FACILITY_WIN32, 0))
            return HRESULT_CODE(hr);

        return ERROR_ACCESS_DENIED;
    }

#define StoreErrorAndReturnIfFailed(hr, valueToReturn) do { if (FAILED(hr)) { *error = HResultToWin32OrAccessDenied(hr); return valueToReturn; } } while (false)
#define StoreErrorAndReturnFalseIfFailed(hr) StoreErrorAndReturnIfFailed(hr, false)

    static inline bool IsPathRooted(const UTF16String& path)
    {
        if (path.empty())
            return false;

        if (path[0] == '\\')
            return true;

        return path.length() > 1 && path[1] == ':';
    }

    static inline void FixSlashes(UTF16String& path)
    {
        for (auto& c : path)
        {
            if (c == '/')
                c = '\\';
        }
    }

    static UTF16String GetFullPath(UTF16String path)
    {
        FixSlashes(path);
        if (IsPathRooted(path))
            return path;

        UTF16String fullPath;
        DWORD fullPathLength = GetFullPathNameW(path.c_str(), 0, nullptr, nullptr);
        Assert(fullPathLength != 0 && "GetFullPathNameW failed!");

        do
        {
            fullPath.resize(fullPathLength);
            fullPathLength = GetFullPathNameW(path.c_str(), fullPathLength, &fullPath[0], nullptr);
            Assert(fullPathLength != 0 && "GetFullPathNameW failed!");
        }
        while (fullPathLength > fullPath.size());

        fullPath.resize(fullPathLength);
        return fullPath;
    }

    static bool SplitPathToFolderAndFileName(UTF16String path, UTF16String& outFolder, UTF16String& outFile)
    {
        FixSlashes(path);

        wchar_t* filePart = nullptr;

        DWORD fullPathLength = GetFullPathNameW(path.c_str(), 0, nullptr, nullptr);
        Assert(fullPathLength != 0 && "GetFullPathNameW failed!");

        do
        {
            outFolder.resize(fullPathLength);
            fullPathLength = GetFullPathNameW(path.c_str(), fullPathLength, &outFolder[0], &filePart);
            Assert(fullPathLength != 0 && "GetFullPathNameW failed!");
        }
        while (fullPathLength > outFolder.size());

        if (filePart != nullptr)
        {
            outFile = filePart;
            outFolder.resize(filePart - &outFolder[0] - 1);
            return true;
        }
        else
        {
            outFolder.resize(fullPathLength);
            outFile.clear();
            return false;
        }
    }

    static HRESULT GetStorageFolderAsync(const UTF16String& path, ABI::Windows::Foundation::IAsyncOperation<ABI::Windows::Storage::StorageFolder*>** operation)
    {
        Assert(IsPathRooted(path) && "GetStorageFolder expects an absolute path.");

        auto storageFolderStatics = s_StorageFolderStatics.Get();
        Assert(storageFolderStatics != nullptr && "Failed to get StorageFolder statics");

        return storageFolderStatics->GetFolderFromPathAsync(HStringReference(path.c_str(), static_cast<uint32_t>(path.length())).Get(), operation);
    }

    static HRESULT GetStorageFolder(const UTF16String& path, ABI::Windows::Storage::IStorageFolder** storageFolder)
    {
        ComPtr<ABI::Windows::Foundation::IAsyncOperation<ABI::Windows::Storage::StorageFolder*> > operation;
        auto hr = GetStorageFolderAsync(path, &operation);
        if (FAILED(hr))
            return hr;

        return MakeSynchronousOperation(operation.Get())->GetResults(storageFolder);
    }

    static HRESULT GetStorageFileAsync(const UTF16String& path, ABI::Windows::Foundation::IAsyncOperation<ABI::Windows::Storage::StorageFile*>** operation)
    {
        Assert(IsPathRooted(path) && "GetStorageFile expects an absolute path.");

        auto storageFileStatics = s_StorageFileStatics.Get();
        Assert(storageFileStatics != nullptr && "Failed to get StorageFile statics");

        return storageFileStatics->GetFileFromPathAsync(HStringReference(path.c_str(), static_cast<uint32_t>(path.length())).Get(), operation);
    }

    static HRESULT GetStorageFile(const UTF16String& path, ABI::Windows::Storage::IStorageFile** storageFile)
    {
        ComPtr<ABI::Windows::Foundation::IAsyncOperation<ABI::Windows::Storage::StorageFile*> > operation;
        auto hr = GetStorageFileAsync(path, &operation);
        if (FAILED(hr))
            return hr;

        return MakeSynchronousOperation(operation.Get())->GetResults(storageFile);
    }

    static HRESULT AsStorageItem(IInspectable* itf, ABI::Windows::Storage::IStorageItem** storageItem)
    {
        return itf->QueryInterface(__uuidof(*storageItem), reinterpret_cast<void**>(storageItem));
    }

    static HRESULT GetStorageItem(const UTF16String& path, ABI::Windows::Storage::IStorageItem** storageItem)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Storage;

        auto fullPath = GetFullPath(path);

        // We don't know whether it's a folder or a file. Try getting a file first
        ComPtr<IStorageFile> storageFile;
        auto hr = GetStorageFile(fullPath, &storageFile);
        if (SUCCEEDED(hr))
            return AsStorageItem(storageFile.Get(), storageItem);

        // Perhaps it's not a file but a folder?
        ComPtr<IStorageFolder> storageFolder;
        hr = GetStorageFolder(fullPath, &storageFolder);
        if (SUCCEEDED(hr))
            return AsStorageItem(storageFolder.Get(), storageItem);

        return hr;
    }

    int BrokeredFileSystem::CreateDirectoryW(const UTF16String& path)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Storage;

        ComPtr<IAsyncOperation<StorageFolder*> > creationOperation;

        {
            UTF16String parentFolderName, name;
            if (!SplitPathToFolderAndFileName(path, parentFolderName, name))
                return ERROR_ACCESS_DENIED;

            ComPtr<IStorageFolder> parentFolder;
            auto hr = GetStorageFolder(parentFolderName, &parentFolder);
            if (FAILED(hr))
                return HResultToWin32OrAccessDenied(hr);

            hr = parentFolder->CreateFolderAsync(HStringReference(name.c_str(), static_cast<uint32_t>(name.length())).Get(), CreationCollisionOption_FailIfExists, &creationOperation);
            if (FAILED(hr))
                return HResultToWin32OrAccessDenied(hr);
        }

        auto hr = MakeSynchronousOperation(creationOperation.Get())->Wait();
        if (FAILED(hr))
            return HResultToWin32OrAccessDenied(hr);

        return kErrorCodeSuccess;
    }

    static int DeleteStorageItem(ABI::Windows::Storage::IStorageItem* storageItem)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Storage;

        ComPtr<IAsyncAction> deletionAction;
        auto hr = storageItem->DeleteAsync(StorageDeleteOption_PermanentDelete, &deletionAction);
        if (FAILED(hr))
            return HResultToWin32OrAccessDenied(hr);

        hr = MakeSynchronousOperation(deletionAction.Get())->Wait();
        if (FAILED(hr))
            return HResultToWin32OrAccessDenied(hr);

        return kErrorCodeSuccess;
    }

    int BrokeredFileSystem::RemoveDirectoryW(const UTF16String& path)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Storage;

        auto fullPath = GetFullPath(path);

        ComPtr<IStorageFolder> storageFolder;
        auto hr = GetStorageFolder(fullPath, &storageFolder);
        if (FAILED(hr))
            return HResultToWin32OrAccessDenied(hr);

        ComPtr<IStorageItem> storageItem;
        hr = storageFolder.As(&storageItem);
        if (FAILED(hr))
            return HResultToWin32OrAccessDenied(hr);

        return DeleteStorageItem(storageItem.Get());
    }

    static UnityPalFileAttributes TranslateWinRTAttributesToPALAttributes(ABI::Windows::Storage::FileAttributes winrtAttributes)
    {
        // Normal file attribute enum value is different.
        // The rest are the same.
        if (winrtAttributes == ABI::Windows::Storage::FileAttributes_Normal)
            return kFileAttributeNormal;

        return static_cast<UnityPalFileAttributes>(winrtAttributes);
    }

    static ABI::Windows::Storage::FileAttributes TranslatePALAttributesToWinRTAttributes(UnityPalFileAttributes attributes)
    {
        return static_cast<ABI::Windows::Storage::FileAttributes>(attributes & ~kFileAttributeNormal);
    }

    static HRESULT FindFileSystemEntries(const UTF16String& path, UTF16String pathWithPattern, int* error, ABI::Windows::Foundation::Collections::IVectorView<ABI::Windows::Storage::IStorageItem*>** foundItems)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Foundation::Collections;
        using namespace ABI::Windows::Storage;
        using namespace ABI::Windows::Storage::Search;

        ComPtr<IAsyncOperation<StorageFolder*> > getFolderOperation;
        auto hr = GetStorageFolderAsync(GetFullPath(path), &getFolderOperation);
        StoreErrorAndReturnIfFailed(hr, hr);

        ComPtr<IInspectable> queryOptionsInspectable;
        hr = RoActivateInstance(HStringReference(RuntimeClass_Windows_Storage_Search_QueryOptions).Get(), &queryOptionsInspectable);
        StoreErrorAndReturnIfFailed(hr, hr);

        ComPtr<IQueryOptions> queryOptions;
        hr = queryOptionsInspectable.As(&queryOptions);
        IL2CPP_ASSERT(SUCCEEDED(hr) && "Failed to cast QueryOptions to IQueryOptions");

        hr = queryOptions->put_FolderDepth(FolderDepth_Shallow); // We're doing a non-recursive search
        StoreErrorAndReturnIfFailed(hr, hr);

        auto aqs = L"System.ItemPathDisplay:~\"" + GetFullPath(std::move(pathWithPattern)) + L"\"";
        hr = queryOptions->put_ApplicationSearchFilter(HStringReference(aqs.c_str(), static_cast<uint32_t>(aqs.length())).Get());
        StoreErrorAndReturnIfFailed(hr, hr);

        ComPtr<IStorageFolder> folderToSearch;
        hr = MakeSynchronousOperation(getFolderOperation.Get())->GetResults(&folderToSearch);
        StoreErrorAndReturnIfFailed(hr, hr);

        ComPtr<IStorageFolderQueryOperations> folderQueryOperations;
        hr = folderToSearch.As(&folderQueryOperations);
        IL2CPP_ASSERT(SUCCEEDED(hr) && "Failed to cast StorageFolder to IStorageFolderQueryOperations!");

        ComPtr<IStorageItemQueryResult> queryResult;
        hr = folderQueryOperations->CreateItemQueryWithOptions(queryOptions.Get(), &queryResult);
        StoreErrorAndReturnIfFailed(hr, hr);

        ComPtr<IAsyncOperation<IVectorView<IStorageItem*>*> > itemsOperation;
        hr = queryResult->GetItemsAsyncDefaultStartAndCount(&itemsOperation);
        StoreErrorAndReturnIfFailed(hr, hr);

        hr = MakeSynchronousOperation(itemsOperation.Get())->GetResults(foundItems);
        StoreErrorAndReturnIfFailed(hr, hr);

        return hr;
    }

    std::set<std::string> BrokeredFileSystem::GetFileSystemEntries(const UTF16String& path, const UTF16String& pathWithPattern, int32_t attributes, int32_t attributeMask, int* error)
    {
        using namespace ABI::Windows::Foundation::Collections;
        using namespace ABI::Windows::Storage;

        std::set<std::string> fileSystemEntries;

        ComPtr<IVectorView<IStorageItem*> > foundItems;
        auto hr = FindFileSystemEntries(path, pathWithPattern, error, &foundItems);
        StoreErrorAndReturnIfFailed(hr, fileSystemEntries);

        uint32_t foundCount;
        hr = foundItems->get_Size(&foundCount);
        StoreErrorAndReturnIfFailed(hr, fileSystemEntries);

        for (uint32_t i = 0; i < foundCount; i++)
        {
            ComPtr<IStorageItem> item;
            hr = foundItems->GetAt(i, &item);
            if (FAILED(hr)) continue;

            FileAttributes winrtAttributes;
            hr = item->get_Attributes(&winrtAttributes);
            if (FAILED(hr)) continue;

            auto palAttributes = TranslateWinRTAttributesToPALAttributes(winrtAttributes);
            if ((palAttributes & attributeMask) == attributes)
            {
                Microsoft::WRL::Wrappers::HString path;
                hr = item->get_Path(path.GetAddressOf());
                if (FAILED(hr)) continue;

                uint32_t pathLength;
                auto pathStr = path.GetRawBuffer(&pathLength);
                fileSystemEntries.insert(utils::StringUtils::Utf16ToUtf8(pathStr, pathLength));
            }
        }

        return fileSystemEntries;
    }

    os::ErrorCode BrokeredFileSystem::FindFirstFileW(Directory::FindHandle* findHandle, const utils::StringView<Il2CppNativeChar>& searchPathWithPattern, Il2CppNativeString* resultFileName, int32_t* resultAttributes)
    {
        using namespace ABI::Windows::Foundation::Collections;
        using namespace ABI::Windows::Storage;

        int error;
        UTF16String searchPath(searchPathWithPattern.Str(), searchPathWithPattern.Length());
        FixSlashes(searchPath);

        ComPtr<IVectorView<IStorageItem*> > foundItems;
        auto hr = FindFileSystemEntries(utils::PathUtils::DirectoryName(searchPath), searchPath, &error, &foundItems);
        if (FAILED(hr))
            return static_cast<os::ErrorCode>(error);

        ComPtr<IIterable<IStorageItem*> > foundItemsIterable;
        hr = foundItems.As(&foundItemsIterable);
        IL2CPP_ASSERT(SUCCEEDED(hr) && "Failed to cast IVectorView<IStorageItem*> to IIterable<IStorageItem*>");

        ComPtr<IIterator<IStorageItem*> > iterator;
        hr = foundItemsIterable->First(&iterator);
        if (FAILED(hr))
            return static_cast<os::ErrorCode>(HResultToWin32OrAccessDenied(hr));

        *resultAttributes = kFileAttributeDirectory;
        *resultFileName = L".";
        findHandle->handleFlags = kUseBrokeredFileSystem;
        findHandle->SetOSHandle(iterator.Detach());
        return kErrorCodeSuccess;
    }

    os::ErrorCode BrokeredFileSystem::FindNextFileW(Directory::FindHandle* findHandle, Il2CppNativeString* resultFileName, int32_t* resultAttributes)
    {
        using namespace ABI::Windows::Foundation::Collections;
        using namespace ABI::Windows::Storage;

        IL2CPP_ASSERT(findHandle->handleFlags & kUseBrokeredFileSystem);

        auto iterator = static_cast<IIterator<IStorageItem*>*>(findHandle->osHandle);

        boolean hasCurrent;
        auto hr = iterator->get_HasCurrent(&hasCurrent);
        if (FAILED(hr))
            return static_cast<os::ErrorCode>(HResultToWin32OrAccessDenied(hr));

        if (!hasCurrent)
            return kErrorCodeNoMoreFiles;

        ComPtr<IStorageItem> storageItem;
        hr = iterator->get_Current(&storageItem);
        if (FAILED(hr))
            return static_cast<os::ErrorCode>(HResultToWin32OrAccessDenied(hr));

        hr = iterator->MoveNext(&hasCurrent);
        if (FAILED(hr))
            return static_cast<os::ErrorCode>(HResultToWin32OrAccessDenied(hr));

        Microsoft::WRL::Wrappers::HString name;
        hr = storageItem->get_Name(name.GetAddressOf());
        if (FAILED(hr))
            return static_cast<os::ErrorCode>(HResultToWin32OrAccessDenied(hr));

        FileAttributes winrtAttributes;
        hr = storageItem->get_Attributes(&winrtAttributes);
        if (FAILED(hr))
            return static_cast<os::ErrorCode>(HResultToWin32OrAccessDenied(hr));

        uint32_t nameLength;
        auto nameBuffer = name.GetRawBuffer(&nameLength);
        resultFileName->assign(nameBuffer, nameBuffer + nameLength);
        *resultAttributes = TranslateWinRTAttributesToPALAttributes(winrtAttributes);
        return kErrorCodeSuccess;
    }

    os::ErrorCode BrokeredFileSystem::FindClose(void* osHandle)
    {
        using namespace ABI::Windows::Foundation::Collections;
        using namespace ABI::Windows::Storage;

        static_cast<IIterator<IStorageItem*>*>(osHandle)->Release();
        return kErrorCodeSuccess;
    }

    template<typename Operation>
    static bool MoveOrCopyFile(const UTF16String& source, const UTF16String& destination, int* error, Operation&& performOperation)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Storage;

        ComPtr<IAsyncOperation<StorageFile*> > getSourceFileOp;
        ComPtr<IAsyncOperation<StorageFolder*> > getDestinationFolderOp;

        auto fullSourcePath = GetFullPath(source);
        auto hr = GetStorageFileAsync(fullSourcePath, &getSourceFileOp);
        StoreErrorAndReturnFalseIfFailed(hr);

        UTF16String destinationFolderPath, destinationFileName;
        if (!SplitPathToFolderAndFileName(destination, destinationFolderPath, destinationFileName))
        {
            *error = ERROR_ACCESS_DENIED;
            return false;
        }

        hr = GetStorageFolderAsync(destinationFolderPath, &getDestinationFolderOp);
        StoreErrorAndReturnFalseIfFailed(hr);

        // We start getting both source file and destination folder before waiting on the first async operation to complete.

        ComPtr<IStorageFile> sourceFile;
        hr = MakeSynchronousOperation(getSourceFileOp.Get())->GetResults(&sourceFile);
        if (FAILED(hr))
        {
            auto originalHR = hr;

            // If source is not a file but a folder, we need to fail with E_ACCESSDENIED
            // In this case, GetStorageFile fails with E_INVALIDARG but we cannot tell whether
            // that means that the path is malformed or if it points to a folder, so we try to
            // get a folder and if we succeed, we change the originalHR to E_ACCESSDENIED
            ComPtr<IStorageFolder> sourceFolder;
            hr = GetStorageFolder(fullSourcePath, &sourceFolder);
            if (SUCCEEDED(hr))
                originalHR = E_ACCESSDENIED;

            StoreErrorAndReturnFalseIfFailed(originalHR);
        }

        ComPtr<IStorageFolder> destinationFolder;
        hr = MakeSynchronousOperation(getDestinationFolderOp.Get())->GetResults(&destinationFolder);
        if (FAILED(hr))
        {
            // If we cannot retrieve destination folder, we should return ERROR_PATH_NOT_FOUND
            if (hr == E_INVALIDARG || hr == MAKE_HRESULT(SEVERITY_ERROR, FACILITY_WIN32, ERROR_FILE_NOT_FOUND))
                hr = MAKE_HRESULT(SEVERITY_ERROR, FACILITY_WIN32, ERROR_PATH_NOT_FOUND);

            StoreErrorAndReturnFalseIfFailed(hr);
        }

        HStringReference destinationFileNameHString(destinationFileName.c_str(), static_cast<uint32_t>(destinationFileName.length()));
        hr = performOperation(sourceFile.Get(), destinationFolder.Get(), destinationFileNameHString.Get());
        if (FAILED(hr))
        {
            // We're being consistent with WIN32 API here
            if (hr == MAKE_HRESULT(SEVERITY_ERROR, FACILITY_WIN32, ERROR_ALREADY_EXISTS))
                hr = MAKE_HRESULT(SEVERITY_ERROR, FACILITY_WIN32, ERROR_FILE_EXISTS);

            StoreErrorAndReturnFalseIfFailed(hr);
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    bool BrokeredFileSystem::CopyFileW(const UTF16String& source, const UTF16String& destination, bool overwrite, int* error)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Storage;

        return MoveOrCopyFile(source, destination, error, [overwrite](IStorageFile* sourceFile, IStorageFolder* destinationFolder, HSTRING destinationFileName)
        {
            NameCollisionOption collisionOption = overwrite ? NameCollisionOption_ReplaceExisting : NameCollisionOption_FailIfExists;

            ComPtr<IAsyncOperation<StorageFile*> > copyOperation;
            auto hr = sourceFile->CopyOverload(destinationFolder, destinationFileName, collisionOption, &copyOperation);
            if (FAILED(hr))
                return hr;

            return MakeSynchronousOperation(copyOperation.Get())->Wait();
        });
    }

    bool BrokeredFileSystem::MoveFileW(const UTF16String& source, const UTF16String& destination, int * error)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Storage;

        return MoveOrCopyFile(source, destination, error, [](IStorageFile* sourceFile, IStorageFolder* destinationFolder, HSTRING destinationFileName)
        {
            ComPtr<IAsyncAction> moveOperation;
            auto hr = sourceFile->MoveOverloadDefaultOptions(destinationFolder, destinationFileName, &moveOperation);
            if (FAILED(hr))
                return hr;

            return MakeSynchronousOperation(moveOperation.Get())->Wait();
        });
    }

    int BrokeredFileSystem::DeleteFileW(const UTF16String& path)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Storage;

        auto fullPath = GetFullPath(path);

        ComPtr<IStorageFile> storageFile;
        auto hr = GetStorageFile(fullPath, &storageFile);
        if (FAILED(hr))
            return HResultToWin32OrAccessDenied(hr);

        ComPtr<IStorageItem> storageItem;
        hr = storageFile.As(&storageItem);
        if (FAILED(hr))
            return HResultToWin32OrAccessDenied(hr);

        return DeleteStorageItem(storageItem.Get());
    }

    UnityPalFileAttributes BrokeredFileSystem::GetFileAttributesW(const UTF16String& path, int* error)
    {
        ComPtr<ABI::Windows::Storage::IStorageItem> storageItem;
        auto hr = GetStorageItem(path, &storageItem);
        if (FAILED(hr))
        {
            *error = HResultToWin32OrAccessDenied(hr);
            return static_cast<UnityPalFileAttributes>(INVALID_FILE_ATTRIBUTES);
        }

        ABI::Windows::Storage::FileAttributes attributes;
        hr = storageItem->get_Attributes(&attributes);
        if (FAILED(hr))
        {
            *error = HResultToWin32OrAccessDenied(hr);
            return static_cast<UnityPalFileAttributes>(INVALID_FILE_ATTRIBUTES);
        }

        *error = kErrorCodeSuccess;
        return TranslateWinRTAttributesToPALAttributes(attributes);
    }

    struct StringObjectKeyValuePair : Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::WinRtClassicComMix>, Microsoft::WRL::FtmBase, ABI::Windows::Foundation::Collections::IKeyValuePair<HSTRING, IInspectable*> >
    {
    public:
        StringObjectKeyValuePair(Microsoft::WRL::Wrappers::HString key, ComPtr<IInspectable> value) :
            m_Key(std::move(key)),
            m_Value(std::move(value))
        {
        }

        HRESULT __stdcall get_Key(HSTRING* key) override
        {
            return WindowsDuplicateString(m_Key.Get(), key);
        }

        HRESULT __stdcall get_Value(IInspectable** value) override
        {
            *value = m_Value.Get();
            (*value)->AddRef();
            return S_OK;
        }

    private:
        Microsoft::WRL::Wrappers::HString m_Key;
        ComPtr<IInspectable> m_Value;
    };

    // Wraps a single object in IIterable collection
    // Used by SetFileAttributesW and GetFileStat... we only ever need one item so there's no reason to implement a full collection
    template<typename T>
    struct SingleItemIterable : Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::WinRtClassicComMix>, Microsoft::WRL::FtmBase, ABI::Windows::Foundation::Collections::IIterable<T> >
    {
        SingleItemIterable(T value) :
            m_Value(value)
        {
            il2cpp::winrt::ReferenceCounter<T>::AddRef(m_Value);
        }

        ~SingleItemIterable()
        {
            il2cpp::winrt::ReferenceCounter<T>::Release(m_Value);
        }

        HRESULT __stdcall First(ABI::Windows::Foundation::Collections::IIterator<T>** first) override
        {
            *first = Microsoft::WRL::Make<Iterator>(m_Value).Detach();
            return S_OK;
        }

    private:
        T m_Value;

        struct Iterator : Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::WinRtClassicComMix>, Microsoft::WRL::FtmBase, ABI::Windows::Foundation::Collections::IIterator<T> >
        {
            Iterator(T value) :
                m_Value(std::move(value)),
                m_HasValue(true)
            {
                il2cpp::winrt::ReferenceCounter<T>::AddRef(m_Value);
            }

            ~Iterator()
            {
                il2cpp::winrt::ReferenceCounter<T>::Release(m_Value);
            }

            virtual HRESULT __stdcall get_Current(T* current) override
            {
                if (!m_HasValue)
                    return E_BOUNDS;

                *current = m_Value;
                il2cpp::winrt::ReferenceCounter<T>::AddRef(*current);
                return S_OK;
            }

            virtual HRESULT __stdcall get_HasCurrent(boolean* hasCurrent) override
            {
                *hasCurrent = m_HasValue;
                return S_OK;
            }

            virtual HRESULT __stdcall MoveNext(boolean* hasCurrent) override
            {
                *hasCurrent = m_HasValue = false;
                return S_OK;
            }

        private:
            T m_Value;
            bool m_HasValue;
        };
    };

    static HRESULT GetStorageItemBasicProperties(ABI::Windows::Storage::IStorageItem* storageItem, ABI::Windows::Storage::FileProperties::IBasicProperties** result)
    {
        ComPtr<ABI::Windows::Foundation::IAsyncOperation<ABI::Windows::Storage::FileProperties::BasicProperties*> > filePropertiesGetOperation;
        auto hr = storageItem->GetBasicPropertiesAsync(&filePropertiesGetOperation);
        if (FAILED(hr))
            return hr;

        return MakeSynchronousOperation(filePropertiesGetOperation.Get())->GetResults(result);
    }

    bool BrokeredFileSystem::SetFileAttributesW(const UTF16String& path, UnityPalFileAttributes attributes, int* error)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Foundation::Collections;
        using namespace ABI::Windows::Storage;
        using namespace ABI::Windows::Storage::FileProperties;

        ComPtr<IAsyncAction> savePropertiesAction;

        {
            ComPtr<IStorageItem> storageItem;
            auto hr = GetStorageItem(path, &storageItem);
            StoreErrorAndReturnFalseIfFailed(hr);

            ComPtr<IBasicProperties> fileProperties;
            hr = GetStorageItemBasicProperties(storageItem.Get(), &fileProperties);
            StoreErrorAndReturnFalseIfFailed(hr);

            ComPtr<IStorageItemExtraProperties> extraFileProperties;
            hr = fileProperties.As(&extraFileProperties);
            StoreErrorAndReturnFalseIfFailed(hr);

            ComPtr<IPropertyValueStatics> propertyValueStatics;
            hr = RoGetActivationFactory(HStringReference(RuntimeClass_Windows_Foundation_PropertyValue).Get(), __uuidof(propertyValueStatics), &propertyValueStatics);
            IL2CPP_ASSERT(SUCCEEDED(hr) && "Failed to get PropertyValue statics!"); // This should never fail.

            Microsoft::WRL::Wrappers::HString propertyKey;
            hr = propertyKey.Set(L"System.FileAttributes");
            StoreErrorAndReturnFalseIfFailed(hr);

            ComPtr<IInspectable> attributesValue;
            hr = propertyValueStatics->CreateUInt32(TranslatePALAttributesToWinRTAttributes(attributes), &attributesValue);
            StoreErrorAndReturnFalseIfFailed(hr);

            auto pair = Microsoft::WRL::Make<StringObjectKeyValuePair>(std::move(propertyKey), std::move(attributesValue));
            auto propertyPair = Microsoft::WRL::Make<SingleItemIterable<IKeyValuePair<HSTRING, IInspectable*>*> >(pair.Get());
            hr = extraFileProperties->SavePropertiesAsync(propertyPair.Get(), &savePropertiesAction);
            StoreErrorAndReturnFalseIfFailed(hr);

            // We release all unneeded smart pointers before waiting on async operation
        }

        auto hr = MakeSynchronousOperation(savePropertiesAction.Get())->Wait();
        StoreErrorAndReturnFalseIfFailed(hr);

        *error = il2cpp::os::ErrorCode::kErrorCodeSuccess;
        return true;
    }

    bool BrokeredFileSystem::GetFileStat(const std::string& utf8Path, const UTF16String& path, FileStat* stat, int* error)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Foundation::Collections;
        using namespace ABI::Windows::Storage;
        using namespace ABI::Windows::Storage::FileProperties;

        FileAttributes winrtAttributes;
        DateTime creationDate, modificationDate, accessDate;
        UINT64 fileSize;
        ComPtr<IAsyncOperation<IMap<HSTRING, IInspectable*>*> > propertiesRetrievalOperation;
        HStringReference dateAccessedKeyString(L"System.DateAccessed");

        {
            ComPtr<IStorageItem> storageItem;
            auto hr = GetStorageItem(path, &storageItem);
            StoreErrorAndReturnFalseIfFailed(hr);

            hr = storageItem->get_Attributes(&winrtAttributes);
            StoreErrorAndReturnFalseIfFailed(hr);

            hr = storageItem->get_DateCreated(&creationDate);
            StoreErrorAndReturnFalseIfFailed(hr);

            ComPtr<IBasicProperties> fileProperties;
            hr = GetStorageItemBasicProperties(storageItem.Get(), &fileProperties);
            StoreErrorAndReturnFalseIfFailed(hr);

            hr = fileProperties->get_DateModified(&modificationDate);
            StoreErrorAndReturnFalseIfFailed(hr);

            hr = fileProperties->get_Size(&fileSize);
            StoreErrorAndReturnFalseIfFailed(hr);

            ComPtr<IStorageItemExtraProperties> extraFileProperties;
            hr = fileProperties.As(&extraFileProperties);
            StoreErrorAndReturnFalseIfFailed(hr);

            auto dateAccessedKey = Microsoft::WRL::Make<SingleItemIterable<HSTRING> >(dateAccessedKeyString.Get());
            hr = extraFileProperties->RetrievePropertiesAsync(dateAccessedKey.Get(), &propertiesRetrievalOperation);
            StoreErrorAndReturnFalseIfFailed(hr);

            // We release all unneeded smart pointers before waiting on async operation
        }

        ComPtr<IMap<HSTRING, IInspectable*> > propertiesMap;
        auto hr = MakeSynchronousOperation(propertiesRetrievalOperation.Get())->GetResults(&propertiesMap);
        StoreErrorAndReturnFalseIfFailed(hr);

        ComPtr<IInspectable> accessDateInspectable; // This will fail for certain file types
        if (SUCCEEDED(propertiesMap->Lookup(dateAccessedKeyString.Get(), &accessDateInspectable)))
        {
            ComPtr<IReference<DateTime> > boxedAccessDate;
            hr = accessDateInspectable.As(&boxedAccessDate);
            StoreErrorAndReturnFalseIfFailed(hr);

            hr = boxedAccessDate->get_Value(&accessDate);
            StoreErrorAndReturnFalseIfFailed(hr);
        }
        else
        {
            // Fallback to modification date if failed
            accessDate = modificationDate;
        }

        stat->attributes = TranslateWinRTAttributesToPALAttributes(winrtAttributes);
        stat->name = il2cpp::utils::PathUtils::Basename(utf8Path);
        stat->length = fileSize;
        stat->creation_time = creationDate.UniversalTime;
        stat->last_write_time = modificationDate.UniversalTime;
        stat->last_access_time = accessDate.UniversalTime;

        *error = il2cpp::os::ErrorCode::kErrorCodeSuccess;
        return true;
    }

    FileHandle* BrokeredFileSystem::Open(const UTF16String& path, uint32_t desiredAccess, uint32_t shareMode, uint32_t creationDisposition, uint32_t flagsAndAttributes, int* error)
    {
        using namespace ABI::Windows::Foundation;
        using namespace ABI::Windows::Storage;

        UTF16String parentFolderName, name;
        if (!SplitPathToFolderAndFileName(path, parentFolderName, name))
        {
            *error = ERROR_ACCESS_DENIED;
            return reinterpret_cast<FileHandle*>(INVALID_HANDLE_VALUE);
        }

        ComPtr<IStorageFolder> parentFolder;
        auto hr = GetStorageFolder(parentFolderName, &parentFolder);
        if (FAILED(hr))
        {
            *error = HResultToWin32OrAccessDenied(hr);
            return reinterpret_cast<FileHandle*>(INVALID_HANDLE_VALUE);
        }

        ComPtr<winrt_interfaces::IStorageFolderHandleAccess> folderHandleAccess;
        hr = parentFolder.As(&folderHandleAccess);
        if (FAILED(hr))
        {
            *error = ERROR_ACCESS_DENIED;
            return reinterpret_cast<FileHandle*>(INVALID_HANDLE_VALUE);
        }

        int translatedAccess = winrt_interfaces::HAO_NONE;
        if (desiredAccess & GENERIC_READ)
            translatedAccess |= winrt_interfaces::HAO_READ | winrt_interfaces::HAO_READ_ATTRIBUTES;

        if (desiredAccess & GENERIC_WRITE)
            translatedAccess |= winrt_interfaces::HAO_WRITE;

        HANDLE fileHandle;
        hr = folderHandleAccess->Create(name.c_str(),
            static_cast<winrt_interfaces::HANDLE_CREATION_OPTIONS>(creationDisposition),
            static_cast<winrt_interfaces::HANDLE_ACCESS_OPTIONS>(translatedAccess),
            static_cast<winrt_interfaces::HANDLE_SHARING_OPTIONS>(shareMode),
            static_cast<winrt_interfaces::HANDLE_OPTIONS>(flagsAndAttributes & winrt_interfaces::HO_ALL_POSSIBLE_OPTIONS),
            nullptr,
            &fileHandle);
        if (FAILED(hr))
        {
            *error = ERROR_ACCESS_DENIED;
            return reinterpret_cast<FileHandle*>(INVALID_HANDLE_VALUE);
        }

        *error = ERROR_SUCCESS;
        return reinterpret_cast<FileHandle*>(fileHandle);
    }

    void BrokeredFileSystem::CleanupStatics()
    {
        s_StorageFileStatics.Release();
        s_StorageFolderStatics.Release();
    }
}
}

#endif
