#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct mscorlib_System_Guid;
struct mscorlib_System_Reflection_MemberInfo;

struct Il2CppObject;
struct Il2CppDelegate;
struct Il2CppReflectionType;
struct Il2CppString;

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
    class LIBIL2CPP_CODEGEN_API Marshal
    {
    public:
        static int32_t GetLastWin32Error();
        static void SetLastWin32Error(uint32_t);
        static int32_t AddRefInternal(intptr_t pUnk);
        static intptr_t AllocCoTaskMem(int32_t size);
        static intptr_t AllocHGlobal(intptr_t size);
        static void DestroyStructure(intptr_t ptr, Il2CppReflectionType* structureType);
        static void FreeBSTR(intptr_t ptr);
        static void FreeCoTaskMem(intptr_t ptr);
        static void FreeHGlobal(intptr_t hglobal);
        static intptr_t GetCCW(Il2CppObject* o, Il2CppReflectionType * T);
        static int32_t GetComSlotForMethodInfoInternal(mscorlib_System_Reflection_MemberInfo * m);
        static Il2CppDelegate* GetDelegateForFunctionPointerInternal(intptr_t ptr, Il2CppReflectionType* t);
        static intptr_t GetFunctionPointerForDelegateInternal(Il2CppDelegate* d);
        static intptr_t GetIDispatchForObjectInternal(Il2CppObject* o);
        static intptr_t GetIUnknownForObjectInternal(Il2CppObject* o);
        static Il2CppObject* GetObjectForCCW(intptr_t pUnk);
        static bool IsComObject(Il2CppObject* o);
        static intptr_t OffsetOf(Il2CppReflectionType* t, Il2CppString* fieldName);
        static void Prelink(Il2CppReflectionMethod* m);
        static void PrelinkAll(Il2CppReflectionType* c);
        static Il2CppString* PtrToStringAnsi_mscorlib_System_String_mscorlib_System_IntPtr(intptr_t ptr);
        static Il2CppString* PtrToStringAnsi_mscorlib_System_String_mscorlib_System_IntPtr_mscorlib_System_Int32(intptr_t ptr, int32_t len);
        static Il2CppString* PtrToStringBSTR(intptr_t ptr);
        static Il2CppString* PtrToStringUni_mscorlib_System_String_mscorlib_System_IntPtr(intptr_t ptr);
        static Il2CppString* PtrToStringUni_mscorlib_System_String_mscorlib_System_IntPtr_mscorlib_System_Int32(intptr_t ptr, int32_t len);
        static Il2CppObject* PtrToStructure(intptr_t ptr, Il2CppReflectionType * structureType);
        static void PtrToStructureObject(intptr_t ptr, Il2CppObject* structure);
        static int32_t QueryInterfaceInternal(intptr_t pUnk, mscorlib_System_Guid * iid, intptr_t* ppv);
        static intptr_t ReAllocCoTaskMem(intptr_t ptr, int32_t size);
        static intptr_t ReAllocHGlobal(intptr_t ptr, intptr_t size);
        static uint8_t ReadByte(intptr_t ptr, int32_t ofs);
        static int16_t ReadInt16(intptr_t ptr, int32_t ofs);
        static int32_t ReadInt32(intptr_t ptr, int32_t ofs);
        static int64_t ReadInt64(intptr_t ptr, int32_t ofs);
        static intptr_t ReadIntPtr(intptr_t ptr, int32_t ofs);
        static int32_t ReleaseComObjectInternal(Il2CppObject* co);
        static int32_t ReleaseInternal(intptr_t pUnk);
        static int SizeOf(Il2CppReflectionType * rtype);
        static intptr_t StringToBSTR(Il2CppString* s);
        static intptr_t StringToHGlobalAnsi(Il2CppChar* s, int32_t length);
        static intptr_t StringToHGlobalUni(Il2CppChar* s, int32_t length);
        static void StructureToPtr(Il2CppObject* structure, intptr_t ptr, bool deleteOld);
        static intptr_t UnsafeAddrOfPinnedArrayElement(Il2CppArray* arr, int32_t index);
        static void WriteByte(intptr_t ptr, int32_t ofs, uint8_t val);
        static void WriteInt16(intptr_t ptr, int32_t ofs, int16_t val);
        static void WriteInt32(intptr_t ptr, int32_t ofs, int32_t val);
        static void WriteInt64(intptr_t ptr, int32_t ofs, int64_t val);
        static void copy_from_unmanaged(intptr_t, int, Il2CppArray *, int);
        static void copy_to_unmanaged(Il2CppArray * source, int32_t startIndex, intptr_t destination, int32_t length);
        static void WriteIntPtr(intptr_t ptr, int32_t ofs, intptr_t val);

        static intptr_t BufferToBSTR(Il2CppChar* ptr, int32_t slen);

        static int32_t GetHRForException_WinRT(Il2CppException* e);
        static intptr_t GetRawIUnknownForComObjectNoAddRef(Il2CppObject* o);
        static Il2CppObject* GetNativeActivationFactory(Il2CppObject* type);

        static intptr_t AllocCoTaskMemSize(intptr_t sizet);

        static void copy_from_unmanaged_fixed(intptr_t source, int32_t startIndex, Il2CppArray* destination, int32_t length, void* fixed_destination_element);
        static void copy_to_unmanaged_fixed(Il2CppArray* source, int32_t startIndex, intptr_t destination, int32_t length, void* fixed_source_element);
    };
} /* namespace InteropServices */
} /* namespace Runtime */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
