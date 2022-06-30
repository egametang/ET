#pragma once
#include <stdint.h>

#if defined(__cplusplus)
extern "C"
{
#endif

void* UnityPalSystemCertificatesOpenSystemRootStore();
int UnityPalSystemCertificatesEnumSystemCertificates(void* certStore, void** iter, int *format, int* size, void** data);
void UnityPalSystemCertificatesCloseSystemRootStore(void* cStore);

#if defined(__cplusplus)
}
#endif
