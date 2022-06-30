// DO NOT PUT #pragma once or include guard check here
// This header is designed to be able to be included multiple times

// -------------------------------------------------------------------------------------------------
// this macros are undefined in UndefineCoreMacros.h

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!! IF YOU ADD A NEW MACRO TO THIS SECTION !!!
// !!! please add it to UndefineCoreMacros.h  !!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

// this is where we collect context-free macros of general utility. it's a holding area until the new sub-core layer
// project is started.
//
// IMPORTANT: only macros! and no non-system #includes!

// FORCE_INLINE forwarded to compiler defined macro
#define FORCE_INLINE COMPILER_FORCEINLINE

// You may use OPTIMIZER_LIKELY / OPTIMIZER_UNLIKELY to provide the compiler with branch prediction information.
//
// The return value is the value of 'EXPR_', which should be an integral expression.
//
// OPTIMIZER_LIKELY makes it so that the branch predictor chooses to take the branch.
// OPTIMIZER_UNLIKELY makes it so that the branch predictor chooses not to take the branch.
//
#define OPTIMIZER_LIKELY(EXPR_)     COMPILER_BUILTIN_EXPECT(!!(EXPR_), 1)
#define OPTIMIZER_UNLIKELY(EXPR_)   COMPILER_BUILTIN_EXPECT(!!(EXPR_), 0)

// UNUSED will tell the compiler not to warn about a given variable being unused. "yeah, we know - this is unused."
//
// the internet says that (void)sizeof(expr) is the right way to do this, but not for us, not with our
// compilers. the below is the result of much experimentation by @lucas, who says that we have at least one compiler
// that does not consider sizeof(expr) to be a 'usage' of the variable(s) inside of expr.
//
// also note that we do not have the 'if+const expr' warning enabled because combining #if and if expression/constants
// (which we often need to do - for example 'caps->gles.requireClearAlpha = PLATFORM_WEBGL || PLATFORM_STV') is super noisy.
//
#define UNUSED(EXPR_) \
    do { if (false) (void)(EXPR_); } while(0)

// COMPILER_WARNING will generate a compiler warning. this will work for all our compilers, though note the usage
// requires a pragma. (based on http://goodliffe.blogspot.dk/2009/07/c-how-to-say-warning-to-visual-studio-c.html)
//
// usage:
//
//   #pragma COMPILER_WARNING("this file is obsolete! use foo/bar.h instead.")
//
#define COMPILER_WARNING(MESSAGE_) message(__FILE__ "(" UNITY_STRINGIFY(__LINE__) ") : warning: " MESSAGE_)

#define UNSIGNED_FLAGS_1(FLAG1_) static_cast<unsigned int>(FLAG1_)
#define UNSIGNED_FLAGS_2(FLAG1_, FLAG2_) UNSIGNED_FLAGS_1(FLAG1_) | UNSIGNED_FLAGS_1(FLAG2_)
#define UNSIGNED_FLAGS_3(FLAG1_, FLAG2_, FLAG3_) UNSIGNED_FLAGS_1(FLAG1_) | UNSIGNED_FLAGS_2(FLAG2_, FLAG3_)
#define UNSIGNED_FLAGS_4(FLAG1_, FLAG2_, FLAG3_, FLAG4_) UNSIGNED_FLAGS_1(FLAG1_) | UNSIGNED_FLAGS_3(FLAG2_, FLAG3_, FLAG4_)
#define UNSIGNED_FLAGS_5(FLAG1_, FLAG2_, FLAG3_, FLAG4_, FLAG5_) UNSIGNED_FLAGS_1(FLAG1_) | UNSIGNED_FLAGS_4(FLAG2_, FLAG3_, FLAG4_, FLAG5_)
#define UNSIGNED_FLAGS_6(FLAG1_, FLAG2_, FLAG3_, FLAG4_, FLAG5_, FLAG6_) UNSIGNED_FLAGS_1(FLAG1_) | UNSIGNED_FLAGS_5(FLAG2_, FLAG3_, FLAG4_, FLAG5_, FLAG6_)
#define UNSIGNED_FLAGS(...) PP_VARG_SELECT_OVERLOAD(UNSIGNED_FLAGS_, (__VA_ARGS__))

// -------------------------------------------------------------------------------------------------
// this macros are not undefined in UndefineCoreMacros.h, hence we put a guard to not define them twice
#ifndef DETAIL__PP_AND_DETAILS_CORE_MACROS_DEFINED
#define DETAIL__PP_AND_DETAILS_CORE_MACROS_DEFINED

// when putting control-flow, multiple statements, or unknown code (e.g. passed via an outer macro) inside of a macro,
// wrap it in PP_WRAP_CODE to be safe. https://q.unity3d.com/answers/1382/view.html
//
// (also see http://stackoverflow.com/questions/154136/do-while-and-if-else-statements-in-c-c-macros)
//
// things not to use PP_WRAP_CODE for:
//
//   * 'break' or 'continue' statements that are expected to operate on the scope containing the macro
//   * introduction of variables that are expected not to go out of scope at macro end
//
#define PP_WRAP_CODE(CODE_) \
    do { CODE_; } while (0)

// PP_EMPTY_STATEMENT is used to insert an empty statement in a macro to require a semicolon terminator where used.
// most useful when creating "function style" macros where there is no natural place inside the macro to leave off a
// semicolon so as to require it in usage (for example when the internals end with a closing brace).
//
#define PP_EMPTY_STATEMENT \
    do { } while (0)

// PP_VARG_COUNT expands to the the number of arguments passed to the macro. It supports 1 to 20 arguments (0 is not
// supported)
//
#define PP_VARG_COUNT(...) \
    DETAIL__PP_EXPAND_2(DETAIL__PP_VARG_COUNT, (__VA_ARGS__, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1))

// PP_VARG_SELECT_OVERLOAD calls the correct overloaded version of the macro "name" name1, name2 etc.
//
// WARNING: **Varargs break Intellisense**. Intellisense gives us the argument list, which most of the time should be
// sufficient documentation for using a macro. Macro overloading hides the args and possibly makes it less safe as
// well. So be certain this tradeoff is worth it!
//
// Use like this:
//
//   #define FORWARD_DECLARE_CLASS_1(CLASSNAME_) class CLASSNAME_;
//   #define FORWARD_DECLARE_CLASS_2(NAMESPACE_, CLASSNAME_) namespace NAMESPACE_ { class CLASSNAME_; }
//   #define FORWARD_DECLARE_CLASS_3(NAMESPACE_1_, NAMESPACE_2_, CLASSNAME_) namespace NAMESPACE_1_ { namespace NAMESPACE_1_ { class CLASSNAME_; } }
//   // Up to 10 overloads can be added
//   #define FORWARD_DECLARE_CLASS(...) PP_VARG_SELECT_OVERLOAD(FORWARD_DECLARE_CLASS_, (__VA_ARGS__))
//
// ...which can then be used with optional number of arguments like this:
//
//   FORWARD_DECLARE_CLASS(GlobalClass)
//   FORWARD_DECLARE_CLASS(FooNamespace, FooClass)
//   FORWARD_DECLARE_CLASS(FooNamespace, BarNamespace, FooBarClass)
//
#define PP_VARG_SELECT_OVERLOAD(NAME_, ARGS_) \
    DETAIL__PP_EXPAND_2(DETAIL__PP_VARG_CONCAT(NAME_, PP_VARG_COUNT ARGS_), ARGS_)

// PP_CONCAT concatenates all passed preprocessor tokens after macro-expanding them
#define PP_CONCAT(...) PP_VARG_SELECT_OVERLOAD(DETAIL__PP_CONCAT_, (__VA_ARGS__))

// PP_NOOP does nothing, but is useful for forcing the preprocessor to re-evaluate expressions after expansion.
#define PP_NOOP()

// PP_DEFER defers evaluation of EXPR_ to the next expansion pass.
#define PP_DEFER(EXPR_) EXPR_ PP_NOOP ()

// PP_DEFER2 defers evaluation of EXPR_ to the expansion pass *after* the next one
#define PP_DEFER2(EXPR_) EXPR_ PP_NOOP PP_NOOP ()()

// PP_DEFER3 defers evaluation of EXP_ to the expansion pass *after* the expansion pass after the next one
#define PP_DEFER3(EXPR_) EXPR_ PP_NOOP PP_NOOP PP_NOOP ()()()

// PP_RECURSE allows recursive expansion of macros; PP_RECURSE(A) will expand to A_RECURSE which you should define to A.
#define PP_RECURSE(MACRO_) PP_DEFER(MACRO_##_RECURSE)()

// Use PP_EVAL to force up to 1024 evaluation passes on an expression, ensuring that everything is fully expanded.
#define PP_EVAL(...) DETAIL__PP_EVAL1024(__VA_ARGS__)

// Use PP_STRINGIZE to wrap the precise characters in the given argument in double quotes (auto-escaping where necessary).
// This is most often used to convert an expression to a string, but be aware that the contents aren't limited to what a
// C expression permits! For example PP_STRINGIZE(pork & beans ("awesome")!) results in the literal "pork & beans (\"awesome\")!"
#define PP_STRINGIZE(ARG_) DETAIL__PP_STRINGIZE_EXPAND(ARG_)

// PP_FIRST expands to the first argument in a list of arguments
#define PP_FIRST(A_, ...) A_

// PP_SECOND expands to the second argument in a list of (at least two) arguments
#define PP_SECOND(A_, B_, ...) B_

// PP_BOOLIFY expands to 0 if the argument is 0, and 1 otherwise
// It should be used inside a PP_EVAL expression.
#define PP_BOOLIFY(EXPR_) DETAIL__PP_BOOLIFY_NOT(DETAIL__PP_BOOLIFY_NOT(EXPR_))

// PP_VARG_IS_NONEMPTY evaluates to 0 if no arguments are provided, and 1 otherwise
// It should be used inside a PP_EVAL expression.
#if COMPILER_MSVC
    #define PP_VARG_IS_NONEMPTY(...) PP_BOOLIFY(PP_FIRST(__VA_ARGS__ DETAIL__PP_VARG_END_MARKER)())
#else
    #define PP_VARG_IS_NONEMPTY(...) PP_BOOLIFY(PP_DEFER(PP_FIRST)(DETAIL__PP_VARG_END_MARKER DETAIL__PP_VARG_UNPAREN_FIRST(__VA_ARGS__))())
#endif

// PP_IF_ELSE(EXPR_)(A_)(B_) evaluates to A_ if EXPR_ is nonzero, or to B_ if EXPR_ is 0.
// It should be used inside a PP_EVAL expression.
#define PP_IF_ELSE(EXPR_) DETAIL__PP_IF_ELSE(PP_BOOLIFY(EXPR_))

// PP_MAP applies MACRO_ to each of the following arguments.
// It should be used inside a PP_EVAL expression.
#define PP_MAP(MACRO_, ...) PP_EVAL(PP_IF_ELSE(PP_VARG_IS_NONEMPTY(__VA_ARGS__))(PP_DEFER3(DETAIL__PP_MAP_NONOPTIONAL)(MACRO_, __VA_ARGS__))())

// PP_UNPAREN removes one set of optional parenthesis around the argument.
// Useful for implementing macros that take types as argument since the commas in templated types
// normally are seen as macro argument separators.
// #define ARRAY(NAME_, TYPE_, COUNT_) PP_UNPAREN(TYPE_) NAME_[COUNT_]
// The type passed to the ARRAY macro ARRAY macro can now be used like:
//     ARRAY(array_of_maps, (map<int,int>), 8);
// while still accepting:
//     ARRAY(array_of_ints, int, 8);
#define PP_UNPAREN(EXPR_) DETAIL__PP_UNPAREN_EVAL_AND_CONCAT_FIRST_2_ARGS(DETAIL__PP_UNPAREN_EMPTY_, DETAIL__PP_UNPAREN_HELPER EXPR_)

#if __cplusplus

// PP_IS_STRING evaluates to true if the argument is of const char* type, false otherwise
#define PP_IS_STRING(EXPR_) (sizeof(core::detail::ReturnCharIfString(EXPR_)) == sizeof(char))

// PP_CONST_VALUE takes a value that _may_ be an int, or may be a string, and produces a constant expression suitable for use as an enum
// initializer. If you pass it a string, the result will still be a constant expression, but it will have undefined value.
#define PP_CONST_VALUE(EXPR_) ((int)DETAIL__PP_CONST_VALUE_HIGHBITS(EXPR_) + (int)DETAIL__PP_CONST_VALUE_LOWBITS(EXPR_))

#endif

// ----------------------------------------------------------------------------

// implementation details for above macros follow. we have this in a separate section to cut down on clutter above.

// visual c++ requires two levels of indirection to ensure proper macro expansion of PP_CONCAT with certain arguments involving __VA_ARGS__
// and other scenarios like the one below.
// #define PP_CAT(a,b) a##b
// #define PP_CAT2(a,b) PP_CAT(a,b)
// #define PP_CAT3(a,b) PP_CAT2(a,b)
//
// #define E(a) QQ a
// #define QQ() Q
//
// #define T2() PP_CAT2(_,E(()))
// #define T3() PP_CAT3(_,E(()))
//
// T2() and T3() will expand differently with VC but not with other preprocessors.
#define DETAIL__PP_CONCAT_Y(A_, B_) A_##B_
#define DETAIL__PP_CONCAT_X(A_, B_) DETAIL__PP_CONCAT_Y(A_, B_)

#define DETAIL__PP_EXPAND_2(A_, B_) A_ B_


#define DETAIL__PP_VARG_CONCAT_Y(A_, B_) A_##B_
#define DETAIL__PP_VARG_CONCAT_X(A_, B_) DETAIL__PP_VARG_CONCAT_Y(A_, B_)
#define DETAIL__PP_VARG_CONCAT(A_, B_) DETAIL__PP_VARG_CONCAT_X(A_, B_)
#define DETAIL__PP_VARG_COUNT(ARG0_, ARG1_, ARG2_, ARG3_, ARG4_, ARG5_, ARG6_, ARG7_, ARG8_, ARG9_, ARG10_, ARG11_, ARG12_, ARG13_, ARG14_, ARG15_, ARG16_, ARG17_, ARG18_, ARG19_, RESULT_, ...) RESULT_

#define DETAIL__PP_CONCAT_1(A_) DETAIL__PP_CONCAT_X(A_,)
#define DETAIL__PP_CONCAT_2(A_, B_) DETAIL__PP_CONCAT_X(A_, B_)
#define DETAIL__PP_CONCAT_3(A_, B_, C_) DETAIL__PP_CONCAT_2(DETAIL__PP_CONCAT_2(A_, B_), C_)
#define DETAIL__PP_CONCAT_4(A_, B_, C_, D_) DETAIL__PP_CONCAT_2(DETAIL__PP_CONCAT_2(A_, B_), DETAIL__PP_CONCAT_2(C_, D_))
#define DETAIL__PP_CONCAT_5(A_, B_, C_, D_, E_) DETAIL__PP_CONCAT_2(DETAIL__PP_CONCAT_2(A_, B_), DETAIL__PP_CONCAT_3(C_, D_, E_))
#define DETAIL__PP_CONCAT_6(A_, B_, C_, D_, E_, F_) DETAIL__PP_CONCAT_2(DETAIL__PP_CONCAT_2(A_, B_), DETAIL__PP_CONCAT_4(C_, D_, E_, F_))
#define DETAIL__PP_CONCAT_7(A_, B_, C_, D_, E_, F_, G_) DETAIL__PP_CONCAT_2(DETAIL__PP_CONCAT_3(A_, B_, C_), DETAIL__PP_CONCAT_4(D_, E_, F_, G_))
#define DETAIL__PP_CONCAT_8(A_, B_, C_, D_, E_, F_, G_, H_) DETAIL__PP_CONCAT_2(DETAIL__PP_CONCAT_4(A_, B_, C_, D_), DETAIL__PP_CONCAT_4(E_, F_, G_, H_))

#define DETAIL__PP_EVAL1024(...) DETAIL__PP_EVAL512(DETAIL__PP_EVAL512(__VA_ARGS__))
#define DETAIL__PP_EVAL512(...) DETAIL__PP_EVAL256(DETAIL__PP_EVAL256(__VA_ARGS__))
#define DETAIL__PP_EVAL256(...) DETAIL__PP_EVAL128(DETAIL__PP_EVAL128(__VA_ARGS__))
#define DETAIL__PP_EVAL128(...) DETAIL__PP_EVAL64(DETAIL__PP_EVAL64(__VA_ARGS__))
#define DETAIL__PP_EVAL64(...) DETAIL__PP_EVAL32(DETAIL__PP_EVAL32(__VA_ARGS__))
#define DETAIL__PP_EVAL32(...) DETAIL__PP_EVAL16(DETAIL__PP_EVAL16(__VA_ARGS__))
#define DETAIL__PP_EVAL16(...) DETAIL__PP_EVAL8(DETAIL__PP_EVAL8(__VA_ARGS__))
#define DETAIL__PP_EVAL8(...) DETAIL__PP_EVAL4(DETAIL__PP_EVAL4(__VA_ARGS__))
#define DETAIL__PP_EVAL4(...) DETAIL__PP_EVAL2(DETAIL__PP_EVAL2(__VA_ARGS__))
#define DETAIL__PP_EVAL2(...) DETAIL__PP_EVAL1(DETAIL__PP_EVAL1(__VA_ARGS__))
#define DETAIL__PP_EVAL1(...) __VA_ARGS__

#define DETAIL__PP_CONST_VALUE_ARR(EXPR_) core::detail::ConstValueHelper<sizeof(core::detail::ReturnCharIfString(EXPR_))>::arr

// Extract the high bits of x. We cannot just do (x & 0xffff0000) because 0x7fffffff is the maximum permitted array size on 32bit, so we have to shift
// and the array is not allowed to be size 0, so we add 0x10000 to ensure nonzero
#define DETAIL__PP_CONST_VALUE_HIGHBITS(EXPR_) ((sizeof(DETAIL__PP_CONST_VALUE_ARR(EXPR_)[ ((ptrdiff_t)(EXPR_) >> 16) + 0x10000]) - 0x10000) << 16)

// Extract the low bits of x - as with the high bits, the array cannot be zero-length, so we add 1 and then subtract it again after the sizeof
#define DETAIL__PP_CONST_VALUE_LOWBITS(EXPR_) (sizeof(DETAIL__PP_CONST_VALUE_ARR(EXPR_)[ ((ptrdiff_t)(EXPR_) & 0xFFFF) + 1]) - 1)

#define DETAIL__PP_STRINGIZE_EXPAND(EXPR_) #EXPR_

// Expand to 1 if the first argument is DETAIL__PP_PROBE(), 0 otherwise
#define DETAIL__PP_IS_PROBE(...) PP_DEFER(PP_SECOND)(__VA_ARGS__, 0)
#define DETAIL__PP_PROBE() _, 1

#define DETAIL__PP_BOOLIFY_NOT(EXPR_) PP_DEFER(DETAIL__PP_IS_PROBE)(PP_CONCAT(DETAIL__PP_BOOLIFY_NOT_PROBE_, EXPR_))
#define DETAIL__PP_BOOLIFY_NOT_PROBE_0 DETAIL__PP_PROBE()
#define DETAIL__PP_BOOLIFY_NOT_PROBE_1 0

#define DETAIL__PP_VARG_END_MARKER() 0
#define DETAIL__PP_VARG_UNPAREN_FIRST(...) DETAIL__PP_UNPAREN_EVAL_AND_CONCAT_FIRST_2_ARGS(DETAIL__PP_UNPAREN_EMPTY_, DETAIL__PP_UNPAREN_HELPER __VA_ARGS__)

#define DETAIL__PP_IF_ELSE(EXPR_) PP_CONCAT(DETAIL__PP_IF_, EXPR_)
#define DETAIL__PP_IF_1(...) __VA_ARGS__ DETAIL__PP_IF_1_ELSE
#define DETAIL__PP_IF_0(...)             DETAIL__PP_IF_0_ELSE
#define DETAIL__PP_IF_1_ELSE(...)
#define DETAIL__PP_IF_0_ELSE(...) __VA_ARGS__

#define DETAIL__PP_MAP_NONOPTIONAL(MACRO_, FIRST_, ...) MACRO_(FIRST_) PP_IF_ELSE(PP_VARG_IS_NONEMPTY(__VA_ARGS__))( PP_DEFER2(DETAIL__PP_MAP_RECURSE)()(MACRO_, __VA_ARGS__) )()
#define DETAIL__PP_MAP_RECURSE() DETAIL__PP_MAP_NONOPTIONAL

#define DETAIL__PP_UNPAREN_CONCAT_FIRST_2_ARGS(x, ...) x##__VA_ARGS__
#define DETAIL__PP_UNPAREN_EVAL_AND_CONCAT_FIRST_2_ARGS(x, ...) DETAIL__PP_UNPAREN_CONCAT_FIRST_2_ARGS(x, __VA_ARGS__)
#define DETAIL__PP_UNPAREN_EMPTY_DETAIL__PP_UNPAREN_HELPER
#define DETAIL__PP_UNPAREN_HELPER(...) DETAIL__PP_UNPAREN_HELPER __VA_ARGS__

#if __cplusplus

namespace core
{
namespace detail
{
    char ReturnCharIfString(const char*);
    long ReturnCharIfString(unsigned int);
    long ReturnCharIfString(int);
    long ReturnCharIfString(float);

    template<int dummy> struct ConstValueHelper { typedef char arr; };
    template<> struct ConstValueHelper<sizeof(char)> { static char arr[1]; };
}
}

#endif

#endif /* DETAIL__PP_AND_DETAILS_CORE_MACROS_DEFINED */
