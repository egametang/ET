using DotNetDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MonoHook
{
    public unsafe abstract class CodePatcher
    {
        public bool isValid { get; protected set; }

        protected void*     _pTarget, _pReplace, _pProxy;
        protected int       _jmpCodeSize;
        protected byte[]    _targetHeaderBackup;

        public CodePatcher(IntPtr target, IntPtr replace, IntPtr proxy, int jmpCodeSize)
        {
            _pTarget        = target.ToPointer();
            _pReplace       = replace.ToPointer();
            _pProxy         = proxy.ToPointer();
            _jmpCodeSize    = jmpCodeSize;
        }

        public void ApplyPatch()
        {
            BackupHeader();
            EnableAddrModifiable();
            PatchTargetMethod();
            PatchProxyMethod();
            FlushICache();
        }

        public void RemovePatch()
        {
            if (_targetHeaderBackup == null)
                return;

            EnableAddrModifiable();
            RestoreHeader();
            FlushICache();
        }

        protected void BackupHeader()
        {
            if (_targetHeaderBackup != null)
                return;

            uint requireSize    = LDasm.SizeofMinNumByte(_pTarget, _jmpCodeSize);
            _targetHeaderBackup = new byte[requireSize];

            fixed (void* ptr = _targetHeaderBackup)
                HookUtils.MemCpy(ptr, _pTarget, _targetHeaderBackup.Length);
        }

        protected void RestoreHeader()
        {
            if (_targetHeaderBackup == null)
                return;

            HookUtils.MemCpy_Jit(_pTarget, _targetHeaderBackup);
        }

        protected void PatchTargetMethod()
        {
            byte[] buff = GenJmpCode(_pTarget, _pReplace);
            HookUtils.MemCpy_Jit(_pTarget, buff);
        }
        protected void PatchProxyMethod()
        {
            if (_pProxy == null)
                return;

            // copy target's code to proxy
            HookUtils.MemCpy_Jit(_pProxy, _targetHeaderBackup);

            // jmp to target's new position
            long jmpFrom    = (long)_pProxy + _targetHeaderBackup.Length;
            long jmpTo      = (long)_pTarget + _targetHeaderBackup.Length;

            byte[] buff = GenJmpCode((void*)jmpFrom, (void*)jmpTo);
            HookUtils.MemCpy_Jit((void*)jmpFrom, buff);
        }

        protected void FlushICache()
        {
            HookUtils.FlushICache(_pTarget, _targetHeaderBackup.Length);
            HookUtils.FlushICache(_pProxy, _targetHeaderBackup.Length * 2);
        }
        protected abstract byte[] GenJmpCode(void* jmpFrom, void* jmpTo);

#if ENABLE_HOOK_DEBUG
        protected string PrintAddrs()
        {
            if (IntPtr.Size == 4)
                return $"target:0x{(uint)_pTarget:x}, replace:0x{(uint)_pReplace:x}, proxy:0x{(uint)_pProxy:x}";
            else
                return $"target:0x{(ulong)_pTarget:x}, replace:0x{(ulong)_pReplace:x}, proxy:0x{(ulong)_pProxy:x}";
        }
#endif

        private void EnableAddrModifiable()
        {
            HookUtils.SetAddrFlagsToRWX(new IntPtr(_pTarget), _targetHeaderBackup.Length);
            HookUtils.SetAddrFlagsToRWX(new IntPtr(_pProxy), _targetHeaderBackup.Length + _jmpCodeSize);
        }
    }

    public unsafe class CodePatcher_x86 : CodePatcher
    {
        protected static readonly byte[] s_jmpCode = new byte[] // 5 bytes
        {
            0xE9, 0x00, 0x00, 0x00, 0x00,                     // jmp $val   ; $val = $dst - $src - 5 
        };

        public CodePatcher_x86(IntPtr target, IntPtr replace, IntPtr proxy) : base(target, replace, proxy, s_jmpCode.Length) { }

        protected override unsafe byte[] GenJmpCode(void* jmpFrom, void* jmpTo)
        {
            byte[] ret = new byte[s_jmpCode.Length];
            int val = (int)jmpTo - (int)jmpFrom - 5;

            fixed(void * p = &ret[0])
            {
                byte* ptr = (byte*)p;
                *ptr = 0xE9;
                int* pOffset = (int*)(ptr + 1);
                *pOffset = val;
            }
            return ret;
        }
    }

    /// <summary>
    /// x64下2G 内的跳转
    /// </summary>
    public unsafe class CodePatcher_x64_near : CodePatcher_x86 // x64_near pathcer code is same to x86
    {
        public CodePatcher_x64_near(IntPtr target, IntPtr replace, IntPtr proxy) : base(target, replace, proxy) { }
    }

    /// <summary>
    /// x64下距离超过2G的跳转
    /// </summary>
    public unsafe class CodePatcher_x64_far : CodePatcher
    {
        protected static readonly byte[] s_jmpCode = new byte[] // 12 bytes
        {
            // 由于 rax 会被函数作为返回值修改，并且不会被做为参数使用，因此修改是安全的
            0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,         // mov rax, <jmpTo>
            0x50,                                                               // push rax
            0xC3                                                                // ret
        };

        //protected static readonly byte[] s_jmpCode2 = new byte[] // 14 bytes
        //{
        //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,       // <jmpTo>
        //    0xFF, 0x25, 0xF2, 0xFF, 0xFF, 0xFF                    // jmp [rip - 0xe]
        //};

        public CodePatcher_x64_far(IntPtr target, IntPtr replace, IntPtr proxy) : base(target, replace, proxy, s_jmpCode.Length) { }
        protected override unsafe byte[] GenJmpCode(void* jmpFrom, void* jmpTo)
        {
            byte[] ret = new byte[s_jmpCode.Length];

            fixed (void* p = &ret[0])
            {
                byte* ptr = (byte*)p;
                *ptr++ = 0x48;
                *ptr++ = 0xB8;
                *(long*)ptr = (long)jmpTo;
                ptr += 8;
                *ptr++ = 0x50;
                *ptr++ = 0xC3;
            }
            return ret;
        }
    }

    public unsafe class CodePatcher_arm32_near : CodePatcher
    {
        private static readonly byte[] s_jmpCode = new byte[]    // 4 bytes
        {
            0x00, 0x00, 0x00, 0xEA,                         // B $val   ; $val = (($dst - $src) / 4 - 2) & 0x1FFFFFF
        };

        public CodePatcher_arm32_near(IntPtr target, IntPtr replace, IntPtr proxy) : base(target, replace, proxy, s_jmpCode.Length)
        {
            if (Math.Abs((long)target - (long)replace) >= ((1 << 25) - 1))
                throw new ArgumentException("address offset of target and replace must less than ((1 << 25) - 1)");

#if ENABLE_HOOK_DEBUG
            Debug.Log($"CodePatcher_arm32_near: {PrintAddrs()}");
#endif
        }

        protected override unsafe byte[] GenJmpCode(void* jmpFrom, void* jmpTo)
        {
            byte[] ret = new byte[s_jmpCode.Length];
            int val = ((int)jmpTo - (int)jmpFrom) / 4 - 2;

            fixed (void* p = &ret[0])
            {
                byte* ptr = (byte*)p;
                *ptr++ = (byte)val;
                *ptr++ = (byte)(val >> 8);
                *ptr++ = (byte)(val >> 16);
                *ptr++ = 0xEA;
            }
            return ret;
        }
    }

    public unsafe class CodePatcher_arm32_far : CodePatcher
    {
        private static readonly byte[] s_jmpCode = new byte[]    // 8 bytes
        {
            0x04, 0xF0, 0x1F, 0xE5,                         // LDR PC, [PC, #-4]
            0x00, 0x00, 0x00, 0x00,                         // $val
        };

        public CodePatcher_arm32_far(IntPtr target, IntPtr replace, IntPtr proxy) : base(target, replace, proxy, s_jmpCode.Length)
        {
            if (Math.Abs((long)target - (long)replace) < ((1 << 25) - 1))
                throw new ArgumentException("address offset of target and replace must larger than ((1 << 25) - 1), please use InstructionModifier_arm32_near instead");

#if ENABLE_HOOK_DEBUG
            Debug.Log($"CodePatcher_arm32_far: {PrintAddrs()}");
#endif
        }

        protected override unsafe byte[] GenJmpCode(void* jmpFrom, void* jmpTo)
        {
            byte[] ret = new byte[s_jmpCode.Length];

            fixed (void* p = &ret[0])
            {
                uint* ptr = (uint*)p;
                *ptr++ = 0xE51FF004;
                *ptr = (uint)jmpTo;
            }
            return ret;
        }
    }

    /// <summary>
    /// arm64 下 ±128MB 范围内的跳转
    /// </summary>
    public unsafe class CodePatcher_arm64_near : CodePatcher
    {
        private static readonly byte[] s_jmpCode = new byte[]    // 4 bytes
        {
            /*
             * from 0x14 to 0x17 is B opcode
             * offset bits is 26
             * https://developer.arm.com/documentation/ddi0596/2021-09/Base-Instructions/B--Branch-
             */
            0x00, 0x00, 0x00, 0x14,                         //  B $val   ; $val = (($dst - $src)/4) & 7FFFFFF
        };

        public CodePatcher_arm64_near(IntPtr target, IntPtr replace, IntPtr proxy) : base(target, replace, proxy, s_jmpCode.Length)
        {
            if (Math.Abs((long)target - (long)replace) >= ((1 << 26) - 1) * 4)
                throw new ArgumentException("address offset of target and replace must less than (1 << 26) - 1) * 4");

#if ENABLE_HOOK_DEBUG
            Debug.Log($"CodePatcher_arm64: {PrintAddrs()}");
#endif
        }

        protected override unsafe byte[] GenJmpCode(void* jmpFrom, void* jmpTo)
        {
            byte[] ret = new byte[s_jmpCode.Length];
            int val = (int)((long)jmpTo - (long)jmpFrom) / 4;

            fixed (void* p = &ret[0])
            {
                byte* ptr = (byte*)p;
                *ptr++ = (byte)val;
                *ptr++ = (byte)(val >> 8);
                *ptr++ = (byte)(val >> 16);

                byte last = (byte)(val >> 24);
                last &= 0b11;
                last |= 0x14;

                *ptr = last;
            }
            return ret;
        }
    }

    /// <summary>
    /// arm64 远距离跳转
    /// </summary>
    public unsafe class CodePatcher_arm64_far : CodePatcher
    {
        private static readonly byte[] s_jmpCode = new byte[]    // 20 bytes(字节数过多，太危险了，不建议使用)
        {
            /*
             * ADR: https://developer.arm.com/documentation/ddi0596/2021-09/Base-Instructions/ADR--Form-PC-relative-address-
             * BR: https://developer.arm.com/documentation/ddi0596/2021-09/Base-Instructions/BR--Branch-to-Register-
             */
            0x6A, 0x00, 0x00, 0x10,                         // ADR X10, #C
            0x4A, 0x01, 0x40, 0xF9,                         // LDR X10, [X10,#0]
            0x40, 0x01, 0x1F, 0xD6,                         // BR X10
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00  // $dst
        };

        public CodePatcher_arm64_far(IntPtr target, IntPtr replace, IntPtr proxy, int jmpCodeSize) : base(target, replace, proxy, jmpCodeSize)
        {
        }

        protected override unsafe byte[] GenJmpCode(void* jmpFrom, void* jmpTo)
        {
            throw new NotImplementedException();
        }
    }
}