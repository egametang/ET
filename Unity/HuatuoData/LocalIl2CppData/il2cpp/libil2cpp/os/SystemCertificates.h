#pragma once

#include <stdint.h>

namespace il2cpp
{
namespace os
{
    typedef enum
    {
        DATATYPE_STRING = 0,
        DATATYPE_INTPTR = 1,
        DATATYPE_FILE = 2
    } CertDataFormat;

    typedef struct
    {
        void* certdata;
        int certsize;
    } CertObj;

    class SystemCertificates
    {
    public:
        static void* OpenSystemRootStore();
        static int EnumSystemCertificates(void* certStore, void** iter, int *format, int* size, void** data);
        static void CloseSystemRootStore(void* cStore);
    };
}
}
