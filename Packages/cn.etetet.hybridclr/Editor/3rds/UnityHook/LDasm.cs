using System;
using System.Runtime.InteropServices;

namespace DotNetDetour
{
    /// <summary>
    /// 用于计算汇编指令长度，使用的是BlackBone的LDasm.c中的算法，我把他翻译成C#了
    /// </summary>
    public unsafe class LDasm
    {
        const int F_INVALID = 0x01;
        const int F_PREFIX = 0x02;
        const int F_REX = 0x04;
        const int F_MODRM = 0x08;
        const int F_SIB = 0x10;
        const int F_DISP = 0x20;
        const int F_IMM = 0x40;
        const int F_RELATIVE = 0x80;

        const int OP_NONE = 0x00;
        const int OP_INVALID = 0x80;

        const int OP_DATA_I8 = 0x01;
        const int OP_DATA_I16 = 0x02;
        const int OP_DATA_I16_I32 = 0x04;
        const int OP_DATA_I16_I32_I64 = 0x08;
        const int OP_EXTENDED = 0x10;
        const int OP_RELATIVE = 0x20;
        const int OP_MODRM = 0x40;
        const int OP_PREFIX = 0x80;

        struct ldasm_data
        {
            public byte flags;
            public byte rex;
            public byte modrm;
            public byte sib;
            public byte opcd_offset;
            public byte opcd_size;
            public byte disp_offset;
            public byte disp_size;
            public byte imm_offset;
            public byte imm_size;
        }

        static byte[] flags_table =
        {
            /* 00 */    OP_MODRM,
            /* 01 */    OP_MODRM,
            /* 02 */    OP_MODRM,
            /* 03 */    OP_MODRM,
            /* 04 */    OP_DATA_I8,
            /* 05 */    OP_DATA_I16_I32,
            /* 06 */    OP_NONE,
            /* 07 */    OP_NONE,
            /* 08 */    OP_MODRM,
            /* 09 */    OP_MODRM,
            /* 0A */    OP_MODRM,
            /* 0B */    OP_MODRM,
            /* 0C */    OP_DATA_I8,
            /* 0D */    OP_DATA_I16_I32,
            /* 0E */    OP_NONE,
            /* 0F */    OP_NONE,
    
            /* 10 */    OP_MODRM,
            /* 11 */    OP_MODRM,
            /* 12 */    OP_MODRM,
            /* 13 */    OP_MODRM,
            /* 14 */    OP_DATA_I8,
            /* 15 */    OP_DATA_I16_I32,
            /* 16 */    OP_NONE,
            /* 17 */    OP_NONE,
            /* 18 */    OP_MODRM,
            /* 19 */    OP_MODRM,
            /* 1A */    OP_MODRM,
            /* 1B */    OP_MODRM,
            /* 1C */    OP_DATA_I8,
            /* 1D */    OP_DATA_I16_I32,
            /* 1E */    OP_NONE,
            /* 1F */    OP_NONE,
    
            /* 20 */    OP_MODRM,
            /* 21 */    OP_MODRM,
            /* 22 */    OP_MODRM,
            /* 23 */    OP_MODRM,
            /* 24 */    OP_DATA_I8,
            /* 25 */    OP_DATA_I16_I32,
            /* 26 */    OP_PREFIX,
            /* 27 */    OP_NONE,
            /* 28 */    OP_MODRM,
            /* 29 */    OP_MODRM,
            /* 2A */    OP_MODRM,
            /* 2B */    OP_MODRM,
            /* 2C */    OP_DATA_I8,
            /* 2D */    OP_DATA_I16_I32,
            /* 2E */    OP_PREFIX,
            /* 2F */    OP_NONE,
    
            /* 30 */    OP_MODRM,
            /* 31 */    OP_MODRM,
            /* 32 */    OP_MODRM,
            /* 33 */    OP_MODRM,
            /* 34 */    OP_DATA_I8,
            /* 35 */    OP_DATA_I16_I32,
            /* 36 */    OP_PREFIX,
            /* 37 */    OP_NONE,
            /* 38 */    OP_MODRM,
            /* 39 */    OP_MODRM,
            /* 3A */    OP_MODRM,
            /* 3B */    OP_MODRM,
            /* 3C */    OP_DATA_I8,
            /* 3D */    OP_DATA_I16_I32,
            /* 3E */    OP_PREFIX,
            /* 3F */    OP_NONE,
    
            /* 40 */    OP_NONE,
            /* 41 */    OP_NONE,
            /* 42 */    OP_NONE,
            /* 43 */    OP_NONE,
            /* 44 */    OP_NONE,
            /* 45 */    OP_NONE,
            /* 46 */    OP_NONE,
            /* 47 */    OP_NONE,
            /* 48 */    OP_NONE,
            /* 49 */    OP_NONE,
            /* 4A */    OP_NONE,
            /* 4B */    OP_NONE,
            /* 4C */    OP_NONE,
            /* 4D */    OP_NONE,
            /* 4E */    OP_NONE,
            /* 4F */    OP_NONE,
    
            /* 50 */    OP_NONE,
            /* 51 */    OP_NONE,
            /* 52 */    OP_NONE,
            /* 53 */    OP_NONE,
            /* 54 */    OP_NONE,
            /* 55 */    OP_NONE,
            /* 56 */    OP_NONE,
            /* 57 */    OP_NONE,
            /* 58 */    OP_NONE,
            /* 59 */    OP_NONE,
            /* 5A */    OP_NONE,
            /* 5B */    OP_NONE,
            /* 5C */    OP_NONE,
            /* 5D */    OP_NONE,
            /* 5E */    OP_NONE,
            /* 5F */    OP_NONE,
            /* 60 */    OP_NONE,
    
            /* 61 */    OP_NONE,
            /* 62 */    OP_MODRM,
            /* 63 */    OP_MODRM,
            /* 64 */    OP_PREFIX,
            /* 65 */    OP_PREFIX,
            /* 66 */    OP_PREFIX,
            /* 67 */    OP_PREFIX,
            /* 68 */    OP_DATA_I16_I32,
            /* 69 */    OP_MODRM | OP_DATA_I16_I32,
            /* 6A */    OP_DATA_I8,
            /* 6B */    OP_MODRM | OP_DATA_I8,
            /* 6C */    OP_NONE,
            /* 6D */    OP_NONE,
            /* 6E */    OP_NONE,
            /* 6F */    OP_NONE,
    
            /* 70 */    OP_RELATIVE | OP_DATA_I8,
            /* 71 */    OP_RELATIVE | OP_DATA_I8,
            /* 72 */    OP_RELATIVE | OP_DATA_I8,
            /* 73 */    OP_RELATIVE | OP_DATA_I8,
            /* 74 */    OP_RELATIVE | OP_DATA_I8,
            /* 75 */    OP_RELATIVE | OP_DATA_I8,
            /* 76 */    OP_RELATIVE | OP_DATA_I8,
            /* 77 */    OP_RELATIVE | OP_DATA_I8,
            /* 78 */    OP_RELATIVE | OP_DATA_I8,
            /* 79 */    OP_RELATIVE | OP_DATA_I8,
            /* 7A */    OP_RELATIVE | OP_DATA_I8,
            /* 7B */    OP_RELATIVE | OP_DATA_I8,
            /* 7C */    OP_RELATIVE | OP_DATA_I8,
            /* 7D */    OP_RELATIVE | OP_DATA_I8,
            /* 7E */    OP_RELATIVE | OP_DATA_I8,
            /* 7F */    OP_RELATIVE | OP_DATA_I8,
    
            /* 80 */    OP_MODRM | OP_DATA_I8,
            /* 81 */    OP_MODRM | OP_DATA_I16_I32,
            /* 82 */    OP_MODRM | OP_DATA_I8,
            /* 83 */    OP_MODRM | OP_DATA_I8,
            /* 84 */    OP_MODRM,
            /* 85 */    OP_MODRM,
            /* 86 */    OP_MODRM,
            /* 87 */    OP_MODRM,
            /* 88 */    OP_MODRM,
            /* 89 */    OP_MODRM,
            /* 8A */    OP_MODRM,
            /* 8B */    OP_MODRM,
            /* 8C */    OP_MODRM,
            /* 8D */    OP_MODRM,
            /* 8E */    OP_MODRM,
            /* 8F */    OP_MODRM,
    
            /* 90 */    OP_NONE,
            /* 91 */    OP_NONE,
            /* 92 */    OP_NONE,
            /* 93 */    OP_NONE,
            /* 94 */    OP_NONE,
            /* 95 */    OP_NONE,
            /* 96 */    OP_NONE,
            /* 97 */    OP_NONE,
            /* 98 */    OP_NONE,
            /* 99 */    OP_NONE,
            /* 9A */    OP_DATA_I16 | OP_DATA_I16_I32,
            /* 9B */    OP_NONE,
            /* 9C */    OP_NONE,
            /* 9D */    OP_NONE,
            /* 9E */    OP_NONE,
            /* 9F */    OP_NONE,
    
            /* A0 */    OP_DATA_I8,
            /* A1 */    OP_DATA_I16_I32_I64,
            /* A2 */    OP_DATA_I8,
            /* A3 */    OP_DATA_I16_I32_I64,
            /* A4 */    OP_NONE,
            /* A5 */    OP_NONE,
            /* A6 */    OP_NONE,
            /* A7 */    OP_NONE,
            /* A8 */    OP_DATA_I8,
            /* A9 */    OP_DATA_I16_I32,
            /* AA */    OP_NONE,
            /* AB */    OP_NONE,
            /* AC */    OP_NONE,
            /* AD */    OP_NONE,
            /* AE */    OP_NONE,
            /* AF */    OP_NONE,
    
            /* B0 */    OP_DATA_I8,
            /* B1 */    OP_DATA_I8,
            /* B2 */    OP_DATA_I8,
            /* B3 */    OP_DATA_I8,
            /* B4 */    OP_DATA_I8,
            /* B5 */    OP_DATA_I8,
            /* B6 */    OP_DATA_I8,
            /* B7 */    OP_DATA_I8,
            /* B8 */    OP_DATA_I16_I32_I64,
            /* B9 */    OP_DATA_I16_I32_I64,
            /* BA */    OP_DATA_I16_I32_I64,
            /* BB */    OP_DATA_I16_I32_I64,
            /* BC */    OP_DATA_I16_I32_I64,
            /* BD */    OP_DATA_I16_I32_I64,
            /* BE */    OP_DATA_I16_I32_I64,
            /* BF */    OP_DATA_I16_I32_I64,
    
            /* C0 */    OP_MODRM | OP_DATA_I8,
            /* C1 */    OP_MODRM | OP_DATA_I8,
            /* C2 */    OP_DATA_I16,
            /* C3 */    OP_NONE,
            /* C4 */    OP_MODRM,
            /* C5 */    OP_MODRM,
            /* C6 */    OP_MODRM | OP_DATA_I8,
            /* C7 */    OP_MODRM | OP_DATA_I16_I32,
            /* C8 */    OP_DATA_I8 | OP_DATA_I16,
            /* C9 */    OP_NONE,
            /* CA */    OP_DATA_I16,
            /* CB */    OP_NONE,
            /* CC */    OP_NONE,
            /* CD */    OP_DATA_I8,
            /* CE */    OP_NONE,
            /* CF */    OP_NONE,
    
            /* D0 */    OP_MODRM,
            /* D1 */    OP_MODRM,
            /* D2 */    OP_MODRM,
            /* D3 */    OP_MODRM,
            /* D4 */    OP_DATA_I8,
            /* D5 */    OP_DATA_I8,
            /* D6 */    OP_NONE,
            /* D7 */    OP_NONE,
            /* D8 */    OP_MODRM,
            /* D9 */    OP_MODRM,
            /* DA */    OP_MODRM,
            /* DB */    OP_MODRM,
            /* DC */    OP_MODRM,
            /* DD */    OP_MODRM,
            /* DE */    OP_MODRM,
            /* DF */    OP_MODRM,
    
            /* E0 */    OP_RELATIVE | OP_DATA_I8,
            /* E1 */    OP_RELATIVE | OP_DATA_I8,
            /* E2 */    OP_RELATIVE | OP_DATA_I8,
            /* E3 */    OP_RELATIVE | OP_DATA_I8,
            /* E4 */    OP_DATA_I8,
            /* E5 */    OP_DATA_I8,
            /* E6 */    OP_DATA_I8,
            /* E7 */    OP_DATA_I8,
            /* E8 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* E9 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* EA */    OP_DATA_I16 | OP_DATA_I16_I32,
            /* EB */    OP_RELATIVE | OP_DATA_I8,
            /* EC */    OP_NONE,
            /* ED */    OP_NONE,
            /* EE */    OP_NONE,
            /* EF */    OP_NONE,
    
            /* F0 */    OP_PREFIX,
            /* F1 */    OP_NONE,
            /* F2 */    OP_PREFIX,
            /* F3 */    OP_PREFIX,
            /* F4 */    OP_NONE,
            /* F5 */    OP_NONE,
            /* F6 */    OP_MODRM,
            /* F7 */    OP_MODRM,
            /* F8 */    OP_NONE,
            /* F9 */    OP_NONE,
            /* FA */    OP_NONE,
            /* FB */    OP_NONE,
            /* FC */    OP_NONE,
            /* FD */    OP_NONE,
            /* FE */    OP_MODRM,
            /* FF */    OP_MODRM
        };

        static byte[] flags_table_ex =
        {
            /* 0F00 */    OP_MODRM,
            /* 0F01 */    OP_MODRM,
            /* 0F02 */    OP_MODRM,
            /* 0F03 */    OP_MODRM,
            /* 0F04 */    OP_INVALID,
            /* 0F05 */    OP_NONE,
            /* 0F06 */    OP_NONE,
            /* 0F07 */    OP_NONE,
            /* 0F08 */    OP_NONE,
            /* 0F09 */    OP_NONE,
            /* 0F0A */    OP_INVALID,
            /* 0F0B */    OP_NONE,
            /* 0F0C */    OP_INVALID,
            /* 0F0D */    OP_MODRM,
            /* 0F0E */    OP_INVALID,
            /* 0F0F */    OP_MODRM | OP_DATA_I8,        //3Dnow
    
            /* 0F10 */    OP_MODRM,
            /* 0F11 */    OP_MODRM,
            /* 0F12 */    OP_MODRM,
            /* 0F13 */    OP_MODRM,
            /* 0F14 */    OP_MODRM,
            /* 0F15 */    OP_MODRM,
            /* 0F16 */    OP_MODRM,
            /* 0F17 */    OP_MODRM,
            /* 0F18 */    OP_MODRM,
            /* 0F19 */    OP_INVALID,
            /* 0F1A */    OP_INVALID,
            /* 0F1B */    OP_INVALID,
            /* 0F1C */    OP_INVALID,
            /* 0F1D */    OP_INVALID,
            /* 0F1E */    OP_INVALID,
            /* 0F1F */    OP_NONE,
    
            /* 0F20 */    OP_MODRM,
            /* 0F21 */    OP_MODRM,
            /* 0F22 */    OP_MODRM,
            /* 0F23 */    OP_MODRM,
            /* 0F24 */    OP_MODRM | OP_EXTENDED,        //SSE5
            /* 0F25 */    OP_INVALID,
            /* 0F26 */    OP_MODRM,
            /* 0F27 */    OP_INVALID,
            /* 0F28 */    OP_MODRM,
            /* 0F29 */    OP_MODRM,
            /* 0F2A */    OP_MODRM,
            /* 0F2B */    OP_MODRM,
            /* 0F2C */    OP_MODRM,
            /* 0F2D */    OP_MODRM,
            /* 0F2E */    OP_MODRM,
            /* 0F2F */    OP_MODRM,
    
            /* 0F30 */    OP_NONE,
            /* 0F31 */    OP_NONE,
            /* 0F32 */    OP_NONE,
            /* 0F33 */    OP_NONE,
            /* 0F34 */    OP_NONE,
            /* 0F35 */    OP_NONE,
            /* 0F36 */    OP_INVALID,
            /* 0F37 */    OP_NONE,
            /* 0F38 */    OP_MODRM | OP_EXTENDED, 
            /* 0F39 */    OP_INVALID,
            /* 0F3A */    OP_MODRM | OP_EXTENDED | OP_DATA_I8,
            /* 0F3B */    OP_INVALID,
            /* 0F3C */    OP_INVALID,
            /* 0F3D */    OP_INVALID,
            /* 0F3E */    OP_INVALID,
            /* 0F3F */    OP_INVALID,
    
            /* 0F40 */    OP_MODRM,
            /* 0F41 */    OP_MODRM,
            /* 0F42 */    OP_MODRM,
            /* 0F43 */    OP_MODRM,
            /* 0F44 */    OP_MODRM,
            /* 0F45 */    OP_MODRM,
            /* 0F46 */    OP_MODRM,
            /* 0F47 */    OP_MODRM,
            /* 0F48 */    OP_MODRM,
            /* 0F49 */    OP_MODRM,
            /* 0F4A */    OP_MODRM,
            /* 0F4B */    OP_MODRM,
            /* 0F4C */    OP_MODRM,
            /* 0F4D */    OP_MODRM,
            /* 0F4E */    OP_MODRM,
            /* 0F4F */    OP_MODRM,
    
            /* 0F50 */    OP_MODRM,
            /* 0F51 */    OP_MODRM,
            /* 0F52 */    OP_MODRM,
            /* 0F53 */    OP_MODRM,
            /* 0F54 */    OP_MODRM,
            /* 0F55 */    OP_MODRM,
            /* 0F56 */    OP_MODRM,
            /* 0F57 */    OP_MODRM,
            /* 0F58 */    OP_MODRM,
            /* 0F59 */    OP_MODRM,
            /* 0F5A */    OP_MODRM,
            /* 0F5B */    OP_MODRM,
            /* 0F5C */    OP_MODRM,
            /* 0F5D */    OP_MODRM,
            /* 0F5E */    OP_MODRM,
            /* 0F5F */    OP_MODRM,
    
            /* 0F60 */    OP_MODRM,
            /* 0F61 */    OP_MODRM,
            /* 0F62 */    OP_MODRM,
            /* 0F63 */    OP_MODRM,
            /* 0F64 */    OP_MODRM,
            /* 0F65 */    OP_MODRM,
            /* 0F66 */    OP_MODRM,
            /* 0F67 */    OP_MODRM,
            /* 0F68 */    OP_MODRM,
            /* 0F69 */    OP_MODRM,
            /* 0F6A */    OP_MODRM,
            /* 0F6B */    OP_MODRM,
            /* 0F6C */    OP_MODRM,
            /* 0F6D */    OP_MODRM,
            /* 0F6E */    OP_MODRM,
            /* 0F6F */    OP_MODRM,
    
            /* 0F70 */    OP_MODRM | OP_DATA_I8,
            /* 0F71 */    OP_MODRM | OP_DATA_I8,
            /* 0F72 */    OP_MODRM | OP_DATA_I8,
            /* 0F73 */    OP_MODRM | OP_DATA_I8,
            /* 0F74 */    OP_MODRM,
            /* 0F75 */    OP_MODRM,
            /* 0F76 */    OP_MODRM,
            /* 0F77 */    OP_NONE,
            /* 0F78 */    OP_MODRM,
            /* 0F79 */    OP_MODRM,
            /* 0F7A */    OP_INVALID,
            /* 0F7B */    OP_INVALID,
            /* 0F7C */    OP_MODRM,
            /* 0F7D */    OP_MODRM,
            /* 0F7E */    OP_MODRM,
            /* 0F7F */    OP_MODRM,
    
            /* 0F80 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F81 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F82 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F83 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F84 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F85 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F86 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F87 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F88 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F89 */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F8A */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F8B */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F8C */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F8D */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F8E */    OP_RELATIVE | OP_DATA_I16_I32,
            /* 0F8F */    OP_RELATIVE | OP_DATA_I16_I32,
    
            /* 0F90 */    OP_MODRM,
            /* 0F91 */    OP_MODRM,
            /* 0F92 */    OP_MODRM,
            /* 0F93 */    OP_MODRM,
            /* 0F94 */    OP_MODRM,
            /* 0F95 */    OP_MODRM,
            /* 0F96 */    OP_MODRM,
            /* 0F97 */    OP_MODRM,
            /* 0F98 */    OP_MODRM,
            /* 0F99 */    OP_MODRM,
            /* 0F9A */    OP_MODRM,
            /* 0F9B */    OP_MODRM,
            /* 0F9C */    OP_MODRM,
            /* 0F9D */    OP_MODRM,
            /* 0F9E */    OP_MODRM,
            /* 0F9F */    OP_MODRM,
    
            /* 0FA0 */    OP_NONE,
            /* 0FA1 */    OP_NONE,
            /* 0FA2 */    OP_NONE,
            /* 0FA3 */    OP_MODRM,
            /* 0FA4 */    OP_MODRM | OP_DATA_I8,
            /* 0FA5 */    OP_MODRM,
            /* 0FA6 */    OP_INVALID,
            /* 0FA7 */    OP_INVALID,
            /* 0FA8 */    OP_NONE,
            /* 0FA9 */    OP_NONE,
            /* 0FAA */    OP_NONE,
            /* 0FAB */    OP_MODRM,
            /* 0FAC */    OP_MODRM | OP_DATA_I8,
            /* 0FAD */    OP_MODRM,
            /* 0FAE */    OP_MODRM,
            /* 0FAF */    OP_MODRM,
    
            /* 0FB0 */    OP_MODRM,
            /* 0FB1 */    OP_MODRM,
            /* 0FB2 */    OP_MODRM,
            /* 0FB3 */    OP_MODRM,
            /* 0FB4 */    OP_MODRM,
            /* 0FB5 */    OP_MODRM,
            /* 0FB6 */    OP_MODRM,
            /* 0FB7 */    OP_MODRM,
            /* 0FB8 */    OP_MODRM,
            /* 0FB9 */    OP_MODRM,
            /* 0FBA */    OP_MODRM | OP_DATA_I8,
            /* 0FBB */    OP_MODRM,
            /* 0FBC */    OP_MODRM,
            /* 0FBD */    OP_MODRM,
            /* 0FBE */    OP_MODRM,
            /* 0FBF */    OP_MODRM,
    
            /* 0FC0 */    OP_MODRM,
            /* 0FC1 */    OP_MODRM,
            /* 0FC2 */    OP_MODRM | OP_DATA_I8,
            /* 0FC3 */    OP_MODRM,
            /* 0FC4 */    OP_MODRM | OP_DATA_I8,
            /* 0FC5 */    OP_MODRM | OP_DATA_I8,
            /* 0FC6 */    OP_MODRM | OP_DATA_I8,
            /* 0FC7 */    OP_MODRM,
            /* 0FC8 */    OP_NONE,
            /* 0FC9 */    OP_NONE,
            /* 0FCA */    OP_NONE,
            /* 0FCB */    OP_NONE,
            /* 0FCC */    OP_NONE,
            /* 0FCD */    OP_NONE,
            /* 0FCE */    OP_NONE,
            /* 0FCF */    OP_NONE,
    
            /* 0FD0 */    OP_MODRM,
            /* 0FD1 */    OP_MODRM,
            /* 0FD2 */    OP_MODRM,
            /* 0FD3 */    OP_MODRM,
            /* 0FD4 */    OP_MODRM,
            /* 0FD5 */    OP_MODRM,
            /* 0FD6 */    OP_MODRM,
            /* 0FD7 */    OP_MODRM,
            /* 0FD8 */    OP_MODRM,
            /* 0FD9 */    OP_MODRM,
            /* 0FDA */    OP_MODRM,
            /* 0FDB */    OP_MODRM,
            /* 0FDC */    OP_MODRM,
            /* 0FDD */    OP_MODRM,
            /* 0FDE */    OP_MODRM,
            /* 0FDF */    OP_MODRM,
    
            /* 0FE0 */    OP_MODRM,
            /* 0FE1 */    OP_MODRM,
            /* 0FE2 */    OP_MODRM,
            /* 0FE3 */    OP_MODRM,
            /* 0FE4 */    OP_MODRM,
            /* 0FE5 */    OP_MODRM,
            /* 0FE6 */    OP_MODRM,
            /* 0FE7 */    OP_MODRM,
            /* 0FE8 */    OP_MODRM,
            /* 0FE9 */    OP_MODRM,
            /* 0FEA */    OP_MODRM,
            /* 0FEB */    OP_MODRM,
            /* 0FEC */    OP_MODRM,
            /* 0FED */    OP_MODRM,
            /* 0FEE */    OP_MODRM,
            /* 0FEF */    OP_MODRM,
    
            /* 0FF0 */    OP_MODRM,
            /* 0FF1 */    OP_MODRM,
            /* 0FF2 */    OP_MODRM,
            /* 0FF3 */    OP_MODRM,
            /* 0FF4 */    OP_MODRM,
            /* 0FF5 */    OP_MODRM,
            /* 0FF6 */    OP_MODRM,
            /* 0FF7 */    OP_MODRM,
            /* 0FF8 */    OP_MODRM,
            /* 0FF9 */    OP_MODRM,
            /* 0FFA */    OP_MODRM,
            /* 0FFB */    OP_MODRM,
            /* 0FFC */    OP_MODRM,
            /* 0FFD */    OP_MODRM,
            /* 0FFE */    OP_MODRM,
            /* 0FFF */    OP_INVALID,
        };

        static byte cflags(byte op)
        {
            return flags_table[op];
        }

        static byte cflags_ex(byte op)
        {
            return flags_table_ex[op];
        }

        /// <summary>
        /// 计算大于等于 size 字节的最少指令的长度
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static uint SizeofMinNumByte(void* code, int size)
        {
            if (IsARM())
                return (uint)((size + 3) / 4) * 4; // 此为 jit 模式下的长度，不再支持 thumb

            uint Length;
            byte* pOpcode;
            uint Result = 0;
            ldasm_data data = new ldasm_data();
            bool is64 = IntPtr.Size == 8;
            do
            {
                Length = ldasm(code, data, is64);

                pOpcode = (byte*)code + data.opcd_offset;
                Result += Length;
                if (Result >= size)
                    break;
                if ((Length == 1) && (*pOpcode == 0xCC))
                    break;

                code = (void*)((ulong)code + Length);

            } while (Length>0);

            return Result;
        }

        static bool? s_isArm;
        public static bool IsARM()
        {
            if(s_isArm.HasValue)
                return s_isArm.Value;

            var arch = RuntimeInformation.ProcessArchitecture;
            s_isArm = arch == Architecture.Arm || arch == Architecture.Arm64;

            return s_isArm.Value;
        }

        public static bool IsArm32()
        {
            return IsARM() && IntPtr.Size == 4;
        }

        public static bool IsArm64()
        {
            return IsARM() && IntPtr.Size == 8;
        }

        static bool? s_isiOS;
        public static bool IsiOS()
        {
            if(s_isiOS.HasValue)
                return s_isiOS.Value;

            s_isiOS = UnityEngine.SystemInfo.operatingSystem.ToLower().Contains("ios");
            return s_isiOS.Value;
        }

        static bool? s_isIL2CPP;
        public static bool IsIL2CPP()
        {
            if (s_isIL2CPP.HasValue)
                return s_isIL2CPP.Value;

            try
            {
                byte[] ilBody = typeof(LDasm).GetMethod("IsIL2CPP").GetMethodBody().GetILAsByteArray();
                if (ilBody == null || ilBody.Length == 0)
                    s_isIL2CPP = true;
                else
                    s_isIL2CPP = false;
            }
            catch
            {
                s_isIL2CPP = true;
            }

            return s_isIL2CPP.Value;
        }

        public static bool IsThumb(IntPtr code)
        {
            return IsArm32() &&  ((long)code & 0x1) == 0x1;
        }

        /// <summary>
        /// 计算 thumb 指令长度
        /// </summary>
        /// <param name="code"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static uint CalcARMThumbMinLen(void* code, int size)
        {
            uint len = 0;

            ushort* ins = (ushort*)code;
            while (true)
            {
                if (len >= size)
                    return len;

                if (((*ins >> 13) & 3) == 3)
                {
                    ins += 2;
                    len += 4;
                }
                else
                {
                    ins++;
                    len += 2;
                }
            }
        }

        static uint ldasm(void* code, ldasm_data ld, bool is64)
        {
            byte* p = (byte*)code;
            byte s, op, f;
            byte rexw, pr_66, pr_67;

            s = rexw = pr_66 = pr_67 = 0;

            /* dummy check */
            if ((int)code==0)
                return 0;

            /* init output data */
            //memset(ld, 0, sizeof(ldasm_data));

            /* phase 1: parse prefixies */
            while ((cflags(*p) & OP_PREFIX)!=0)
            {
                if (*p == 0x66)
                    pr_66 = 1;
                if (*p == 0x67)
                    pr_67 = 1;
                p++; s++;
                ld.flags |= F_PREFIX;
                if (s == 15)
                {
                    ld.flags |= F_INVALID;
                    return s;
                }
            }

            /* parse REX prefix */
            if (is64 && *p >> 4 == 4)
            {
                ld.rex = *p;
                rexw = (byte)((ld.rex >> 3) & 1);
                ld.flags |= F_REX;
                p++; s++;
            }

            /* can be only one REX prefix */
            if (is64 && *p >> 4 == 4)
            {
                ld.flags |= F_INVALID;
                s++;
                return s;
            }

            /* phase 2: parse opcode */
            ld.opcd_offset = (byte)(p - (byte*)code);
            ld.opcd_size = 1;
            op = *p++; s++;

            /* is 2 byte opcode? */
            if (op == 0x0F)
            {
                op = *p++; s++;
                ld.opcd_size++;
                f = cflags_ex(op);
                if ((f & OP_INVALID)!=0)
                {
                    ld.flags |= F_INVALID;
                    return s;
                }
                /* for SSE instructions */
                if ((f & OP_EXTENDED)!=0)
                {
                    op = *p++; s++;
                    ld.opcd_size++;
                }
            }
            else {
                f = cflags(op);
                /* pr_66 = pr_67 for opcodes A0-A3 */
                if (op >= 0xA0 && op <= 0xA3)
                    pr_66 = pr_67;
            }

            /* phase 3: parse ModR/M, SIB and DISP */
            if ((f & OP_MODRM)!=0)
            {
                byte mod = (byte)(*p >> 6);
                byte ro = (byte)((*p & 0x38) >> 3);
                byte rm = (byte)(*p & 7);

                ld.modrm = *p++; s++;
                ld.flags |= F_MODRM;

                /* in F6,F7 opcodes immediate data present if R/O == 0 */
                if (op == 0xF6 && (ro == 0 || ro == 1))
                    f |= OP_DATA_I8;
                if (op == 0xF7 && (ro == 0 || ro == 1))
                    f |= OP_DATA_I16_I32_I64;

                /* is SIB byte exist? */
                if (mod != 3 && rm == 4 && !(!is64 && pr_67!=0))
                {
                    ld.sib = *p++; s++;
                    ld.flags |= F_SIB;

                    /* if base == 5 and mod == 0 */
                    if ((ld.sib & 7) == 5 && mod == 0)
                    {
                        ld.disp_size = 4;
                    }
                }

                switch (mod)
                {
                    case 0:
                        if (is64)
                        {
                            if (rm == 5)
                            {
                                ld.disp_size = 4;
                                if (is64)
                                    ld.flags |= F_RELATIVE;
                            }
                        }
                        else if (pr_67!=0)
                        {
                            if (rm == 6)
                                ld.disp_size = 2;
                        }
                        else {
                            if (rm == 5)
                                ld.disp_size = 4;
                        }
                        break;
                    case 1:
                        ld.disp_size = 1;
                        break;
                    case 2:
                        if (is64)
                            ld.disp_size = 4;
                        else if (pr_67!=0)
                            ld.disp_size = 2;
                        else
                            ld.disp_size = 4;
                        break;
                }

                if (ld.disp_size>0)
                {
                    ld.disp_offset = (byte)(p - (byte*)code);
                    p += ld.disp_size;
                    s += ld.disp_size;
                    ld.flags |= F_DISP;
                }
            }

            /* phase 4: parse immediate data */
            if (rexw!=0 && (f & OP_DATA_I16_I32_I64)!=0)
                ld.imm_size = 8;
            else if ((f & OP_DATA_I16_I32)!=0 || (f & OP_DATA_I16_I32_I64)!=0)
                ld.imm_size = (byte)(4 - (pr_66 << 1));

            /* if exist, add OP_DATA_I16 and OP_DATA_I8 size */
            ld.imm_size += (byte)(f & 3);

            if ((ld.imm_size)!=0)
            {
                s += ld.imm_size;
                ld.imm_offset = (byte)(p - (byte*)code);
                ld.flags |= F_IMM;
                if ((f & OP_RELATIVE)!=0)
                    ld.flags |= F_RELATIVE;
            }

            /* instruction is too long */
            if (s > 15)
                ld.flags |= F_INVALID;

            return s;
        }
    }
}
