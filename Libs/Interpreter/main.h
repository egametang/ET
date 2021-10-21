#pragma once

// Macro to put before functions that need to be exposed to C#
#ifdef _WIN32
#define DLLEXPORT __declspec(dllexport)
#else
#define DLLEXPORT 
#endif

DLLEXPORT void InterpreterInit(char* bundleDir, const char* dllName);
