#pragma once

// Use this attribute to disable the thread sanitizer checks for a specific method.

#if defined(__has_feature)
#if __has_feature(thread_sanitizer)
#define IL2CPP_DISABLE_TSAN __attribute__((no_sanitize("thread")))
#else
#define IL2CPP_DISABLE_TSAN
#endif
#else
#define IL2CPP_DISABLE_TSAN
#endif
