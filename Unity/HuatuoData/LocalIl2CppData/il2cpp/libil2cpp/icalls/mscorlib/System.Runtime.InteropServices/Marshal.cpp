#include "il2cpp-config.h"

#include "icalls/mscorlib/System.Runtime.InteropServices/Marshal.h"

#include "il2cpp-class-internals.h"
#include "il2cpp-tabledefs.h"

#include "gc/GarbageCollector.h"
#include "metadata/FieldLayout.h"
#include "os/Atomic.h"
#include "vm/Array.h"
#include "vm/CCW.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Field.h"
#include "vm/Image.h"
#include "vm/LastError.h"
#include "vm/MarshalAlloc.h"
#include "vm/Object.h"
#include "vm/PlatformInvoke.h"
#include "vm/RCW.h"
#include "vm/String.h"
#include "vm/Type.h"
#include "utils/MarshalingUtils.h"
#include "utils/StringUtils.h"
#include <string>
#include <deque>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Runtime
{
namespace InteropServices
{
    int32_t Marshal::AddRefInternal(intptr_t pUnk)
    {
        return static_cast<Il2CppIUnknown*>((void*)pUnk)->AddRef();
    }

    intptr_t Marshal::AllocCoTaskMem(int32_t size)
    {
        intptr_t result;
        result = (intptr_t)vm::MarshalAlloc::Allocate(size);
        return result;
    }

    intptr_t Marshal::AllocHGlobal(intptr_t size)
    {
        intptr_t result;
        result = (intptr_t)vm::MarshalAlloc::AllocateHGlobal((size_t)size);
        return result;
    }

    void Marshal::copy_from_unmanaged(intptr_t source, int startIndex, Il2CppArray * destination, int length)
    {
        uint32_t element_size = (uint32_t)il2cpp_array_element_size(destination->klass);
        memcpy(il2cpp_array_addr_with_size(destination, element_size, startIndex), reinterpret_cast<void*>(source), length * element_size);
    }

    void Marshal::copy_to_unmanaged(Il2CppArray * source, int32_t startIndex, intptr_t destination, int32_t length)
    {
        uint32_t element_size = (uint32_t)il2cpp_array_element_size(source->klass);
        memcpy(reinterpret_cast<void*>(destination), il2cpp_array_addr_with_size(source, element_size, startIndex), length * element_size);
    }

    void Marshal::FreeBSTR(intptr_t ptr)
    {
        vm::PlatformInvoke::MarshalFreeBString(reinterpret_cast<Il2CppChar*>(ptr));
    }

    void Marshal::FreeCoTaskMem(intptr_t ptr)
    {
        vm::MarshalAlloc::Free(reinterpret_cast<void*>(ptr));
    }

    void Marshal::FreeHGlobal(intptr_t hglobal)
    {
        vm::MarshalAlloc::FreeHGlobal(reinterpret_cast<void*>(hglobal));
    }

    bool Marshal::IsComObject(Il2CppObject* o)
    {
        if (o == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("o"));

        return o->klass->is_import_or_windows_runtime;
    }

    intptr_t Marshal::GetCCW(Il2CppObject* o, Il2CppReflectionType* T)
    {
        if (o == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("o"));

        if (T == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("T"));

        Il2CppClass* klass = vm::Class::FromIl2CppType(T->type);

        if (!vm::Class::IsInterface(klass))
        {
            DECLARE_NATIVE_C_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(parameter, IL2CPP_NATIVE_STRING("T"));
            DECLARE_NATIVE_C_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(message, IL2CPP_NATIVE_STRING("The T parameter is not an interface."));
            vm::Exception::Raise(vm::Exception::GetArgumentException(parameter, message));
        }

        if (vm::Class::IsGeneric(klass))
        {
            DECLARE_NATIVE_C_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(parameter, IL2CPP_NATIVE_STRING("T"));
            DECLARE_NATIVE_C_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(message, IL2CPP_NATIVE_STRING("The T parameter is a generic type."));
            vm::Exception::Raise(vm::Exception::GetArgumentException(parameter, message));
        }

        const Il2CppInteropData* interopData = klass->interopData;
        if (interopData == NULL || interopData->guid == NULL)
        {
            DECLARE_NATIVE_C_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(parameter, IL2CPP_NATIVE_STRING("T"));
            DECLARE_NATIVE_C_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(message, IL2CPP_NATIVE_STRING("The specified type must be visible from COM."));
            vm::Exception::Raise(vm::Exception::GetArgumentException(parameter, message));
        }

        return reinterpret_cast<intptr_t>(vm::CCW::GetOrCreate(o, *interopData->guid));
    }

    int32_t Marshal::GetComSlotForMethodInfoInternal(mscorlib_System_Reflection_MemberInfo * m)
    {
        NOT_SUPPORTED_IL2CPP(Marshal::GetComSlotForMethodInfoInternal, "Not implemented.");
        return 0;
    }

    Il2CppDelegate* Marshal::GetDelegateForFunctionPointerInternal(intptr_t ptr, Il2CppReflectionType* t)
    {
        Il2CppClass* delegateType = il2cpp::vm::Class::FromIl2CppType(t->type);
        return vm::PlatformInvoke::MarshalFunctionPointerToDelegate(reinterpret_cast<void*>(ptr), delegateType);
    }

    intptr_t Marshal::GetFunctionPointerForDelegateInternal(Il2CppDelegate* d)
    {
        return vm::PlatformInvoke::MarshalDelegate(d);
    }

    intptr_t Marshal::GetIDispatchForObjectInternal(Il2CppObject* o)
    {
        NOT_SUPPORTED_IL2CPP(Marshal::GetIDispatchForObjectInternal, "Not implemented.");
        return 0;
    }

    intptr_t Marshal::GetIUnknownForObjectInternal(Il2CppObject* o)
    {
        NOT_SUPPORTED_IL2CPP(Marshal::GetIUnknownForObjectInternal, "This icall is not supported by il2cpp. Use the il2cpp_codegen_com_get_iunknown_for_object intrinsic instead.");
        return 0;
    }

    Il2CppObject* Marshal::GetObjectForCCW(intptr_t pUnk)
    {
        if (pUnk == 0)
            return NULL;

        return vm::RCW::GetOrCreateFromIUnknown(reinterpret_cast<Il2CppIUnknown*>(pUnk), il2cpp_defaults.il2cpp_com_object_class);
    }

    Il2CppString* Marshal::PtrToStringBSTR(intptr_t ptr)
    {
        if (ptr == 0)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("ptr"));

        return vm::PlatformInvoke::MarshalCppBStringToCSharpStringResult(reinterpret_cast<const Il2CppChar*>(ptr));
    }

    Il2CppString* Marshal::PtrToStringAnsi_mscorlib_System_String_mscorlib_System_IntPtr(intptr_t ptr)
    {
        char* value = (char*)ptr;
        if (value == NULL)
            return NULL;
        return vm::String::New(value);
    }

    Il2CppString* Marshal::PtrToStringAnsi_mscorlib_System_String_mscorlib_System_IntPtr_mscorlib_System_Int32(intptr_t ptr, int32_t len)
    {
        char* value = (char*)ptr;
        if (value == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("ptr"));

        return vm::String::NewLen(value, len);
    }

    Il2CppString* Marshal::PtrToStringUni_mscorlib_System_String_mscorlib_System_IntPtr(intptr_t ptr)
    {
        Il2CppChar* value = reinterpret_cast<Il2CppChar*>(ptr);
        if (value == NULL)
            return NULL;

        int32_t len = 0;
        Il2CppChar* t = value;

        while (*t++)
            len++;

        return vm::String::NewUtf16(value, len);
    }

    Il2CppString* Marshal::PtrToStringUni_mscorlib_System_String_mscorlib_System_IntPtr_mscorlib_System_Int32(intptr_t ptr, int32_t len)
    {
        Il2CppChar* value = reinterpret_cast<Il2CppChar*>(ptr);
        if (value == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("ptr"));

        return vm::String::NewUtf16(value, len);
    }

    Il2CppObject* Marshal::PtrToStructure(intptr_t ptr, Il2CppReflectionType* structureType)
    {
        if (ptr == 0)
            return NULL;

        if (structureType == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("structureType"));

        Il2CppClass* type = vm::Class::FromIl2CppType(structureType->type);

        Il2CppTypeEnum typeType = structureType->type->type;

        if (typeType == IL2CPP_TYPE_STRING || typeType == IL2CPP_TYPE_SZARRAY || (typeType == IL2CPP_TYPE_CLASS && !vm::Class::HasDefaultConstructor(type)))
        {
            vm::Exception::Raise(vm::Exception::GetMissingMethodException("No parameterless constructor defined for this object."));
        }

        if (type->interopData != NULL && type->interopData->pinvokeMarshalFromNativeFunction != NULL)
        {
            Il2CppObject* result = vm::Object::New(type);

            if (typeType == IL2CPP_TYPE_CLASS)
            {
                typedef void (*Constructor)(Il2CppObject*);
                Constructor ctor = reinterpret_cast<Constructor>(vm::Class::GetMethodFromName(type, ".ctor", 0)->methodPointer);
                ctor(result);
                utils::MarshalingUtils::MarshalStructFromNative(reinterpret_cast<void*>(ptr), result, type->interopData);
            }
            else
            {
                utils::MarshalingUtils::MarshalStructFromNative(reinterpret_cast<void*>(ptr), vm::Object::Unbox(result), type->interopData);
            }

            return result;
        }

        // If there's no custom marshal function, it means it's either a primitive, or invalid argument
        if (type->native_size != -1)
        {
            // We may also need to throw a NotSupportedException for an ArgIterator.
            if (structureType->type->type == IL2CPP_TYPE_VOID)
                vm::Exception::Raise(vm::Exception::GetNotSupportedException("Cannot dynamically create an instance of System.Void."));

            // PtrToStructure is supposed to throw on enums
            if (!type->enumtype)
            {
                Il2CppObject* result = vm::Object::New(type);
                memcpy(vm::Object::Unbox(result), reinterpret_cast<void*>(ptr), type->native_size);
                return result;
            }
        }

        // If we got this far, throw an exception

        if (type->generic_class != NULL || type->is_generic)
            vm::Exception::Raise(vm::Exception::GetArgumentException("structure", "The specified object must not be an instance of a generic type."));

        vm::Exception::Raise(vm::Exception::GetArgumentException("structure", "The specified structure must be blittable or have layout information."));
        return NULL;
    }

    void Marshal::PtrToStructureObject(intptr_t ptr, Il2CppObject* structure)
    {
        if (ptr == 0)
            return;

        if (structure == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("structure"));

        Il2CppClass* type = structure->klass;

        // This is only legal for classes.
        if (type->byval_arg.type != IL2CPP_TYPE_CLASS)
        {
            vm::Exception::Raise(vm::Exception::GetArgumentException("structure", "The specified structure must be an instance of a formattable class."));
        }

        if (type->interopData != NULL && type->interopData->pinvokeMarshalFromNativeFunction != NULL)
        {
            utils::MarshalingUtils::MarshalStructFromNative(reinterpret_cast<void*>(ptr), structure, type->interopData);
            return;
        }

        if (type->generic_class || type->is_generic)
            vm::Exception::Raise(vm::Exception::GetArgumentException("structure", "The specified object must not be an instance of a generic type."));

        vm::Exception::Raise(vm::Exception::GetArgumentException("structure", "The specified structure must be blittable or have layout information."));
    }

    int32_t Marshal::QueryInterfaceInternal(intptr_t pUnk, mscorlib_System_Guid * iid, intptr_t* ppv)
    {
        Il2CppIUnknown* unknown = reinterpret_cast<Il2CppIUnknown*>(pUnk);
        return unknown->QueryInterface(reinterpret_cast<Il2CppGuid&>(*iid), reinterpret_cast<void**>(ppv));
    }

    template<typename T>
    static inline T ReadValue(intptr_t ptr, int32_t offset)
    {
        return *reinterpret_cast<T*>(reinterpret_cast<uint8_t*>(ptr) + offset);
    }

    uint8_t Marshal::ReadByte(intptr_t ptr, int32_t ofs)
    {
        return ReadValue<uint8_t>(ptr, ofs);
    }

    int16_t Marshal::ReadInt16(intptr_t ptr, int32_t ofs)
    {
        return ReadValue<int16_t>(ptr, ofs);
    }

    int32_t Marshal::ReadInt32(intptr_t ptr, int32_t ofs)
    {
        return ReadValue<int32_t>(ptr, ofs);
    }

    int64_t Marshal::ReadInt64(intptr_t ptr, int32_t ofs)
    {
        return ReadValue<int64_t>(ptr, ofs);
    }

    intptr_t Marshal::ReadIntPtr(intptr_t ptr, int32_t ofs)
    {
        return ReadValue<intptr_t>(ptr, ofs);
    }

    int32_t Marshal::ReleaseInternal(intptr_t pUnk)
    {
        return reinterpret_cast<Il2CppIUnknown*>(pUnk)->Release();
    }

    int32_t Marshal::ReleaseComObjectInternal(Il2CppObject* co)
    {
        // There's a check in mscorlib before calling this internal icall, so assert instead of full check is OK here.
        IL2CPP_ASSERT(co->klass->is_import_or_windows_runtime);

        int32_t newRefCount = os::Atomic::Decrement(&static_cast<Il2CppComObject*>(co)->refCount);
        if (newRefCount == 0)
        {
            // We can't really release the COM object directly, because it might have additional
            // fields that cache different interfaces. So let's just call its finalizer here.
            // In order to deal with the fact that this may get called from different threads
            // at the same time, we (atomically) register a NULL finalizer, and if another finalizer
            // was already registered, we call it. If there was no finalizer registered, it means
            // that we lost the race and we should just carry on.
            gc::GarbageCollector::FinalizerCallback oldFinalizer = gc::GarbageCollector::RegisterFinalizerWithCallback(co, NULL);
            if (oldFinalizer != NULL)
                oldFinalizer(co, NULL);
        }

        return newRefCount;
    }

    int Marshal::SizeOf(Il2CppReflectionType* rtype)
    {
        if (rtype == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("t"));

        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(rtype->type);

        if (typeInfo->native_size != -1)
        {
            // SizeOf "cannot" get size of enums and strings
            if (!typeInfo->enumtype && rtype->type->type != IL2CPP_TYPE_STRING)
                return typeInfo->native_size;
        }

        // If we get to here, type does not have a native size
        // So let's figure what kind of exception to throw

        if (typeInfo->generic_class != NULL || typeInfo->is_generic)
            vm::Exception::Raise(vm::Exception::GetArgumentException("t", "The t parameter is a generic type."));

        std::string exceptionMessage = utils::StringUtils::Printf("Type \'%s\' cannot be marshaled as an unmanaged structure; no meaningful size or offset can be computed.",
            vm::Type::GetName(rtype->type, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME).c_str());

        vm::Exception::Raise(vm::Exception::GetArgumentException(NULL, exceptionMessage.c_str()));
        return 0;
    }

    intptr_t Marshal::StringToBSTR(Il2CppString* s)
    {
        return reinterpret_cast<intptr_t>(vm::PlatformInvoke::MarshalCSharpStringToCppBString(s));
    }

    intptr_t Marshal::StringToHGlobalUni(Il2CppString* s)
    {
        if (s == NULL)
            return 0;

        int32_t size = utils::StringUtils::GetLength(s);
        const Il2CppChar* utf16 = utils::StringUtils::GetChars(s);
        size_t bytes = (size + 1) * 2;
        Il2CppChar* cstr = static_cast<Il2CppChar*>(vm::MarshalAlloc::AllocateHGlobal(bytes));
        memcpy(cstr, utf16, bytes);
        return reinterpret_cast<intptr_t>(cstr);
    }

    intptr_t Marshal::StringToHGlobalAnsi(Il2CppString* s)
    {
        if (s == NULL)
            return 0;

        const Il2CppChar* utf16 = utils::StringUtils::GetChars(s);
        std::string str = il2cpp::utils::StringUtils::Utf16ToUtf8(utf16);
        char *cstr = (char*)vm::MarshalAlloc::AllocateHGlobal(str.size() + 1);
        strcpy(cstr, str.c_str());
        return reinterpret_cast<intptr_t>(cstr);
    }

    void Marshal::StructureToPtr(Il2CppObject* structure, intptr_t ptr, bool deleteOld)
    {
        if (structure == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("structure"));

        if (ptr == 0)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("ptr"));

        Il2CppClass* type = structure->klass;

        if (type->interopData != NULL && type->interopData->pinvokeMarshalToNativeFunction != NULL)
        {
            if (deleteOld)
                utils::MarshalingUtils::MarshalFreeStruct(reinterpret_cast<void*>(ptr), type->interopData);

            void* objectPtr = (type->byval_arg.type == IL2CPP_TYPE_CLASS) ? structure : vm::Object::Unbox(structure);
            utils::MarshalingUtils::MarshalStructToNative(objectPtr, reinterpret_cast<void*>(ptr), type->interopData);
            return;
        }

        // If there's no custom marshal function, it means it's either a primitive, or invalid argument

        if (type->native_size != -1)
        {
            // StructureToPtr is supposed to throw on strings and enums
            if (!type->enumtype && type->byval_arg.type != IL2CPP_TYPE_STRING)
            {
                memcpy(reinterpret_cast<void*>(ptr), vm::Object::Unbox(structure), type->native_size);
                return;
            }
        }

        // If we got this far, throw an exception
        Il2CppException* exception;

        if (type->generic_class != NULL || type->is_generic)
        {
            exception = vm::Exception::GetArgumentException("structure", "The specified object must not be an instance of a generic type.");
        }
        else
        {
            exception = vm::Exception::GetArgumentException("structure", "The specified structure must be blittable or have layout information.");
        }

        vm::Exception::Raise(exception);
    }

    template<typename T>
    static inline void WriteValue(intptr_t ptr, int32_t offset, T value)
    {
        *reinterpret_cast<T*>(reinterpret_cast<uint8_t*>(ptr) + offset) = value;
    }

    void Marshal::WriteByte(intptr_t ptr, int32_t ofs, uint8_t val)
    {
        WriteValue(ptr, ofs, val);
    }

    void Marshal::WriteInt16(intptr_t ptr, int32_t ofs, int16_t val)
    {
        WriteValue(ptr, ofs, val);
    }

    void Marshal::WriteInt32(intptr_t ptr, int32_t ofs, int32_t val)
    {
        WriteValue(ptr, ofs, val);
    }

    void Marshal::WriteInt64(intptr_t ptr, int32_t ofs, int64_t val)
    {
        WriteValue(ptr, ofs, val);
    }

    void Marshal::WriteIntPtr(intptr_t ptr, int32_t ofs, intptr_t val)
    {
        WriteValue(ptr, ofs, val);
    }

    void Marshal::DestroyStructure(intptr_t ptr, Il2CppReflectionType* structureType)
    {
        if (ptr == 0)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("ptr"));

        if (structureType == NULL)
            vm::Exception::Raise(vm::Exception::GetArgumentNullException("structureType"));

        Il2CppClass* type = vm::Class::FromIl2CppType(structureType->type);

        // If cleanup function exists, it will call it and return true
        // In that case, we're done.
        if (utils::MarshalingUtils::MarshalFreeStruct(reinterpret_cast<void*>(ptr), type->interopData))
            return;

        if (type->is_generic)
        {
            vm::Exception::Raise(vm::Exception::GetArgumentException("structureType", "The specified type must not be an instance of a generic type."));
        }

        // Enums are blittable, but they don't have layout information, therefore Marshal.DestroyStructure is supposed to throw
        if (!type->enumtype)
        {
            // Char and Boolean are not blittable, but they should not raise this exception, as we can call DestroyStructure on them without problems.
            if (type->is_blittable || structureType->type->type == IL2CPP_TYPE_CHAR || structureType->type->type == IL2CPP_TYPE_BOOLEAN)
                return;
        }

        // If we got this far, throw an exception
        vm::Exception::Raise(vm::Exception::GetArgumentException("structureType", "The specified type must be blittable or have layout information."));
    }

    int32_t Marshal::GetLastWin32Error()
    {
        return vm::LastError::GetLastError();
    }

    static size_t RoundUpToMultiple(size_t numToRound, size_t multiple)
    {
        if (multiple == 0)
            return numToRound;

        size_t remainder = numToRound % multiple;
        if (remainder == 0)
            return numToRound;
        return numToRound + multiple - remainder;
    }

    intptr_t Marshal::OffsetOf(Il2CppReflectionType* t, Il2CppString* fieldName)
    {
        std::string fieldNameToFind = utils::StringUtils::Utf16ToUtf8(fieldName->chars);
        Il2CppClass* type = vm::Class::FromIl2CppType(t->type);

        FieldInfo* field = vm::Class::GetFieldFromName(type, fieldNameToFind.c_str());
        if (field == NULL || (vm::Field::GetFlags(field) & FIELD_ATTRIBUTE_STATIC))
        {
            std::string message;
            message = "Field '" + fieldNameToFind + "' is not a marshaled member of the type '" + type->name + "'";
            vm::Exception::Raise(vm::Exception::GetArgumentException("fieldName", message.c_str()));
        }

        // Order the base classes so the most base class is first.
        std::deque<Il2CppClass*> inheritanceHierarchy;
        while (type)
        {
            inheritanceHierarchy.push_front(type);
            type = type->parent;
        }

        bool fieldFound = false;

        size_t offset = 0;
        FieldInfo* previousField = NULL;
        for (std::deque<Il2CppClass*>::iterator it = inheritanceHierarchy.begin(); it < inheritanceHierarchy.end(); ++it)
        {
            type = *it;
            void* iter = NULL;
            while ((field = vm::Class::GetFields(type, &iter)))
            {
                if (vm::Field::GetFlags(field) & FIELD_ATTRIBUTE_STATIC)
                    continue;

                // Determine how much the previous field added to the offset.
                if (previousField != NULL)
                {
                    if (!vm::Type::IsStruct(previousField->type))
                    {
                        size_t managedOffset = field->offset - previousField->offset;
                        if (managedOffset != 0) // overlapping fields have a zero offset
                        {
                            offset += vm::Class::GetFieldMarshaledSize(previousField);
                        }
                    }
                    else
                    {
                        offset += vm::Class::FromIl2CppType(previousField->type)->native_size;
                    }

                    if (offset != 0)
                    {
                        int marshaledFieldAlignment = vm::Class::GetFieldMarshaledAlignment(field);
                        offset = RoundUpToMultiple(offset, type->packingSize == 0 ? marshaledFieldAlignment : std::min((int)type->packingSize, marshaledFieldAlignment));
                    }
                }
                previousField = field;

                if (fieldNameToFind == vm::Field::GetName(field))
                {
                    fieldFound = true;
                    break;
                }
            }

            if (fieldFound)
                break;
        }

        return static_cast<intptr_t>(offset);
    }

    // We _could_ implement these two if we wanted - we'd just need to init their metadata
    void Marshal::Prelink(Il2CppReflectionMethod* m)
    {
        NOT_SUPPORTED_IL2CPP(Marshal::Prelink, "Not implemented.");
    }

    void Marshal::PrelinkAll(Il2CppReflectionType* c)
    {
        NOT_SUPPORTED_IL2CPP(Marshal::PrelinkAll, "Not implemented.");
    }

    intptr_t Marshal::ReAllocCoTaskMem(intptr_t ptr, int32_t size)
    {
        return reinterpret_cast<intptr_t>(vm::MarshalAlloc::ReAlloc(reinterpret_cast<void*>(ptr), size));
    }

    intptr_t Marshal::ReAllocHGlobal(intptr_t ptr, intptr_t size)
    {
        if (ptr == 0)
            vm::Exception::RaiseOutOfMemoryException();

        return reinterpret_cast<intptr_t>(vm::MarshalAlloc::ReAllocHGlobal(reinterpret_cast<void*>(ptr), (size_t)size));
    }

    intptr_t Marshal::UnsafeAddrOfPinnedArrayElement(Il2CppArray* arr, int32_t index)
    {
        return reinterpret_cast<intptr_t>(il2cpp_array_addr_with_size(arr, il2cpp_array_element_size(arr->klass), index));
    }

    intptr_t Marshal::BufferToBSTR(Il2CppArray* ptr, int32_t slen)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Marshal::BufferToBSTR);
        IL2CPP_UNREACHABLE;
    }

    int32_t Marshal::GetHRForException_WinRT(Il2CppException* e)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Marshal::GetHRForException_WinRT);
        IL2CPP_UNREACHABLE;
    }

    intptr_t Marshal::GetRawIUnknownForComObjectNoAddRef(Il2CppObject* o)
    {
        IL2CPP_ASSERT(o->klass->is_import_or_windows_runtime);

        return reinterpret_cast<intptr_t>(static_cast<Il2CppComObject*>(o)->identity);
    }

    Il2CppObject* Marshal::GetNativeActivationFactory(Il2CppObject* type)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Marshal::GetNativeActivationFactory);
        IL2CPP_UNREACHABLE;
    }

    intptr_t Marshal::AllocCoTaskMemSize(intptr_t sizet)
    {
        intptr_t result;
        result = (intptr_t)vm::MarshalAlloc::Allocate(sizet);
        return result;
    }
} /* namespace InteropServices */
} /* namespace Runtime */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
