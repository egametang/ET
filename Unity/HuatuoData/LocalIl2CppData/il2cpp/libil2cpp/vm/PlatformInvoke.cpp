#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"

#include "PlatformInvoke.h"
#include "Exception.h"
#include "MetadataCache.h"
#include "Method.h"
#include "Object.h"
#include "Runtime.h"
#include "Type.h"

#include "gc/WriteBarrier.h"
#include "os/LibraryLoader.h"
#include "os/MarshalStringAlloc.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"
#include "vm-utils/VmStringUtils.h"

#include <stdint.h>
#include <algorithm>

namespace il2cpp
{
namespace vm
{
    void PlatformInvoke::SetFindPluginCallback(Il2CppSetFindPlugInCallback method)
    {
        os::LibraryLoader::SetFindPluginCallback(method);
    }

    Il2CppMethodPointer PlatformInvoke::Resolve(const PInvokeArguments& pinvokeArgs)
    {
        // Before resolving a P/Invoke, check against a hardcoded list of "known P/Invokes" that is different for every platform.
        // This bit solves several different problems we have when P/Invoking into native system libraries from mscorlib.dll.
        // Some platforms, like UWP, just don't allow you to load to load system libraries at runtime dynamically.
        // On other platforms (THEY SHALL NOT BE NAMED :O), while the functions that mscorlib.dll wants to P/Invoke into exist,
        // They exist in different system libraries than it is said in the DllImport attribute.
        Il2CppMethodPointer function = os::LibraryLoader::GetHardcodedPInvokeDependencyFunctionPointer(pinvokeArgs.moduleName, pinvokeArgs.entryPoint, pinvokeArgs.charSet);
        if (function != NULL)
            return function;

        Baselib_DynamicLibrary_Handle dynamicLibrary = Baselib_DynamicLibrary_Handle_Invalid;
        std::string detailedLoadError;
        if (utils::VmStringUtils::CaseSensitiveEquals(il2cpp::utils::StringUtils::NativeStringToUtf8(pinvokeArgs.moduleName.Str()).c_str(), "__InternalDynamic"))
            dynamicLibrary = os::LibraryLoader::LoadDynamicLibrary(il2cpp::utils::StringView<Il2CppNativeChar>::Empty(), detailedLoadError);
        else
            dynamicLibrary = os::LibraryLoader::LoadDynamicLibrary(pinvokeArgs.moduleName, detailedLoadError);

        if (dynamicLibrary == Baselib_DynamicLibrary_Handle_Invalid)
        {
            std::string message;
            message += "Unable to load DLL '";
            message += il2cpp::utils::StringUtils::NativeStringToUtf8(pinvokeArgs.moduleName.Str());
            message += "'. Tried the load the following dynamic libraries: ";
            message += detailedLoadError;
            Exception::Raise(Exception::GetDllNotFoundException(message.c_str()));
        }

        std::string detailedGetFunctionError;
        function = os::LibraryLoader::GetFunctionPointer(dynamicLibrary, pinvokeArgs, detailedGetFunctionError);
        if (function == NULL)
        {
            std::string message;
            message += "Unable to find an entry point named '";
            message += pinvokeArgs.entryPoint.Str();
            message += "' in '";
            message += il2cpp::utils::StringUtils::NativeStringToUtf8(pinvokeArgs.moduleName.Str());
            message += "'. Tried the following entry points: ";
            message += detailedGetFunctionError;
            Exception::Raise(Exception::GetEntryPointNotFoundException(message.c_str()));
        }

        return function;
    }

    void PlatformInvoke::MarshalFree(void* ptr)
    {
        if (ptr != NULL)
            MarshalAlloc::Free(ptr);
    }

    char* PlatformInvoke::MarshalCSharpStringToCppString(Il2CppString* managedString)
    {
        if (managedString == NULL)
            return NULL;

        std::string utf8String = utils::StringUtils::Utf16ToUtf8(managedString->chars);

        char* nativeString = MarshalAllocateStringBuffer<char>(utf8String.size() + 1);
        strcpy(nativeString, utf8String.c_str());

        return nativeString;
    }

    void PlatformInvoke::MarshalCSharpStringToCppStringFixed(Il2CppString* managedString, char* buffer, int numberOfCharacters)
    {
        if (managedString == NULL)
        {
            *buffer = '\0';
        }
        else
        {
            std::string utf8String = utils::StringUtils::Utf16ToUtf8(managedString->chars, numberOfCharacters - 1);
            strcpy(buffer, utf8String.c_str());
        }
    }

    Il2CppChar* PlatformInvoke::MarshalCSharpStringToCppWString(Il2CppString* managedString)
    {
        if (managedString == NULL)
            return NULL;

        int32_t stringLength = utils::StringUtils::GetLength(managedString);
        Il2CppChar* nativeString = MarshalAllocateStringBuffer<Il2CppChar>(stringLength + 1);
        for (int32_t i = 0; i < managedString->length; ++i)
            nativeString[i] = managedString->chars[i];

        nativeString[managedString->length] = '\0';

        return nativeString;
    }

    void PlatformInvoke::MarshalCSharpStringToCppWStringFixed(Il2CppString* managedString, Il2CppChar* buffer, int numberOfCharacters)
    {
        if (managedString == NULL)
        {
            *buffer = '\0';
        }
        else
        {
            int32_t stringLength = std::min(utils::StringUtils::GetLength(managedString), numberOfCharacters - 1);
            for (int32_t i = 0; i < stringLength; ++i)
                buffer[i] = managedString->chars[i];

            buffer[stringLength] = '\0';
        }
    }

    il2cpp_hresult_t PlatformInvoke::MarshalCSharpStringToCppBStringNoThrow(Il2CppString* managedString, Il2CppChar** bstr)
    {
        IL2CPP_ASSERT(bstr);

        if (managedString == NULL)
        {
            *bstr = NULL;
            return IL2CPP_S_OK;
        }

        int32_t stringLength = utils::StringUtils::GetLength(managedString);
        Il2CppChar* stringChars = utils::StringUtils::GetChars(managedString);
        return os::MarshalStringAlloc::AllocateBStringLength(stringChars, stringLength, bstr);
    }

    Il2CppChar* PlatformInvoke::MarshalCSharpStringToCppBString(Il2CppString* managedString)
    {
        Il2CppChar* bstr;
        const il2cpp_hresult_t hr = MarshalCSharpStringToCppBStringNoThrow(managedString, &bstr);
        vm::Exception::RaiseIfFailed(hr, true);
        return bstr;
    }

    Il2CppString* PlatformInvoke::MarshalCppStringToCSharpStringResult(const char* value)
    {
        if (value == NULL)
            return NULL;

        return String::New(value);
    }

    Il2CppString* PlatformInvoke::MarshalCppWStringToCSharpStringResult(const Il2CppChar* value)
    {
        if (value == NULL)
            return NULL;

        return String::NewUtf16(value, (int32_t)utils::StringUtils::StrLen(value));
    }

    Il2CppString* PlatformInvoke::MarshalCppBStringToCSharpStringResult(const Il2CppChar* value)
    {
        if (value == NULL)
            return NULL;

        int32_t length;
        const il2cpp_hresult_t hr = os::MarshalStringAlloc::GetBStringLength(value, &length);
        vm::Exception::RaiseIfFailed(hr, true);

        return String::NewUtf16(value, length);
    }

    void PlatformInvoke::MarshalFreeBString(Il2CppChar* value)
    {
        const il2cpp_hresult_t hr = os::MarshalStringAlloc::FreeBString(value);
        vm::Exception::RaiseIfFailed(hr, true);
    }

    char* PlatformInvoke::MarshalEmptyStringBuilder(Il2CppStringBuilder* stringBuilder, size_t& stringLength, std::vector<std::string>& utf8Chunks, std::vector<Il2CppStringBuilder*>& builders)
    {
        if (stringBuilder == NULL)
            return NULL;

        stringLength = 0;
        Il2CppStringBuilder* currentBuilder = stringBuilder;

        while (true)
        {
            if (currentBuilder == NULL)
                break;

            const Il2CppChar *str = (Il2CppChar*)il2cpp::vm::Array::GetFirstElementAddress(currentBuilder->chunkChars);
            std::string utf8String = utils::StringUtils::Utf16ToUtf8(str, (int)currentBuilder->chunkChars->max_length);

            utf8Chunks.push_back(utf8String);
            builders.push_back(currentBuilder);

            size_t lenToCount = std::max((size_t)currentBuilder->chunkChars->max_length, utf8String.size());

            stringLength += lenToCount;

            currentBuilder = currentBuilder->chunkPrevious;
        }

        char* nativeString = MarshalAllocateStringBuffer<char>(stringLength + 1);

        // We need to zero out the memory because the chunkChar array lengh may have been larger than the chunkLength
        // and when this happens we'll have a utf8String that is smaller than the the nativeString we allocated.  When we go to copy the
        // chunk utf8String into the nativeString it won't fill everything and we can end up with w/e junk value was in that memory before
        memset(nativeString, 0, sizeof(char) * (stringLength + 1));

        return nativeString;
    }

    char* PlatformInvoke::MarshalEmptyStringBuilder(Il2CppStringBuilder* stringBuilder)
    {
        size_t sizeLength;
        std::vector<std::string> utf8Chunks;
        std::vector<Il2CppStringBuilder*> builders;
        return MarshalEmptyStringBuilder(stringBuilder, sizeLength, utf8Chunks, builders);
    }

    char* PlatformInvoke::MarshalStringBuilder(Il2CppStringBuilder* stringBuilder)
    {
        if (stringBuilder == NULL)
            return NULL;

        size_t stringLength;
        std::vector<std::string> utf8Chunks;
        std::vector<Il2CppStringBuilder*> builders;
        char* nativeString = MarshalEmptyStringBuilder(stringBuilder, stringLength, utf8Chunks, builders);

        if (stringLength > 0)
        {
            int offsetAdjustment = 0;
            for (int i = (int)utf8Chunks.size() - 1; i >= 0; i--)
            {
                std::string utf8String = utf8Chunks[i];

                const char* utf8CString = utf8String.c_str();

                memcpy(nativeString + builders[i]->chunkOffset + offsetAdjustment, utf8CString, (int)utf8String.size());

                offsetAdjustment += (int)utf8String.size() - builders[i]->chunkLength;
            }
        }

        return nativeString;
    }

    Il2CppChar* PlatformInvoke::MarshalEmptyWStringBuilder(Il2CppStringBuilder* stringBuilder, size_t& stringLength)
    {
        if (stringBuilder == NULL)
            return NULL;

        stringLength = 0;
        Il2CppStringBuilder* currentBuilder = stringBuilder;
        while (true)
        {
            if (currentBuilder == NULL)
                break;

            stringLength += (size_t)currentBuilder->chunkChars->max_length;

            currentBuilder = currentBuilder->chunkPrevious;
        }

        return MarshalAllocateStringBuffer<Il2CppChar>(stringLength + 1);
    }

    Il2CppChar* PlatformInvoke::MarshalEmptyWStringBuilder(Il2CppStringBuilder* stringBuilder)
    {
        size_t stringLength;
        return MarshalEmptyWStringBuilder(stringBuilder, stringLength);
    }

    Il2CppChar* PlatformInvoke::MarshalWStringBuilder(Il2CppStringBuilder* stringBuilder)
    {
        if (stringBuilder == NULL)
            return NULL;

        size_t stringLength;
        Il2CppChar* nativeString = MarshalEmptyWStringBuilder(stringBuilder, stringLength);

        if (stringLength > 0)
        {
            Il2CppStringBuilder* currentBuilder = stringBuilder;
            while (true)
            {
                if (currentBuilder == NULL)
                    break;

                const Il2CppChar *str = (Il2CppChar*)il2cpp::vm::Array::GetFirstElementAddress(currentBuilder->chunkChars);

                memcpy(nativeString + currentBuilder->chunkOffset, str, (int)currentBuilder->chunkChars->max_length * sizeof(Il2CppChar));

                currentBuilder = currentBuilder->chunkPrevious;
            }
        }

        nativeString[stringLength] = '\0';

        return nativeString;
    }

    void PlatformInvoke::MarshalStringBuilderResult(Il2CppStringBuilder* stringBuilder, char* buffer)
    {
        if (stringBuilder == NULL || buffer == NULL)
            return;

        UTF16String utf16String = utils::StringUtils::Utf8ToUtf16(buffer);

        IL2CPP_OBJECT_SETREF(stringBuilder, chunkChars, il2cpp::vm::Array::New(il2cpp_defaults.char_class, (int)utf16String.size() + 1));

        for (int i = 0; i < (int)utf16String.size(); i++)
            il2cpp_array_set(stringBuilder->chunkChars, Il2CppChar, i, utf16String[i]);

        il2cpp_array_set(stringBuilder->chunkChars, Il2CppChar, (int)utf16String.size(), '\0');

        stringBuilder->chunkLength = (int)utf16String.size();
        stringBuilder->chunkOffset = 0;
        IL2CPP_OBJECT_SETREF_NULL(stringBuilder, chunkPrevious);
    }

    void PlatformInvoke::MarshalWStringBuilderResult(Il2CppStringBuilder* stringBuilder, Il2CppChar* buffer)
    {
        if (stringBuilder == NULL || buffer == NULL)
            return;

        int len = (int)utils::StringUtils::StrLen(buffer);

        IL2CPP_OBJECT_SETREF(stringBuilder, chunkChars, il2cpp::vm::Array::New(il2cpp_defaults.char_class, len + 1));

        for (int i = 0; i < len; i++)
            il2cpp_array_set(stringBuilder->chunkChars, Il2CppChar, i, buffer[i]);

        il2cpp_array_set(stringBuilder->chunkChars, Il2CppChar, len, '\0');

        stringBuilder->chunkLength = len;
        stringBuilder->chunkOffset = 0;
        IL2CPP_OBJECT_SETREF_NULL(stringBuilder, chunkPrevious);
    }

    static bool IsGenericInstance(const Il2CppType* type)
    {
        if (type->type == IL2CPP_TYPE_GENERICINST)
            return true;

        while (type->type == IL2CPP_TYPE_SZARRAY)
        {
            if (type->data.type->type == IL2CPP_TYPE_GENERICINST)
                return true;

            type = type->data.type;
        }

        return false;
    }

    static std::string TypeNameListFor(const Il2CppGenericInst* genericInst)
    {
        std::string typeNameList;
        if (genericInst != NULL)
        {
            int numberOfTypeArguments = genericInst->type_argc;
            for (int i = 0; i < numberOfTypeArguments; i++)
            {
                typeNameList += Type::GetName(genericInst->type_argv[i], IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
                if (i != numberOfTypeArguments - 1)
                    typeNameList += ", ";
            }
        }

        return typeNameList;
    }

#if !IL2CPP_TINY
    intptr_t PlatformInvoke::MarshalDelegate(Il2CppDelegate* d)
    {
        if (d == NULL)
            return 0;

        if (IsFakeDelegateMethodMarshaledFromNativeCode(d))
            return reinterpret_cast<intptr_t>(d->delegate_trampoline);

        Il2CppMethodPointer reversePInvokeWrapper = MetadataCache::GetReversePInvokeWrapper(d->method->klass->image, d->method);
        if (reversePInvokeWrapper == NULL)
        {
            std::string methodName = il2cpp::vm::Method::GetFullName(d->method);
            // Okay, we cannot marshal it for some reason. Figure out why.
            if (Method::IsInstance(d->method))
            {
                std::string errorMessage = "IL2CPP does not support marshaling delegates that point to instance methods to native code. The method we're attempting to marshal is: " + methodName;
                vm::Exception::Raise(vm::Exception::GetNotSupportedException(errorMessage.c_str()));
            }

            if (il2cpp::vm::Method::HasFullGenericSharingSignature(d->method))
            {
                std::string errorMessage = "IL2CPP does not support marshaling generic delegates when full generic sharing is enabled. The method we're attempting to marshal is: " + methodName;
                errorMessage += "\nTo marshal this delegate, please add an attribute named 'MonoPInvokeCallback' to the method definition.";
                errorMessage += "\nThis attribute should have a type argument which is a generic delegate with all of the types required for this generic instantiation:";

                std::string genericTypeArguments = TypeNameListFor(d->method->genericMethod->context.class_inst);
                if (!genericTypeArguments.empty())
                    errorMessage += "\nGeneric type arguments: " + genericTypeArguments;

                std::string genericMethodArguments = TypeNameListFor(d->method->genericMethod->context.method_inst);
                if (!genericMethodArguments.empty())
                    errorMessage += "\nGeneric method arguments: " + genericMethodArguments;

                errorMessage += "\nThis C# code should work, for example:";
                std::string maybeComma = !genericTypeArguments.empty() ? ", " : "";
                errorMessage += "\n[MonoPInvokeCallback(typeof(System.Action<" + genericTypeArguments + maybeComma + genericMethodArguments + ">))]";

                vm::Exception::Raise(vm::Exception::GetNotSupportedException(errorMessage.c_str()));
            }

            if (d->method->parameters != NULL)
            {
                for (int i = 0; i < d->method->parameters_count; ++i)
                {
                    if (IsGenericInstance(d->method->parameters[i]))
                    {
                        std::string errorMessage = "Cannot marshal method '" + methodName + "' parameter '" + Method::GetParamName(d->method, i) + "': Generic types cannot be marshaled.";
                        vm::Exception::Raise(vm::Exception::GetMarshalDirectiveException(errorMessage.c_str()));
                    }
                }
            }

            std::string errorMessage = "To marshal a managed method, please add an attribute named 'MonoPInvokeCallback' to the method definition. The method we're attempting to marshal is: " + methodName;
            vm::Exception::Raise(vm::Exception::GetNotSupportedException(errorMessage.c_str()));
        }

        return reinterpret_cast<intptr_t>(reversePInvokeWrapper);
    }

    Il2CppDelegate* PlatformInvoke::MarshalFunctionPointerToDelegate(void* functionPtr, Il2CppClass* delegateType)
    {
        if (!Class::HasParent(delegateType, il2cpp_defaults.delegate_class))
            Exception::Raise(Exception::GetArgumentException("t", "Type must derive from Delegate."));

        const Il2CppInteropData* interopData = delegateType->interopData;
        Il2CppMethodPointer managedToNativeWrapperMethodPointer = interopData != NULL ? interopData->delegatePInvokeWrapperFunction : NULL;

        if (managedToNativeWrapperMethodPointer == NULL)
            Exception::Raise(Exception::GetMarshalDirectiveException(utils::StringUtils::Printf("Cannot marshal P/Invoke call through delegate of type '%s.%s'", Class::GetNamespace(delegateType), Class::GetName(delegateType)).c_str()));

        const MethodInfo* invokeMethod = il2cpp::vm::Runtime::GetDelegateInvoke(delegateType);
        Il2CppDelegate* delegate = (Il2CppDelegate*)il2cpp::vm::Object::New(delegateType);
        Type::ConstructClosedDelegate(delegate, (Il2CppObject*)delegate, managedToNativeWrapperMethodPointer, invokeMethod);
        delegate->delegate_trampoline = functionPtr;

        return delegate;
    }

    // When a delegate is marshalled from native code via Marshal.GetDelegateForFunctionPointer
    // It will store the native function pointer in delegate_trampoline
    bool PlatformInvoke::IsFakeDelegateMethodMarshaledFromNativeCode(const Il2CppDelegate* d)
    {
        return d->delegate_trampoline != NULL;
    }

#endif // !IL2CPP_TINY
} /* namespace vm */
} /* namespace il2cpp */
