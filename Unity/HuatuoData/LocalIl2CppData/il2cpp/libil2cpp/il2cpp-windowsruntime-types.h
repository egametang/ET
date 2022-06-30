#pragma once
#include "il2cpp-config.h"
#include <stdint.h>

typedef struct Il2CppHString__
{
    int unused;
} Il2CppHString__;

typedef Il2CppHString__* Il2CppHString;

typedef struct Il2CppHStringHeader
{
    union
    {
        void* Reserved1;
#if IL2CPP_SIZEOF_VOID_P == 8
        char Reserved2[24];
#else
        char Reserved2[20];
#endif
    } Reserved;
} Il2CppHStringHeader;

// System.Guid
typedef struct Il2CppGuid
{
    uint32_t data1;
    uint16_t data2;
    uint16_t data3;
    uint8_t data4[8];
} Il2CppGuid;

typedef struct Il2CppSafeArrayBound
{
    uint32_t element_count;
    int32_t lower_bound;
} Il2CppSafeArrayBound;

typedef struct Il2CppSafeArray
{
    uint16_t dimension_count;
    uint16_t features;
    uint32_t element_size;
    uint32_t lock_count;
    void* data;
    Il2CppSafeArrayBound bounds[1];
} Il2CppSafeArray;

typedef struct Il2CppWin32Decimal
{
    uint16_t reserved;
    union
    {
        struct
        {
            uint8_t scale;
            uint8_t sign;
        } s;
        uint16_t signscale;
    } u;
    uint32_t hi32;
    union
    {
        struct
        {
            uint32_t lo32;
            uint32_t mid32;
        } s2;
        uint64_t lo64;
    } u2;
} Il2CppWin32Decimal;

typedef int16_t IL2CPP_VARIANT_BOOL;

#define IL2CPP_VARIANT_TRUE ((IL2CPP_VARIANT_BOOL)-1)
#define IL2CPP_VARIANT_FALSE ((IL2CPP_VARIANT_BOOL)0)

typedef enum Il2CppVarType
{
    IL2CPP_VT_EMPTY = 0,
    IL2CPP_VT_NULL = 1,
    IL2CPP_VT_I2 = 2,
    IL2CPP_VT_I4 = 3,
    IL2CPP_VT_R4 = 4,
    IL2CPP_VT_R8 = 5,
    IL2CPP_VT_CY = 6,
    IL2CPP_VT_DATE = 7,
    IL2CPP_VT_BSTR = 8,
    IL2CPP_VT_DISPATCH = 9,
    IL2CPP_VT_ERROR = 10,
    IL2CPP_VT_BOOL = 11,
    IL2CPP_VT_VARIANT = 12,
    IL2CPP_VT_UNKNOWN = 13,
    IL2CPP_VT_DECIMAL = 14,
    IL2CPP_VT_I1 = 16,
    IL2CPP_VT_UI1 = 17,
    IL2CPP_VT_UI2 = 18,
    IL2CPP_VT_UI4 = 19,
    IL2CPP_VT_I8 = 20,
    IL2CPP_VT_UI8 = 21,
    IL2CPP_VT_INT = 22,
    IL2CPP_VT_UINT = 23,
    IL2CPP_VT_VOID = 24,
    IL2CPP_VT_HRESULT = 25,
    IL2CPP_VT_PTR = 26,
    IL2CPP_VT_SAFEARRAY = 27,
    IL2CPP_VT_CARRAY = 28,
    IL2CPP_VT_USERDEFINED = 29,
    IL2CPP_VT_LPSTR = 30,
    IL2CPP_VT_LPWSTR = 31,
    IL2CPP_VT_RECORD = 36,
    IL2CPP_VT_INT_PTR = 37,
    IL2CPP_VT_UINT_PTR = 38,
    IL2CPP_VT_FILETIME = 64,
    IL2CPP_VT_BLOB = 65,
    IL2CPP_VT_STREAM = 66,
    IL2CPP_VT_STORAGE = 67,
    IL2CPP_VT_STREAMED_OBJECT = 68,
    IL2CPP_VT_STORED_OBJECT = 69,
    IL2CPP_VT_BLOB_OBJECT = 70,
    IL2CPP_VT_CF = 71,
    IL2CPP_VT_CLSID = 72,
    IL2CPP_VT_VERSIONED_STREAM = 73,
    IL2CPP_VT_BSTR_BLOB = 0xfff,
    IL2CPP_VT_VECTOR = 0x1000,
    IL2CPP_VT_ARRAY = 0x2000,
    IL2CPP_VT_BYREF = 0x4000,
    IL2CPP_VT_RESERVED = 0x8000,
    IL2CPP_VT_ILLEGAL = 0xffff,
    IL2CPP_VT_ILLEGALMASKED = 0xfff,
    IL2CPP_VT_TYPEMASK = 0xfff,
} Il2CppVarType;

typedef struct Il2CppVariant Il2CppVariant;
typedef struct Il2CppIUnknown Il2CppIUnknown;

typedef struct Il2CppVariant
{
    union
    {
        struct __tagVARIANT
        {
            uint16_t type;
            uint16_t reserved1;
            uint16_t reserved2;
            uint16_t reserved3;
            union
            {
                int64_t llVal;
                int32_t lVal;
                uint8_t bVal;
                int16_t iVal;
                float fltVal;
                double dblVal;
                IL2CPP_VARIANT_BOOL boolVal;
                int32_t scode;
                int64_t cyVal;
                double date;
                Il2CppChar* bstrVal;
                Il2CppIUnknown* punkVal;
                void* pdispVal;
                Il2CppSafeArray* parray;
                uint8_t* pbVal;
                int16_t* piVal;
                int32_t* plVal;
                int64_t* pllVal;
                float* pfltVal;
                double* pdblVal;
                IL2CPP_VARIANT_BOOL* pboolVal;
                int32_t* pscode;
                int64_t* pcyVal;
                double* pdate;
                Il2CppChar* pbstrVal;
                Il2CppIUnknown** ppunkVal;
                void** ppdispVal;
                Il2CppSafeArray** pparray;
                Il2CppVariant* pvarVal;
                void* byref;
                char cVal;
                uint16_t uiVal;
                uint32_t ulVal;
                uint64_t ullVal;
                int intVal;
                unsigned int uintVal;
                Il2CppWin32Decimal* pdecVal;
                char* pcVal;
                uint16_t* puiVal;
                uint32_t* pulVal;
                uint64_t* pullVal;
                int* pintVal;
                unsigned int* puintVal;
                struct __tagBRECORD
                {
                    void* pvRecord;
                    void* pRecInfo;
                } n4;
            } n3;
        } n2;
        Il2CppWin32Decimal decVal;
    } n1;
} Il2CppVariant;

typedef struct Il2CppFileTime
{
    uint32_t low;
    uint32_t high;
} Il2CppFileTime;

typedef struct Il2CppStatStg
{
    Il2CppChar* name;
    uint32_t type;
    uint64_t size;
    Il2CppFileTime mtime;
    Il2CppFileTime ctime;
    Il2CppFileTime atime;
    uint32_t mode;
    uint32_t locks;
    Il2CppGuid clsid;
    uint32_t state;
    uint32_t reserved;
} Il2CppStatStg;

enum Il2CppWindowsRuntimeTypeKind
{
    kTypeKindPrimitive = 0,
    kTypeKindMetadata,
    kTypeKindCustom
};

struct Il2CppWindowsRuntimeTypeName
{
    Il2CppHString typeName;
    enum Il2CppWindowsRuntimeTypeKind typeKind;
};

#ifdef __cplusplus
struct LIBIL2CPP_CODEGEN_API NOVTABLE Il2CppIUnknown
{
    static const Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL QueryInterface(const Il2CppGuid& iid, void** object) = 0;
    virtual uint32_t STDCALL AddRef() = 0;
    virtual uint32_t STDCALL Release() = 0;
};

struct NOVTABLE Il2CppISequentialStream : Il2CppIUnknown
{
    static const LIBIL2CPP_CODEGEN_API Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL Read(void* buffer, uint32_t size, uint32_t* read) = 0;
    virtual il2cpp_hresult_t STDCALL Write(const void* buffer, uint32_t size, uint32_t* written) = 0;
};

struct NOVTABLE Il2CppIStream : Il2CppISequentialStream
{
    static const LIBIL2CPP_CODEGEN_API Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL Seek(int64_t move, uint32_t origin, uint64_t* position) = 0;
    virtual il2cpp_hresult_t STDCALL SetSize(uint64_t size) = 0;
    virtual il2cpp_hresult_t STDCALL CopyTo(Il2CppIStream* stream, uint64_t size, uint64_t* read, uint64_t* written) = 0;
    virtual il2cpp_hresult_t STDCALL Commit(uint32_t flags) = 0;
    virtual il2cpp_hresult_t STDCALL Revert() = 0;
    virtual il2cpp_hresult_t STDCALL LockRegion(uint64_t offset, uint64_t size, uint32_t type) = 0;
    virtual il2cpp_hresult_t STDCALL UnlockRegion(uint64_t offset, uint64_t size, uint32_t type) = 0;
    virtual il2cpp_hresult_t STDCALL Stat(Il2CppStatStg* data, uint32_t flags) = 0;
    virtual il2cpp_hresult_t STDCALL Clone(Il2CppIStream** stream) = 0;
};

struct LIBIL2CPP_CODEGEN_API NOVTABLE Il2CppIMarshal : Il2CppIUnknown
{
    static const Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL GetUnmarshalClass(const Il2CppGuid& iid, void* object, uint32_t context, void* reserved, uint32_t flags, Il2CppGuid* clsid) = 0;
    virtual il2cpp_hresult_t STDCALL GetMarshalSizeMax(const Il2CppGuid& iid, void* object, uint32_t context, void* reserved, uint32_t flags, uint32_t* size) = 0;
    virtual il2cpp_hresult_t STDCALL MarshalInterface(Il2CppIStream* stream, const Il2CppGuid& iid, void* object, uint32_t context, void* reserved, uint32_t flags) = 0;
    virtual il2cpp_hresult_t STDCALL UnmarshalInterface(Il2CppIStream* stream, const Il2CppGuid& iid, void** object) = 0;
    virtual il2cpp_hresult_t STDCALL ReleaseMarshalData(Il2CppIStream* stream) = 0;
    virtual il2cpp_hresult_t STDCALL DisconnectObject(uint32_t reserved) = 0;
};

struct NOVTABLE Il2CppIManagedObject : Il2CppIUnknown
{
    static const LIBIL2CPP_CODEGEN_API Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL GetSerializedBuffer(Il2CppChar** bstr) = 0;
    virtual il2cpp_hresult_t STDCALL GetObjectIdentity(Il2CppChar** bstr_guid, int32_t* app_domain_id, intptr_t* ccw) = 0;
};

struct LIBIL2CPP_CODEGEN_API NOVTABLE Il2CppIManagedObjectHolder : Il2CppIUnknown
{
    static const Il2CppGuid IID;
    virtual Il2CppObject* STDCALL GetManagedObject() = 0;
    virtual void STDCALL Destroy() = 0;
};

struct LIBIL2CPP_CODEGEN_API NOVTABLE Il2CppIInspectable : Il2CppIUnknown
{
    static const Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL GetIids(uint32_t* iidCount, Il2CppGuid** iids) = 0;
    virtual il2cpp_hresult_t STDCALL GetRuntimeClassName(Il2CppHString* className) = 0;
    virtual il2cpp_hresult_t STDCALL GetTrustLevel(int32_t* trustLevel) = 0;
};

struct NOVTABLE Il2CppIActivationFactory : Il2CppIInspectable
{
    static const LIBIL2CPP_CODEGEN_API Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL ActivateInstance(Il2CppIInspectable** instance) = 0;
};

struct NOVTABLE Il2CppIRestrictedErrorInfo : Il2CppIUnknown
{
    static const LIBIL2CPP_CODEGEN_API Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL GetErrorDetails(Il2CppChar** bstrDescription, il2cpp_hresult_t* error, Il2CppChar** bstrRestrictedDescription, Il2CppChar** bstrCapabilitySid) = 0;
    virtual il2cpp_hresult_t STDCALL GetReference(Il2CppChar** bstrReference) = 0;
};

struct NOVTABLE Il2CppILanguageExceptionErrorInfo : Il2CppIUnknown
{
    static const LIBIL2CPP_CODEGEN_API Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL GetLanguageException(Il2CppIUnknown** languageException) = 0;
};

struct NOVTABLE Il2CppIAgileObject : Il2CppIUnknown
{
    static const LIBIL2CPP_CODEGEN_API Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL GetLanguageException(Il2CppIUnknown** languageException) = 0;
};

struct NOVTABLE Il2CppIWeakReference : Il2CppIUnknown
{
    static const LIBIL2CPP_CODEGEN_API Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL Resolve(const Il2CppGuid& iid, Il2CppIInspectable** object) = 0;
};

struct NOVTABLE Il2CppIWeakReferenceSource : Il2CppIUnknown
{
    static const LIBIL2CPP_CODEGEN_API Il2CppGuid IID;
    virtual il2cpp_hresult_t STDCALL GetWeakReference(Il2CppIWeakReference** weakReference) = 0;
};

#endif //__cplusplus
