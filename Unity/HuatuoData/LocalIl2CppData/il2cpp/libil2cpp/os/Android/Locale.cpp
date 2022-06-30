#include "il2cpp-config.h"

#if IL2CPP_TARGET_ANDROID

#include <string>
#include <android/log.h>
#include <jni.h>

extern JavaVM *sJavaVM;

namespace il2cpp
{
namespace os
{
    std::string AndroidGetLocale()
    {
        std::string locale;
        if (sJavaVM == nullptr)
        {
            __android_log_print(ANDROID_LOG_INFO, "IL2CPP", "Java VM not initialized");
            return locale;
        }
        JNIEnv* env = nullptr;
        bool detached = sJavaVM->GetEnv((void**)&env, JNI_VERSION_1_2) == JNI_EDETACHED;
        if (detached)
        {
            sJavaVM->AttachCurrentThread(&env, NULL);
        }

        jclass localeClass = env->FindClass("java/util/Locale");
        if (localeClass != nullptr)
        {
            jmethodID getDefault = env->GetStaticMethodID(localeClass, "getDefault", "()Ljava/util/Locale;");
            if (getDefault != nullptr)
            {
                jobject def = env->CallStaticObjectMethod(localeClass, getDefault);
                jmethodID toLanguageTag = env->GetMethodID(localeClass, "toLanguageTag", "()Ljava/lang/String;");
                // toLanguageTag is available since API 21 only, so returning default locale for Android 4.4
                if (toLanguageTag != nullptr)
                {
                    jstring tag = (jstring)env->CallObjectMethod(def, toLanguageTag);
                    const char *nativeTag = env->GetStringUTFChars(tag, nullptr);
                    __android_log_print(ANDROID_LOG_INFO, "IL2CPP", "Locale %s", nativeTag);
                    locale = nativeTag;
                    env->ReleaseStringUTFChars(tag, nativeTag);
                }
            }
        }
        if (env->ExceptionCheck())
        {
            env->ExceptionClear();
        }

        if (detached)
            sJavaVM->DetachCurrentThread();
        return locale;
    }
} /* namespace os */
} /* namespace il2cpp */

#endif
