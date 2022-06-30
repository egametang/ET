#pragma once

#if IL2CPP_TARGET_PS4
#define FILE_PATH_PREFIX "/app0/"
#else
#define FILE_PATH_PREFIX
#endif

#define CURRENT_DIRECTORY(filename) FILE_PATH_PREFIX filename
