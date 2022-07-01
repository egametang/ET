#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "vm/Object.h"
#include "vm/CCW.h"
#include "vm/Class.h"
#include "vm/CachedCCWBase.h"
#include "vm/Exception.h"
#include "vm/MetadataCache.h"
#include "vm/RCW.h"
#include "vm/Runtime.h"
#include "vm/ScopedThreadAttacher.h"
#include "vm/String.h"

namespace il2cpp
{
namespace vm
{
    struct ManagedObject : CachedCCWBase<ManagedObject>
    {
        inline ManagedObject(Il2CppObject* obj) :
            CachedCCWBase<ManagedObject>(obj)
        {
        }

        virtual il2cpp_hresult_t STDCALL QueryInterface(const Il2CppGuid& iid, void** object) IL2CPP_OVERRIDE
        {
            if (::memcmp(&iid, &Il2CppIUnknown::IID, sizeof(Il2CppGuid)) == 0
                || ::memcmp(&iid, &Il2CppIInspectable::IID, sizeof(Il2CppGuid)) == 0
                || ::memcmp(&iid, &Il2CppIAgileObject::IID, sizeof(Il2CppGuid)) == 0)
            {
                *object = GetIdentity();
                AddRefImpl();
                return IL2CPP_S_OK;
            }

            if (::memcmp(&iid, &Il2CppIManagedObjectHolder::IID, sizeof(Il2CppGuid)) == 0)
            {
                *object = static_cast<Il2CppIManagedObjectHolder*>(this);
                AddRefImpl();
                return IL2CPP_S_OK;
            }

            if (::memcmp(&iid, &Il2CppIMarshal::IID, sizeof(Il2CppGuid)) == 0)
            {
                *object = static_cast<Il2CppIMarshal*>(this);
                AddRefImpl();
                return IL2CPP_S_OK;
            }

            if (::memcmp(&iid, &Il2CppIWeakReferenceSource::IID, sizeof(Il2CppGuid)) == 0)
            {
                *object = static_cast<Il2CppIWeakReferenceSource*>(this);
                AddRefImpl();
                return IL2CPP_S_OK;
            }

            *object = NULL;
            return IL2CPP_E_NOINTERFACE;
        }
    };

    Il2CppIUnknown* CCW::CreateCCW(Il2CppObject* obj)
    {
        // check for ccw create function, which is implemented by objects that implement COM or Windows Runtime interfaces
        const Il2CppInteropData* interopData = obj->klass->interopData;
        if (interopData != NULL)
        {
            const CreateCCWFunc createCcw = interopData->createCCWFunction;

            if (createCcw != NULL)
                return createCcw(obj);
        }

        // otherwise create generic ccw object that "only" implements IUnknown, IMarshal, IInspectable, IAgileObject and IManagedObjectHolder interfaces
        void* memory = utils::Memory::Malloc(sizeof(ManagedObject));
        if (memory == NULL)
            Exception::RaiseOutOfMemoryException();
        return static_cast<Il2CppIManagedObjectHolder*>(new(memory) ManagedObject(obj));
    }

    Il2CppObject* CCW::Unpack(Il2CppIUnknown* unknown)
    {
        Il2CppIManagedObjectHolder* managedHolder;
        il2cpp_hresult_t hr = unknown->QueryInterface(Il2CppIManagedObjectHolder::IID, reinterpret_cast<void**>(&managedHolder));
        Exception::RaiseIfFailed(hr, true);

        Il2CppObject* instance = managedHolder->GetManagedObject();
        managedHolder->Release();

        IL2CPP_ASSERT(instance);
        return instance;
    }

    static Il2CppString* ValueToStringFallbackToEmpty(Il2CppObject* value)
    {
        Il2CppClass* klass = il2cpp::vm::Object::GetClass(value);
        const MethodInfo* toStringMethod = il2cpp::vm::Class::GetMethodFromName(klass, "ToString", 0);

        Il2CppException* exception = NULL;
        Il2CppString* result = (Il2CppString*)il2cpp::vm::Runtime::Invoke(toStringMethod, value, NULL, &exception);
        if (exception != NULL)
            return String::Empty();

        return result;
    }

    static il2cpp_hresult_t HandleInvalidIPropertyConversionImpl(const std::string& exceptionMessage)
    {
        ScopedThreadAttacher scopedThreadAttacher; // Make sure we're attached before we create exceptions (aka allocate managed memory)

        Il2CppException* exception = Exception::GetInvalidCastException(exceptionMessage.c_str());
        Exception::PrepareExceptionForThrow(exception);
        Exception::StoreExceptionInfo(exception, ValueToStringFallbackToEmpty(exception));
        return exception->hresult;
    }

    il2cpp_hresult_t CCW::HandleInvalidIPropertyConversion(const char* fromType, const char* toType)
    {
        std::string message = il2cpp::utils::StringUtils::Printf("Object in an IPropertyValue is of type '%s', which cannot be converted to a '%s'.", fromType, toType);
        return HandleInvalidIPropertyConversionImpl(message);
    }

    il2cpp_hresult_t CCW::HandleInvalidIPropertyConversion(Il2CppObject* value, const char* fromType, const char* toType)
    {
        Il2CppString* valueStr = ValueToStringFallbackToEmpty(value);
        std::string message = il2cpp::utils::StringUtils::Printf(
            "Object in an IPropertyValue is of type '%s' with value '%s', which cannot be converted to a '%s'.",
            fromType,
            utils::StringUtils::Utf16ToUtf8(valueStr->chars, valueStr->length).c_str(),
            toType);
        return HandleInvalidIPropertyConversionImpl(message);
    }

    il2cpp_hresult_t CCW::HandleInvalidIPropertyArrayConversion(const char* fromArrayType, const char* fromElementType, const char* toElementType, il2cpp_array_size_t index)
    {
        std::string message = il2cpp::utils::StringUtils::Printf(
            "Object in an IPropertyValue is of type '%s' which cannot be converted to a '%s[]' due to array element '%d': Object in an IPropertyValue is of type '%s', which cannot be converted to a '%s'.",
            fromArrayType,
            toElementType,
            static_cast<int>(index),
            fromElementType,
            toElementType);
        return HandleInvalidIPropertyConversionImpl(message);
    }

    il2cpp_hresult_t CCW::HandleInvalidIPropertyArrayConversion(Il2CppObject* value, const char* fromArrayType, const char* fromElementType, const char* toElementType, il2cpp_array_size_t index)
    {
        Il2CppString* valueStr = ValueToStringFallbackToEmpty(value);
        std::string message = il2cpp::utils::StringUtils::Printf(
            "Object in an IPropertyValue is of type '%s' which cannot be converted to a '%s[]' due to array element '%d': Object in an IPropertyValue is of type '%s' with value '%s', which cannot be converted to a '%s'.",
            fromArrayType,
            toElementType,
            static_cast<int>(index),
            fromElementType,
            utils::StringUtils::Utf16ToUtf8(valueStr->chars, valueStr->length).c_str(),
            toElementType);
        return HandleInvalidIPropertyConversionImpl(message);
    }
} /* namespace vm */
} /* namespace il2cpp */
