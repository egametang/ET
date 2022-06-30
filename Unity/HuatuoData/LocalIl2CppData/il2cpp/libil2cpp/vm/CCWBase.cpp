#include "il2cpp-config.h"
#include "il2cpp-string-types.h"
#include "CCWBase.h"
#include "metadata/GenericMetadata.h"
#include "os/WindowsRuntime.h"
#include "vm/Class.h"
#include "vm/GenericClass.h"
#include "vm/MetadataCache.h"
#include "vm/WeakReference.h"
#include "utils/StringUtils.h"

static inline const Il2CppClass* GetBoxedWindowsRuntimeClass(const Il2CppClass* typeDefinition, const Il2CppClass* genericArg)
{
    const Il2CppType* klass = &genericArg->byval_arg;
    const Il2CppGenericInst* inst = il2cpp::vm::MetadataCache::GetGenericInst(&klass, 1);
    Il2CppGenericClass* genericClass = il2cpp::metadata::GenericMetadata::GetGenericClass(typeDefinition, inst);
    return il2cpp::vm::GenericClass::GetClass(genericClass);
}

static inline bool CanPotentiallyBeBoxedToWindowsRuntime(const Il2CppClass* klass)
{
    if (il2cpp::vm::Class::IsInflated(klass))
        return false;

    if (il2cpp::vm::Class::IsValuetype(klass))
        return true;

    if (klass == il2cpp_defaults.string_class)
        return true;

    return false;
}

il2cpp_hresult_t il2cpp::vm::CCWBase::GetRuntimeClassNameImpl(Il2CppHString* className)
{
    const Il2CppClass* objectClass = GetManagedObjectInline()->klass;
    if (il2cpp_defaults.ireference_class != NULL && CanPotentiallyBeBoxedToWindowsRuntime(objectClass))
    {
        // For value types/strings we're supposed to return the name of its boxed representation, i.e. Windows.Foundation.IReference`1<T>
        objectClass = GetBoxedWindowsRuntimeClass(il2cpp_defaults.ireference_class, objectClass);
    }
    else if (il2cpp_defaults.ireferencearray_class != NULL && objectClass->rank > 0)
    {
        // For arrays of value types/strings we're supposed to return the name of its boxed representation too, i.e. Windows.Foundation.IReferenceArray`1<T>
        const Il2CppClass* elementClass = objectClass->element_class;
        if (CanPotentiallyBeBoxedToWindowsRuntime(elementClass))
        {
            objectClass = GetBoxedWindowsRuntimeClass(il2cpp_defaults.ireferencearray_class, elementClass);
        }
        else if (elementClass == il2cpp_defaults.object_class || strcmp(elementClass->image->assembly->aname.name, "WindowsRuntimeMetadata") == 0)
        {
            // Object arrays can be boxed, but objects cannot, so we need to special case it
            // For object and WindowsRuntime classes arrays, we also return Windows.Foundation.IReferenceArray`1<Object>
            return os::WindowsRuntime::CreateHString(utils::StringView<Il2CppNativeChar>(IL2CPP_NATIVE_STRING("Windows.Foundation.IReferenceArray`1<Object>")), className);
        }
    }

    const char* name = MetadataCache::GetWindowsRuntimeClassName(objectClass);
    if (name == NULL)
    {
        *className = NULL;
        return IL2CPP_S_OK;
    }

    UTF16String nameUtf16 = utils::StringUtils::Utf8ToUtf16(name);
    return os::WindowsRuntime::CreateHString(utils::StringView<Il2CppChar>(nameUtf16.c_str(), nameUtf16.length()), className);
}

Il2CppObject* STDCALL il2cpp::vm::CCWBase::GetManagedObject()
{
    return GetManagedObjectInline();
}

il2cpp_hresult_t STDCALL il2cpp::vm::CCWBase::GetWeakReference(Il2CppIWeakReference** weakReference)
{
    return WeakReference::Create(GetManagedObjectInline(), weakReference);
}
