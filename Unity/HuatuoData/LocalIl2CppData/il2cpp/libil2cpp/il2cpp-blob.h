#pragma once

// Corresponds to element type signatures
// See ECMA-335, II.23.1.16

typedef enum Il2CppTypeEnum
{
    IL2CPP_TYPE_END        = 0x00,       /* End of List */
    IL2CPP_TYPE_VOID       = 0x01,
    IL2CPP_TYPE_BOOLEAN    = 0x02,
    IL2CPP_TYPE_CHAR       = 0x03,
    IL2CPP_TYPE_I1         = 0x04,
    IL2CPP_TYPE_U1         = 0x05,
    IL2CPP_TYPE_I2         = 0x06,
    IL2CPP_TYPE_U2         = 0x07,
    IL2CPP_TYPE_I4         = 0x08,
    IL2CPP_TYPE_U4         = 0x09,
    IL2CPP_TYPE_I8         = 0x0a,
    IL2CPP_TYPE_U8         = 0x0b,
    IL2CPP_TYPE_R4         = 0x0c,
    IL2CPP_TYPE_R8         = 0x0d,
    IL2CPP_TYPE_STRING     = 0x0e,
    IL2CPP_TYPE_PTR        = 0x0f,       /* arg: <type> token */
    IL2CPP_TYPE_BYREF      = 0x10,       /* arg: <type> token */
    IL2CPP_TYPE_VALUETYPE  = 0x11,       /* arg: <type> token */
    IL2CPP_TYPE_CLASS      = 0x12,       /* arg: <type> token */
    IL2CPP_TYPE_VAR        = 0x13,       /* Generic parameter in a generic type definition, represented as number (compressed unsigned integer) number */
    IL2CPP_TYPE_ARRAY      = 0x14,       /* type, rank, boundsCount, bound1, loCount, lo1 */
    IL2CPP_TYPE_GENERICINST = 0x15,     /* <type> <type-arg-count> <type-1> \x{2026} <type-n> */
    IL2CPP_TYPE_TYPEDBYREF = 0x16,
    IL2CPP_TYPE_I          = 0x18,
    IL2CPP_TYPE_U          = 0x19,
    IL2CPP_TYPE_FNPTR      = 0x1b,        /* arg: full method signature */
    IL2CPP_TYPE_OBJECT     = 0x1c,
    IL2CPP_TYPE_SZARRAY    = 0x1d,       /* 0-based one-dim-array */
    IL2CPP_TYPE_MVAR       = 0x1e,       /* Generic parameter in a generic method definition, represented as number (compressed unsigned integer)  */
    IL2CPP_TYPE_CMOD_REQD  = 0x1f,       /* arg: typedef or typeref token */
    IL2CPP_TYPE_CMOD_OPT   = 0x20,       /* optional arg: typedef or typref token */
    IL2CPP_TYPE_INTERNAL   = 0x21,       /* CLR internal type */

    IL2CPP_TYPE_MODIFIER   = 0x40,       /* Or with the following types */
    IL2CPP_TYPE_SENTINEL   = 0x41,       /* Sentinel for varargs method signature */
    IL2CPP_TYPE_PINNED     = 0x45,       /* Local var that points to pinned object */
    // ==={{ huatuo
    IL2CPP_TYPE_SYSTEM_TYPE = 0x50,
    IL2CPP_TYPE_BOXED_OBJECT    = 0x51,
    IL2CPP_TYPE_FIELD       = 0x53,
    IL2CPP_TYPE_PROPERTY    = 0x54,
    // ===}} huatuo

    IL2CPP_TYPE_ENUM       = 0x55        /* an enumeration */
} Il2CppTypeEnum;
