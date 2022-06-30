#include "il2cpp-config.h"

#if IL2CPP_TARGET_ANDROID

#include <string>

#include "os/Initialize.h"
#include "os/LibraryLoader.h"
#include "utils/Logging.h"

#include <jni.h>
#include <android/log.h>

static void AndroidLogCallback(const char* message)
{
    __android_log_print(ANDROID_LOG_INFO, "IL2CPP", "%s", message);
}

JavaVM *sJavaVM = nullptr;

static const Il2CppNativeChar* AndroidLoadLibrary(const Il2CppNativeChar* libName)
{
    if (sJavaVM == nullptr)
    {
        __android_log_print(ANDROID_LOG_INFO, "IL2CPP", "Java VM not initialized");
        return libName;
    }

    if (libName != nullptr)
    {
        JNIEnv* env = nullptr;
        bool detached = sJavaVM->GetEnv((void**)&env, JNI_VERSION_1_2) == JNI_EDETACHED;
        if (detached)
        {
            sJavaVM->AttachCurrentThread(&env, NULL);
        }
        env->ExceptionClear();

        jclass systemClass = env->FindClass("java/lang/System");
        if (systemClass != nullptr)
        {
            jmethodID loadLibrary = env->GetStaticMethodID(systemClass, "loadLibrary", "(Ljava/lang/String;)V");
            if (loadLibrary != nullptr)
            {
                jstring jstr = env->NewStringUTF(libName);
                env->CallStaticVoidMethod(systemClass, loadLibrary, jstr);
                if (env->ExceptionCheck())
                {
                    env->ExceptionClear();

                    // try without lib prefix
                    if (std::string(libName).find("lib") == 0)
                    {
                        jstr = env->NewStringUTF(libName + 3);
                        env->CallStaticVoidMethod(systemClass, loadLibrary, jstr);
                    }
                }
            }
        }
        if (env->ExceptionCheck())
        {
            env->ExceptionClear();
        }
        if (detached)
        {
            sJavaVM->DetachCurrentThread();
        }
    }

    return libName;
}

extern "C"
JNIEXPORT jint JNI_OnLoad(JavaVM *jvm, void *reserved)
{
    __android_log_print(ANDROID_LOG_INFO, "IL2CPP", "JNI_OnLoad");
    sJavaVM = jvm;
    il2cpp::os::LibraryLoader::SetFindPluginCallback(AndroidLoadLibrary);
    return JNI_VERSION_1_6;
}

extern "C"
JNIEXPORT void JNI_OnUnload(JavaVM *jvm, void *reserved)
{
    __android_log_print(ANDROID_LOG_INFO, "IL2CPP", "JNI_OnUnload");
    sJavaVM = nullptr;
}

void il2cpp::os::Initialize()
{
    if (!utils::Logging::IsLogCallbackSet())
        utils::Logging::SetLogCallback(AndroidLogCallback);
}

void il2cpp::os::Uninitialize()
{
}

#endif
