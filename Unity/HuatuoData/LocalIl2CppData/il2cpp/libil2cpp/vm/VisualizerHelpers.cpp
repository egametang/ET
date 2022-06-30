#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"

#include "vm/GlobalMetadataFileInternals.h"

#if _MSC_VER

// These structs aren't actually used anywhere, but they are used by the VS debugger visualizer to help visualize various types
namespace VisualizerHelpers
{
    struct Il2CppRawTypeName
    {
        Il2CppClass t;
    };

    struct Il2CppRawTypeNameWithoutDeclaringType
    {
        Il2CppClass t;
    };

    struct Il2CppGenericParameters_DeclaringTypeHas0Parameters
    {
        Il2CppGenericContainer c;
    };

    struct Il2CppGenericParameters_DeclaringTypeHas1Parameters
    {
        Il2CppGenericContainer c;
    };

    struct Il2CppGenericParameters_DeclaringTypeHas2Parameters
    {
        Il2CppGenericContainer c;
    };

    struct Il2CppGenericParameters_DeclaringTypeHas3Parameters
    {
        Il2CppGenericContainer c;
    };

    struct Il2CppGenericParameters_DeclaringTypeHas4Parameters
    {
        Il2CppGenericContainer c;
    };

    struct Il2CppGenericParameters_DeclaringTypeHas5Parameters
    {
        Il2CppGenericContainer c;
    };

    struct Il2CppGenericParameters
    {
        Il2CppGenericContainer c;
    };

    struct Il2CppGenericArguments_DeclaringTypeHas0Arguments
    {
        Il2CppGenericInst gi;
    };

    struct Il2CppGenericArguments_DeclaringTypeHas1Arguments
    {
        Il2CppGenericInst gi;
    };

    struct Il2CppGenericArguments_DeclaringTypeHas2Arguments
    {
        Il2CppGenericInst gi;
    };

    struct Il2CppGenericArguments_DeclaringTypeHas3Arguments
    {
        Il2CppGenericInst gi;
    };

    struct Il2CppGenericArguments_DeclaringTypeHas4Arguments
    {
        Il2CppGenericInst gi;
    };

    struct Il2CppGenericArguments_DeclaringTypeHas5Arguments
    {
        Il2CppGenericInst gi;
    };

    struct Il2CppGenericArguments
    {
        Il2CppGenericClass gi;
    };

    struct Il2CppMethodParameters
    {
        MethodInfo m;
    };

    struct Il2CppMethodGenericParameters
    {
        MethodInfo m;
    };

    struct PreventLinkerFromStrippingTypesFromDebugInfo
    {
        Il2CppRawTypeName* ___rawTypeName;
        Il2CppRawTypeNameWithoutDeclaringType* ___rawTypeNameWithoutDeclaringType;
        Il2CppGenericParameters_DeclaringTypeHas0Parameters* ___genericParameters0;
        Il2CppGenericParameters_DeclaringTypeHas1Parameters* ___genericParameters1;
        Il2CppGenericParameters_DeclaringTypeHas2Parameters* ___genericParameters2;
        Il2CppGenericParameters_DeclaringTypeHas3Parameters* ___genericParameters3;
        Il2CppGenericParameters_DeclaringTypeHas4Parameters* ___genericParameters4;
        Il2CppGenericParameters_DeclaringTypeHas5Parameters* ___genericParameters5;
        Il2CppGenericParameters* ___genericParameters;
        Il2CppGenericArguments_DeclaringTypeHas0Arguments* ___genericArguments0;
        Il2CppGenericArguments_DeclaringTypeHas1Arguments* ___genericArguments1;
        Il2CppGenericArguments_DeclaringTypeHas2Arguments* ___genericArguments2;
        Il2CppGenericArguments_DeclaringTypeHas3Arguments* ___genericArguments3;
        Il2CppGenericArguments_DeclaringTypeHas4Arguments* ___genericArguments4;
        Il2CppGenericArguments_DeclaringTypeHas5Arguments* ___genericArguments5;
        Il2CppGenericArguments* ___genericArguments;
        Il2CppMethodParameters* ___methodParameters;
        Il2CppMethodGenericParameters* ___methodGenericParameters;
    };
}

// We need to declare a global variable, otherwise compiler strips all type information from PDBs, and in result debugger can't visualize them
extern "C" VisualizerHelpers::PreventLinkerFromStrippingTypesFromDebugInfo * ____visualizerHelpersPreventLinkerStripping = NULL;

#endif
