// Macro to put before functions that need to be exposed to C#
#ifdef DLL_EXPORTS
#define INTERPRETER_DLL _declspec(dllexport)
#else
#define INTERPRETER_DLL
#endif

#ifdef __cplusplus
extern "C" {
#endif
    INTERPRETER_DLL void interpreter_set_log(void(*writelog)(const char* buf, int len));

    INTERPRETER_DLL void interpreter_log(const char* str);

    INTERPRETER_DLL void interpreter_init(const char* bundleDir, const char* dllName);

#ifdef __cplusplus
}
#endif