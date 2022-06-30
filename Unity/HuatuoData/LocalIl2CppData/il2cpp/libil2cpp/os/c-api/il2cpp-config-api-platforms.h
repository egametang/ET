#pragma once

#include "os/c-api/il2cpp-config-platforms.h"
#if !defined(IL2CPP_EXPORT)
#ifdef _MSC_VER
# include <malloc.h>
# define IL2CPP_EXPORT __declspec(dllexport)
# define IL2CPP_IMPORT __declspec(dllimport)
#elif IL2CPP_TARGET_PSP2 || IL2CPP_TARGET_PS4
# define IL2CPP_EXPORT __declspec(dllexport)
# define IL2CPP_IMPORT __declspec(dllimport)
#else
# define IL2CPP_EXPORT __attribute__ ((visibility ("default")))
# define IL2CPP_IMPORT
#endif
#endif
