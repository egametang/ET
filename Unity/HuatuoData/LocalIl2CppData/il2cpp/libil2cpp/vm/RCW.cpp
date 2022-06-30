#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-tabledefs.h"
#include "gc/GCHandle.h"
#include "metadata/GenericMetadata.h"
#include "vm/Exception.h"
#include "vm/Field.h"
#include "vm/GenericClass.h"
#include "vm/MetadataCache.h"
#include "vm/Object.h"
#include "vm/PlatformInvoke.h"
#include "vm/RCW.h"
#include "vm/Runtime.h"
#include "os/Atomic.h"
#include "os/COM.h"
#include "vm/Monitor.h"
#include "os/Mutex.h"
#include "os/WindowsRuntime.h"
#include "utils/Il2CppError.h"
#include "utils/Il2CppHashMap.h"
#include "utils/HashUtils.h"
#include "utils/StringUtils.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

const Il2CppGuid Il2CppIUnknown::IID = { 0x00000000, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46 };
const Il2CppGuid Il2CppISequentialStream::IID = { 0x0c733a30, 0x2a1c, 0x11ce, 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d };
const Il2CppGuid Il2CppIStream::IID = { 0x0000000c, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46 };
const Il2CppGuid Il2CppIMarshal::IID = { 0x00000003, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46 };
const Il2CppGuid Il2CppIManagedObject::IID = { 0xc3fcc19e, 0xa970, 0x11d2, 0x8b, 0x5a, 0x00, 0xa0, 0xc9, 0xb7, 0xc9, 0xc4 };
const Il2CppGuid Il2CppIManagedObjectHolder::IID = { 0xd4bbc1c8, 0xf5bf, 0x4647, 0x94, 0x95, 0x2e, 0x5c, 0xf, 0x20, 0xf7, 0x5d };
const Il2CppGuid Il2CppIInspectable::IID = { 0xaf86e2e0, 0xb12d, 0x4c6a, 0x9c, 0x5a, 0xd7, 0xaa, 0x65, 0x10, 0x1E, 0x90 };
const Il2CppGuid Il2CppIActivationFactory::IID = { 0x00000035, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46 };
const Il2CppGuid Il2CppIRestrictedErrorInfo::IID = { 0x82ba7092, 0x4c88, 0x427d, 0xa7, 0xbc, 0x16, 0xdd, 0x93, 0xfe, 0xb6, 0x7e };
const Il2CppGuid Il2CppILanguageExceptionErrorInfo::IID = { 0x04a2dbf3, 0xdf83, 0x116c, 0x09, 0x46, 0x08, 0x12, 0xab, 0xf6, 0xe0, 0x7d };
const Il2CppGuid Il2CppIAgileObject::IID = { 0x94ea2b94, 0xe9cc, 0x49e0, 0xc0, 0xff, 0xee, 0x64, 0xca, 0x8f, 0x5b, 0x90 };
const Il2CppGuid Il2CppIWeakReference::IID = { 0x00000037, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46 };
const Il2CppGuid Il2CppIWeakReferenceSource::IID = { 0x00000038, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46 };

namespace il2cpp
{
namespace vm
{
    typedef Il2CppHashMap<Il2CppIUnknown*, /* Weak GC Handle */ uint32_t, il2cpp::utils::PointerHash<Il2CppIUnknown> > RCWCache;

    static baselib::ReentrantLock s_RCWCacheMutex;
    static RCWCache s_RCWCache;

    void RCW::Register(Il2CppComObject* rcw)
    {
        os::FastAutoLock lock(&s_RCWCacheMutex);
        rcw->refCount = 1;
        auto weakRef = gc::GCHandle::NewWeakref(rcw, false);
        vm::Exception::RaiseIfError(weakRef.GetError());
        const bool inserted = s_RCWCache.insert(std::make_pair(rcw->identity, weakRef.Get())).second;
        Assert(inserted);
    }

    static inline Il2CppIUnknown* GetIdentity(Il2CppIUnknown* unknown)
    {
        Il2CppIUnknown* identity;
        il2cpp_hresult_t hr = unknown->QueryInterface(Il2CppIUnknown::IID, reinterpret_cast<void**>(&identity));
        vm::Exception::RaiseIfFailed(hr, true);
        IL2CPP_ASSERT(identity);

        return identity;
    }

// Shameless comment copycat from .NET Native (https://github.com/dotnet/corert/blob/374c3d47992a7c444ec7d1dfe94b1780de942a55/src/System.Private.Interop/src/Shared/McgComHelpers.cs#L557):
// 1. Prefer using the class returned from GetRuntimeClassName
// 2. Otherwise use the class (if there) in the signature
// 3. Out of options - create Il2CppComObject
    static inline Il2CppClass* GetClassForRCW(Il2CppIInspectable* inspectable, Il2CppClass* fallbackClass)
    {
        Il2CppHString className;
        il2cpp_hresult_t hr = inspectable->GetRuntimeClassName(&className);
        if (IL2CPP_HR_FAILED(hr) || className == NULL)
            return fallbackClass;

        uint32_t classNameLength;
        auto classNamePtr = os::WindowsRuntime::GetHStringBuffer(className, &classNameLength);
        vm::Exception::RaiseIfError(classNamePtr.GetError());
        std::string classNameUtf8 = utils::StringUtils::Utf16ToUtf8(classNamePtr.Get(), classNameLength);
        os::WindowsRuntime::DeleteHString(className);

        Il2CppClass* rcwClass = MetadataCache::GetWindowsRuntimeClass(classNameUtf8.c_str());
        return rcwClass != NULL ? rcwClass : fallbackClass;
    }

    static inline Il2CppClass* GetClassForRCW(Il2CppIUnknown* unknown, Il2CppClass* fallbackClass)
    {
        Il2CppIInspectable* inspectable;
        il2cpp_hresult_t hr = unknown->QueryInterface(Il2CppIInspectable::IID, reinterpret_cast<void**>(&inspectable));

        if (IL2CPP_HR_FAILED(hr))
            return fallbackClass;

        Il2CppClass* result = GetClassForRCW(inspectable, fallbackClass);
        inspectable->Release();
        return result;
    }

    Il2CppObject* ReboxIReference(Il2CppIUnknown* comObject, Il2CppClass* objectClass);
    Il2CppObject* ReboxKeyValuePair(Il2CppIUnknown* comObject, Il2CppClass* keyValuePairGenericInstance);
    Il2CppObject* ReboxUri(Il2CppIUnknown* comObject);

    Il2CppObject* ReboxIfBoxed(Il2CppIUnknown* comObject, Il2CppClass* objectClass)
    {
        if (strcmp(objectClass->namespaze, "Windows.Foundation") == 0)
        {
            if (strcmp(objectClass->name, "IReference`1") == 0 || strcmp(objectClass->name, "IReferenceArray`1") == 0)
                return ReboxIReference(comObject, objectClass);
        }
        else if (strcmp(objectClass->namespaze, "System.Collections.Generic") == 0 && strcmp(objectClass->name, "KeyValuePair`2") == 0)
        {
            return ReboxKeyValuePair(comObject, objectClass);
        }
        else if (objectClass == il2cpp_defaults.system_uri_class)
        {
            return ReboxUri(comObject);
        }

        return NULL;
    }

    Il2CppObject* ReboxIReference(Il2CppIUnknown* comObject, Il2CppClass* objectClass)
    {
        Class::Init(objectClass);

        // Sanity checks
        IL2CPP_ASSERT(Class::IsInflated(objectClass));
        IL2CPP_ASSERT(objectClass->vtable_count == 1); // IReference`1<T> only has get_Value method

        const MethodInfo* getValueMethod = objectClass->vtable[0].method;
        IL2CPP_ASSERT(strcmp(getValueMethod->name, "get_Value") == 0);

        // We don't really want to allocate it on the GC heap for this little invocation
        Il2CppComObject fakeRcw;
        memset(&fakeRcw, 0, sizeof(fakeRcw));
        fakeRcw.klass = objectClass;
        fakeRcw.identity = comObject;

        Il2CppException* exception = NULL;
        Il2CppObject* reboxed = Runtime::Invoke(getValueMethod, &fakeRcw, NULL, &exception);

        if (exception != NULL)
            Exception::Raise(exception);

        return reboxed;
    }

    Il2CppObject* ReboxKeyValuePair(Il2CppIUnknown* comObject, Il2CppClass* keyValuePairGenericInstance)
    {
        Class::Init(keyValuePairGenericInstance);

        // Sanity checks
        IL2CPP_ASSERT(Class::IsInflated(keyValuePairGenericInstance));
        IL2CPP_ASSERT(il2cpp_defaults.ikey_value_pair_class != NULL);

        // Retrieve Windows.Foundation.Collections.IKeyValuePair`1<K, V> generic instance
        Il2CppGenericClass* iKeyValuePairGenericClass = metadata::GenericMetadata::GetGenericClass(il2cpp_defaults.ikey_value_pair_class, keyValuePairGenericInstance->generic_class->context.class_inst);
        Il2CppClass* iKeyValuePairGenericInstance = GenericClass::GetClass(iKeyValuePairGenericClass);
        Class::Init(iKeyValuePairGenericInstance);

        IL2CPP_ASSERT(iKeyValuePairGenericInstance->vtable_count == 2);

        const MethodInfo* getKeyMethod = iKeyValuePairGenericInstance->vtable[0].method;
        IL2CPP_ASSERT(strcmp(getKeyMethod->name, "get_Key") == 0);

        const MethodInfo* getValueMethod = iKeyValuePairGenericInstance->vtable[1].method;
        IL2CPP_ASSERT(strcmp(getValueMethod->name, "get_Value") == 0);

        Il2CppComObject fakeRcw;
        memset(&fakeRcw, 0, sizeof(fakeRcw));
        fakeRcw.klass = il2cpp_defaults.il2cpp_com_object_class;
        fakeRcw.identity = comObject;

        // Create new boxed key value pair
        Il2CppObject* reboxed = Object::New(keyValuePairGenericInstance);

        for (uint16_t i = 0; i < 2; i++)
        {
            const MethodInfo* methodToInvoke = NULL;
            const FieldInfo& field = keyValuePairGenericInstance->fields[i];

            // Figure out which getter to call
            if (strcmp(field.name, "key") == 0)
            {
                methodToInvoke = getKeyMethod;
            }
            else if (strcmp(field.name, "value") == 0)
            {
                methodToInvoke = getValueMethod;
            }

            // Call the getter
            Il2CppException* exception = NULL;
            Il2CppObject* fieldValue = Runtime::Invoke(methodToInvoke, &fakeRcw, NULL, &exception);

            if (exception != NULL)
                Exception::Raise(exception);

            // Set the field in our reboxed key value pair instance
            if (Class::FromIl2CppType(field.type)->byval_arg.valuetype)
            {
                Field::SetValue(reboxed, &field, Object::Unbox(fieldValue));
            }
            else
            {
                Field::SetValue(reboxed, &field, fieldValue);
            }
        }

        return reboxed;
    }

    Il2CppObject* ReboxUri(Il2CppIUnknown* comObject)
    {
        Il2CppClass* systemUriClass = il2cpp_defaults.system_uri_class;
        Il2CppClass* iUriRuntimeClassClass = il2cpp_defaults.windows_foundation_iuri_runtime_class_class;

        Class::Init(systemUriClass);
        Class::Init(iUriRuntimeClassClass);

        const int kGetRawUriMethodIndex = 10; // IUriRuntimeClass::get_RawUri
        IL2CPP_ASSERT(iUriRuntimeClassClass->vtable_count > kGetRawUriMethodIndex);

        VirtualInvokeData getRawUriInvokeData = iUriRuntimeClassClass->vtable[kGetRawUriMethodIndex];
        IL2CPP_ASSERT(strcmp(getRawUriInvokeData.method->name, "get_RawUri") == 0);

        Il2CppComObject fakeRcw;
        memset(&fakeRcw, 0, sizeof(fakeRcw));
        fakeRcw.klass = il2cpp_defaults.il2cpp_com_object_class;
        fakeRcw.identity = comObject;

        Il2CppObject* rawUri = Runtime::InvokeWithThrow(getRawUriInvokeData.method, &fakeRcw, NULL);

        const MethodInfo* uriConstructor = NULL;
        uint16_t uriMethodCount = systemUriClass->method_count;

        for (uint16_t i = 0; i < uriMethodCount; i++)
        {
            const MethodInfo* method = systemUriClass->methods[i];
            if (strcmp(method->name, ".ctor") == 0 && method->parameters_count == 1 && method->parameters[0]->type == IL2CPP_TYPE_STRING)
            {
                uriConstructor = method;
                break;
            }
        }

        IL2CPP_ASSERT(uriConstructor);
        Il2CppObject* reboxedUri = Object::New(systemUriClass);
        void* constructorArgs[1] = { rawUri };

        Runtime::InvokeWithThrow(uriConstructor, reboxedUri, constructorArgs);
        return reboxedUri;
    }

    template<typename T, bool isSealedClassInstance>
    static inline Il2CppObject* GetOrCreateRCW(T* comObject, Il2CppClass* objectClass)
    {
        IL2CPP_ASSERT(comObject != NULL);

        if (!isSealedClassInstance)
        {
            // 1. Check if comObject is actually our COM Callable Wrapper
            Il2CppIManagedObjectHolder* managedHolder;
            il2cpp_hresult_t hr = comObject->QueryInterface(Il2CppIManagedObjectHolder::IID, reinterpret_cast<void**>(&managedHolder));
            if (IL2CPP_HR_SUCCEEDED(hr))
            {
                Il2CppObject* instance = managedHolder->GetManagedObject();
                managedHolder->Release();

                IL2CPP_ASSERT(instance);
                return instance;
            }
        }

        Il2CppIUnknown* identity = GetIdentity(comObject);

        // 2. Try to find it in RCW cache
        os::FastAutoLock lock(&s_RCWCacheMutex);
        RCWCache::iterator iter = s_RCWCache.find(identity);
        if (iter != s_RCWCache.end())
        {
            Il2CppComObject* obj = static_cast<Il2CppComObject*>(gc::GCHandle::GetTarget(iter->second));
            if (obj != NULL)
            {
                // Make sure the RCW isn't dead. If increment returns 1, it means
                // that the ref count had previous reached 0 and was released
                if (os::Atomic::Increment(&obj->refCount) > 1)
                {
                    identity->Release();
                    identity = NULL;
                    return obj;
                }
            }

            // The RCW was already queued for finalization or destroyed by ref count reaching 0.
            // Erase it from the cache and let us create a new one.
            s_RCWCache.erase(iter);
        }

        // 3. Figure out the concrete RCW class
        if (!isSealedClassInstance)
        {
            Il2CppClass* fallbackClass = objectClass;
            objectClass = GetClassForRCW(comObject, fallbackClass);

            // If object class is one of the blessed unboxable classes,
            // unbox the object from its windows runtime representation,
            // unmarshal it, box it to Il2CppObject and return it
            //
            // Current list of unboxable classes:
            //     Windows.Foundation.IReference`1<T>
            //     Windows.Foundation.IReferenceArray`1<T>
            //     System.Collections.Generic.KeyValuePair`2<K, V>
            //     System.Uri
            Il2CppObject* reboxed = ReboxIfBoxed(comObject, objectClass);
            if (reboxed != NULL)
                return reboxed;

            if (objectClass->byval_arg.type != IL2CPP_TYPE_CLASS ||
                objectClass->flags & TYPE_ATTRIBUTE_INTERFACE ||
                objectClass->is_generic)
            {
                // We must be able to instantiate the type. If we can't, fallback to a caller passed in type
                objectClass = fallbackClass;
            }
        }

        IL2CPP_ASSERT(Class::HasParent(objectClass, il2cpp_defaults.il2cpp_com_object_class));

        // 4. Create RCW object
        Il2CppComObject* rcw = static_cast<Il2CppComObject*>(Object::New(objectClass));
        rcw->identity = identity;
        rcw->refCount = 1;

        // 5. Insert it into the cache
        auto weakRef = gc::GCHandle::NewWeakref(rcw, false);
        vm::Exception::RaiseIfError(weakRef.GetError());
        const bool inserted = s_RCWCache.insert(std::make_pair(identity, weakRef.Get())).second;
        Assert(inserted);

        return rcw;
    }

    Il2CppObject* RCW::GetOrCreateFromIUnknown(Il2CppIUnknown* unknown, Il2CppClass* fallbackClass)
    {
        return GetOrCreateRCW<Il2CppIUnknown, false>(unknown, fallbackClass);
    }

    Il2CppObject* RCW::GetOrCreateFromIInspectable(Il2CppIInspectable* inspectable, Il2CppClass* fallbackClass)
    {
        return GetOrCreateRCW<Il2CppIInspectable, false>(inspectable, fallbackClass);
    }

    Il2CppObject* RCW::GetOrCreateForSealedClass(Il2CppIUnknown* unknown, Il2CppClass* objectClass)
    {
        return GetOrCreateRCW<Il2CppIUnknown, true>(unknown, objectClass);
    }

    void RCW::Cleanup(Il2CppComObject* rcw)
    {
        if (rcw->klass->is_import_or_windows_runtime)
        {
            os::FastAutoLock lock(&s_RCWCacheMutex);

            RCWCache::iterator iter = s_RCWCache.find(rcw->identity);

            // It is possible for us to not find object in the cache if two RCWs for the same IUnknown get
            // finalized in a row: then, the first finalizer will remove the NULL object, and the second one
            // will not find it.
            if (iter != s_RCWCache.end())
            {
                Il2CppObject* obj = gc::GCHandle::GetTarget(iter->second);

                // If it's null, it means that the cache contains our object
                // but the weak GC handle has been invalidated by the GC already
                // If it's equal to our object, it means that RCW::Cleanup was
                // called manually, and we should also delete it from the cache
                // Otherwise, it's a different object. It means that we have already
                // created a new RCW in place of this one during the time
                // it had been queued for finalization
                if (obj == NULL || obj == rcw)
                    s_RCWCache.erase(iter);
            }
        }

        int32_t shortCacheSize = rcw->qiShortCacheSize;
        for (int32_t i = 0; i < shortCacheSize; i++)
            rcw->qiShortCache[i].qiResult->Release();

        int32_t longCacheSize = rcw->qiLongCacheSize;
        if (longCacheSize > 0)
        {
            for (int32_t i = 0; i < longCacheSize; i++)
                rcw->qiLongCache[i].qiResult->Release();

            IL2CPP_FREE(rcw->qiLongCache);
        }
    }

    Il2CppIUnknown* RCW::QueryInterfaceCached(Il2CppComObject* rcw, const Il2CppGuid& iid)
    {
        MonitorHolder monitorHolder(rcw);

        int32_t shortCacheSize = rcw->qiShortCacheSize;
        for (int32_t i = 0; i < shortCacheSize; i++)
        {
            const Il2CppGuid* queriedInterface = rcw->qiShortCache[i].iid;
            if (queriedInterface == &iid)
                return rcw->qiShortCache[i].qiResult;
        }

        int32_t longCacheSize = rcw->qiLongCacheSize;
        for (int32_t i = 0; i < longCacheSize; i++)
        {
            const Il2CppGuid* queriedInterface = rcw->qiLongCache[i].iid;
            if (queriedInterface == &iid)
                return rcw->qiLongCache[i].qiResult;
        }

        return NULL;
    }

    bool RCW::CacheQueriedInterface(Il2CppComObject* rcw, const Il2CppGuid& iid, Il2CppIUnknown* queriedInterface)
    {
        MonitorHolder monitorHolder(rcw);

        QICache cache = { &iid, queriedInterface };

        // We need to rescan caches in case another thread got to cache it first
        int32_t shortCacheSize = rcw->qiShortCacheSize;
        IL2CPP_ASSERT(shortCacheSize <= IL2CPP_ARRAY_SIZE(rcw->qiShortCache));

        for (int32_t i = 0; i < shortCacheSize; i++)
        {
            const Il2CppGuid* queriedInterface = rcw->qiShortCache[i].iid;
            if (queriedInterface == &iid)
                return false;
        }

        if (shortCacheSize == IL2CPP_ARRAY_SIZE(rcw->qiShortCache))
        {
            // We only need to check long cache if short cache is full
            int32_t longCacheSize = rcw->qiLongCacheSize;
            for (int32_t i = 0; i < longCacheSize; i++)
            {
                const Il2CppGuid* queriedInterface = rcw->qiLongCache[i].iid;
                if (queriedInterface == &iid)
                    return false;
            }
        }
        else
        {
            rcw->qiShortCache[shortCacheSize] = cache;
            rcw->qiShortCacheSize = shortCacheSize + 1;
            return true;
        }

        int32_t longCacheSize = rcw->qiLongCacheSize;
        int32_t longCacheCapacity = rcw->qiLongCacheCapacity;
        IL2CPP_ASSERT(longCacheSize <= longCacheCapacity);

        if (longCacheSize == longCacheCapacity)
        {
            longCacheCapacity *= 2;
            rcw->qiLongCache = static_cast<QICache*>(IL2CPP_REALLOC(rcw->qiLongCache, sizeof(QICache) * longCacheCapacity));
            rcw->qiLongCacheCapacity = longCacheCapacity;
        }

        rcw->qiLongCache[longCacheSize] = cache;
        rcw->qiLongCacheSize = longCacheSize + 1;
        return true;
    }

    const VirtualInvokeData* RCW::GetComInterfaceInvokeData(Il2CppClass* queriedInterface, const Il2CppClass* targetInterface, Il2CppMethodSlot slot)
    {
        Class::Init(queriedInterface);
        uint16_t vtableCount = queriedInterface->vtable_count;

        if (targetInterface->generic_class != NULL)
        {
            Il2CppMetadataGenericContainerHandle containerHandle = MetadataCache::GetGenericContainerFromGenericClass(targetInterface->image, targetInterface->generic_class);

            if (Class::IsGenericClassAssignableFrom(targetInterface, queriedInterface, targetInterface->image, containerHandle))
                return NULL;

            const Il2CppRuntimeInterfaceOffsetPair* interfaceOffsets = queriedInterface->interfaceOffsets;
            uint16_t interfaceOffsetsCount = queriedInterface->interface_offsets_count;
            for (uint16_t i = 0; i < interfaceOffsetsCount; i++)
            {
                if (Class::IsGenericClassAssignableFrom(targetInterface, interfaceOffsets[i].interfaceType, targetInterface->image, containerHandle))
                {
                    Il2CppMethodSlot slotWithOffset = interfaceOffsets[i].offset + slot;
                    if (slotWithOffset < vtableCount)
                        return &queriedInterface->vtable[slotWithOffset];
                }
            }
        }
        else
        {
            const Il2CppRuntimeInterfaceOffsetPair* interfaceOffsets = queriedInterface->interfaceOffsets;
            uint16_t interfaceOffsetsCount = queriedInterface->interface_offsets_count;
            for (uint16_t i = 0; i < interfaceOffsetsCount; ++i)
            {
                if (interfaceOffsets[i].interfaceType == targetInterface)
                {
                    Il2CppMethodSlot slotWithOffset = interfaceOffsets[i].offset + slot;
                    if (slotWithOffset < vtableCount)
                        return &queriedInterface->vtable[slotWithOffset];
                }
            }
        }

        Il2CppClass* const* implementedInterfaces = queriedInterface->implementedInterfaces;
        uint16_t implementedInterfacesCount = queriedInterface->interfaces_count;

        for (uint16_t i = 0; i < implementedInterfacesCount; i++)
        {
            Il2CppClass* implementedInterface = implementedInterfaces[i];
            const VirtualInvokeData* invokeData = GetComInterfaceInvokeData(implementedInterface, targetInterface, slot);
            if (invokeData != NULL)
                return invokeData;
        }

        return NULL;
    }

    const VirtualInvokeData* RCW::GetComInterfaceInvokeData(Il2CppComObject* rcw, const Il2CppClass* targetInterface, Il2CppMethodSlot slot)
    {
        uint16_t vtableCount = targetInterface->vtable_count;
        if (slot < vtableCount)
        {
            const Il2CppInteropData* itfInteropData = targetInterface->interopData;
            if (itfInteropData != NULL)
            {
                const Il2CppGuid* itfGuid = itfInteropData->guid;
                if (itfGuid != NULL)
                {
                    // Try querying for the interface we were asked
                    if (RCW::QueryInterfaceNoAddRef<false>(rcw, *itfGuid) != NULL)
                        return &targetInterface->vtable[slot];
                }
            }
        }

        if (targetInterface->is_import_or_windows_runtime)
            return NULL;

        // For projected interfaces, we look in the cache for compatible interface in order to handle these scenarios:
        // * Covariable/Contravariance. For instance, we should be able to invoke IReadOnlyList<object> methods on IReadOnlyList<string>, even though if QI fails for IVectorView<object>
        // * Inherited interfaces on CLR but not Windows Runtime side. For instance, IEnumerable<T> implements IEnumerable but IIterable<T> does not implement IBindableIterable
        MonitorHolder monitorHolder(rcw);

        int32_t shortCacheSize = rcw->qiShortCacheSize;
        for (int32_t i = 0; i < shortCacheSize; i++)
        {
            Il2CppClass* queriedInterface = vm::MetadataCache::GetClassForGuid(rcw->qiShortCache[i].iid);
            if (queriedInterface != NULL)
            {
                const VirtualInvokeData* invokeData = GetComInterfaceInvokeData(queriedInterface, targetInterface, slot);
                if (invokeData != NULL)
                    return invokeData;
            }
        }

        int32_t longCacheSize = rcw->qiLongCacheSize;
        for (int32_t i = 0; i < longCacheSize; i++)
        {
            Il2CppClass* queriedInterface = vm::MetadataCache::GetClassForGuid(rcw->qiLongCache[i].iid);
            if (queriedInterface != NULL)
            {
                const VirtualInvokeData* invokeData = GetComInterfaceInvokeData(queriedInterface, targetInterface, slot);
                if (invokeData != NULL)
                    return invokeData;
            }
        }

        if (slot < vtableCount)
            return &targetInterface->vtable[slot];

        return NULL;
    }
} /* namespace vm */
} /* namespace il2cpp */
