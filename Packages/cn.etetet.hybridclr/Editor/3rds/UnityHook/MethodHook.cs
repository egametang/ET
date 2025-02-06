/*
 Desc: 一个可以运行时 Hook Mono 方法的工具，让你可以无需修改 UnityEditor.dll 等文件就可以重写其函数功能
 Author: Misaka Mikoto
 Github: https://github.com/Misaka-Mikoto-Tech/MonoHook
 */

using DotNetDetour;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Runtime.CompilerServices;


/*
>>>>>>> 原始 UnityEditor.LogEntries.Clear 一型(.net 4.x)
0000000000403A00 < | 55                                 | push rbp                                     |
0000000000403A01   | 48 8B EC                           | mov rbp,rsp                                  |
0000000000403A04   | 48 81 EC 80 00 00 00               | sub rsp,80                                   |
0000000000403A0B   | 48 89 65 B0                        | mov qword ptr ss:[rbp-50],rsp                |
0000000000403A0F   | 48 89 6D A8                        | mov qword ptr ss:[rbp-58],rbp                |
0000000000403A13   | 48 89 5D C8                        | mov qword ptr ss:[rbp-38],rbx                | <<
0000000000403A17   | 48 89 75 D0                        | mov qword ptr ss:[rbp-30],rsi                |
0000000000403A1B   | 48 89 7D D8                        | mov qword ptr ss:[rbp-28],rdi                |
0000000000403A1F   | 4C 89 65 E0                        | mov qword ptr ss:[rbp-20],r12                |
0000000000403A23   | 4C 89 6D E8                        | mov qword ptr ss:[rbp-18],r13                |
0000000000403A27   | 4C 89 75 F0                        | mov qword ptr ss:[rbp-10],r14                |
0000000000403A2B   | 4C 89 7D F8                        | mov qword ptr ss:[rbp-8],r15                 |
0000000000403A2F   | 49 BB 00 2D 1E 1A FE 7F 00 00      | mov r11,7FFE1A1E2D00                         |
0000000000403A39   | 4C 89 5D B8                        | mov qword ptr ss:[rbp-48],r11                |
0000000000403A3D   | 49 BB 08 2D 1E 1A FE 7F 00 00      | mov r11,7FFE1A1E2D08                         |


>>>>>>> 二型(.net 2.x)
0000000000403E8F   | 55                                 | push rbp                                     |
0000000000403E90   | 48 8B EC                           | mov rbp,rsp                                  |
0000000000403E93   | 48 83 EC 70                        | sub rsp,70                                   |
0000000000403E97   | 48 89 65 C8                        | mov qword ptr ss:[rbp-38],rsp                |
0000000000403E9B   | 48 89 5D B8                        | mov qword ptr ss:[rbp-48],rbx                |
0000000000403E9F   | 48 89 6D C0                        | mov qword ptr ss:[rbp-40],rbp                | <<(16)
0000000000403EA3   | 48 89 75 F8                        | mov qword ptr ss:[rbp-8],rsi                 |
0000000000403EA7   | 48 89 7D F0                        | mov qword ptr ss:[rbp-10],rdi                |
0000000000403EAB   | 4C 89 65 D0                        | mov qword ptr ss:[rbp-30],r12                |
0000000000403EAF   | 4C 89 6D D8                        | mov qword ptr ss:[rbp-28],r13                |
0000000000403EB3   | 4C 89 75 E0                        | mov qword ptr ss:[rbp-20],r14                |
0000000000403EB7   | 4C 89 7D E8                        | mov qword ptr ss:[rbp-18],r15                |
0000000000403EBB   | 48 83 EC 20                        | sub rsp,20                                   |
0000000000403EBF   | 49 BB 18 3F 15 13 FE 7F 00 00      | mov r11,7FFE13153F18                         |
0000000000403EC9   | 41 FF D3                           | call r11                                     |
0000000000403ECC   | 48 83 C4 20                        | add rsp,20                                   |

>>>>>>>>> arm64
il2cpp:00000000003DE714 F5 0F 1D F8                             STR             X21, [SP,#-0x10+var_20]!                            |  << absolute safe
il2cpp:00000000003DE718 F4 4F 01 A9                             STP             X20, X19, [SP,#0x20+var_10]                         |  << may be safe
il2cpp:00000000003DE71C FD 7B 02 A9                             STP             X29, X30, [SP,#0x20+var_s0]                         |
il2cpp:00000000003DE720 FD 83 00 91                             ADD             X29, SP, #0x20                                      |
il2cpp:00000000003DE724 B5 30 00 B0                             ADRP            X21, #_ZZ62GameObject_SetActive_mCF1EEF2A314F3AE    |  << dangerous: relative instruction, can not be overwritten
il2cpp:00000000003DE728 A2 56 47 F9                             LDR             method, [X21,#_ZZ62GameObject_SetActive_mCF] ;      |
il2cpp:00000000003DE72C F3 03 01 2A                             MOV             W19, W1                                             |
 */

namespace MonoHook
{
    /// <summary>
    /// Hook 类，用来 Hook 某个 C# 方法
    /// </summary>
    public unsafe class MethodHook
    {
        public string tag;
        public bool isHooked { get; private set; }
        public bool isPlayModeHook { get; private set; }

        public MethodBase targetMethod { get; private set; }       // 需要被hook的目标方法
        public MethodBase replacementMethod { get; private set; }  // 被hook后的替代方法
        public MethodBase proxyMethod { get; private set; }        // 目标方法的代理方法(可以通过此方法调用被hook后的原方法)

        private IntPtr _targetPtr;          // 目标方法被 jit 后的地址指针
        private IntPtr _replacementPtr;
        private IntPtr _proxyPtr;

        private CodePatcher _codePatcher;

#if UNITY_EDITOR && !UNITY_2020_3_OR_NEWER
        /// <summary>
        /// call `MethodInfo.MethodHandle.GetFunctionPointer()` 
        /// will visit static class `UnityEditor.IMGUI.Controls.TreeViewGUI.Styles` and invoke its static constructor,
        /// and init static filed `foldout`, but `GUISKin.current` is null now,
        /// so we should wait until `GUISKin.current` has a valid value
        /// </summary>
        private static FieldInfo s_fi_GUISkin_current;
#endif

        static MethodHook()
        {
#if UNITY_EDITOR && !UNITY_2020_3_OR_NEWER
            s_fi_GUISkin_current = typeof(GUISkin).GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
#endif
        }

        /// <summary>
        /// 创建一个 Hook
        /// </summary>
        /// <param name="targetMethod">需要替换的目标方法</param>
        /// <param name="replacementMethod">准备好的替换方法</param>
        /// <param name="proxyMethod">如果还需要调用原始目标方法，可以通过此参数的方法调用，如果不需要可以填 null</param>
        public MethodHook(MethodBase targetMethod, MethodBase replacementMethod, MethodBase proxyMethod, string data = "")
        {
            this.targetMethod       = targetMethod;
            this.replacementMethod  = replacementMethod;
            this.proxyMethod        = proxyMethod;
            this.tag = data;

            CheckMethod();
        }

        public void Install()
        {
            if (LDasm.IsiOS()) // iOS 不支持修改 code 所在区域 page
                return;

            if (isHooked)
                return;

#if UNITY_EDITOR && !UNITY_2020_3_OR_NEWER 
            if (s_fi_GUISkin_current.GetValue(null) != null)
                DoInstall();
            else
                EditorApplication.update += OnEditorUpdate;
#else
            DoInstall();
#endif
            isPlayModeHook = Application.isPlaying;
        }

        public void Uninstall()
        {
            if (!isHooked)
                return;

            _codePatcher.RemovePatch();

            isHooked = false;
            HookPool.RemoveHooker(targetMethod);
        }

        #region private
        private void DoInstall()
        {
            if (targetMethod == null || replacementMethod == null)
                throw new Exception("none of methods targetMethod or replacementMethod can be null");

            HookPool.AddHook(targetMethod, this);

            if (_codePatcher == null)
            {
                if (GetFunctionAddr())
                {
#if ENABLE_HOOK_DEBUG
                    UnityEngine.Debug.Log($"Original [{targetMethod.DeclaringType.Name}.{targetMethod.Name}]: {HookUtils.HexToString(_targetPtr.ToPointer(), 64, -16)}");
                    UnityEngine.Debug.Log($"Original [{replacementMethod.DeclaringType.Name}.{replacementMethod.Name}]: {HookUtils.HexToString(_replacementPtr.ToPointer(), 64, -16)}");
                    if(proxyMethod != null)
                        UnityEngine.Debug.Log($"Original [{proxyMethod.DeclaringType.Name}.{proxyMethod.Name}]: {HookUtils.HexToString(_proxyPtr.ToPointer(), 64, -16)}");
#endif

                    CreateCodePatcher();
                    _codePatcher.ApplyPatch();

#if ENABLE_HOOK_DEBUG
                    UnityEngine.Debug.Log($"New [{targetMethod.DeclaringType.Name}.{targetMethod.Name}]: {HookUtils.HexToString(_targetPtr.ToPointer(), 64, -16)}");
                    UnityEngine.Debug.Log($"New [{replacementMethod.DeclaringType.Name}.{replacementMethod.Name}]: {HookUtils.HexToString(_replacementPtr.ToPointer(), 64, -16)}");
                    if(proxyMethod != null)
                        UnityEngine.Debug.Log($"New [{proxyMethod.DeclaringType.Name}.{proxyMethod.Name}]: {HookUtils.HexToString(_proxyPtr.ToPointer(), 64, -16)}");
#endif
                }
            }

            isHooked = true;
        }

        private void CheckMethod()
        {
            if (targetMethod == null || replacementMethod == null)
                throw new Exception("MethodHook:targetMethod and replacementMethod and proxyMethod can not be null");

            string methodName = $"{targetMethod.DeclaringType.Name}.{targetMethod.Name}";
            if (targetMethod.IsAbstract)
                throw new Exception($"WRANING: you can not hook abstract method [{methodName}]");

#if UNITY_EDITOR && !UNITY_2020_3_OR_NEWER
            int minMethodBodySize = 10;

            {
                if ((targetMethod.MethodImplementationFlags & MethodImplAttributes.InternalCall) != MethodImplAttributes.InternalCall)
                {
                    int codeSize = targetMethod.GetMethodBody().GetILAsByteArray().Length; // GetMethodBody can not call on il2cpp
                    if (codeSize < minMethodBodySize)
                        UnityEngine.Debug.LogWarning($"WRANING: you can not hook method [{methodName}], cause its method body is too short({codeSize}), will random crash on IL2CPP release mode");
                }
            }

            if(proxyMethod != null)
            {
                methodName = $"{proxyMethod.DeclaringType.Name}.{proxyMethod.Name}";
                int codeSize = proxyMethod.GetMethodBody().GetILAsByteArray().Length;
                if (codeSize < minMethodBodySize)
                    UnityEngine.Debug.LogWarning($"WRANING: size of method body[{methodName}] is too short({codeSize}), will random crash on IL2CPP release mode, please fill some dummy code inside");

                if ((proxyMethod.MethodImplementationFlags & MethodImplAttributes.NoOptimization) != MethodImplAttributes.NoOptimization)
                    throw new Exception($"WRANING: method [{methodName}] must has a Attribute `MethodImpl(MethodImplOptions.NoOptimization)` to prevent code call to this optimized by compiler(pass args by shared stack)");
            }
#endif
        }

        private void CreateCodePatcher()
        {
            long addrOffset = Math.Abs(_targetPtr.ToInt64() - _proxyPtr.ToInt64());
            
            if(_proxyPtr != IntPtr.Zero)
                addrOffset = Math.Max(addrOffset, Math.Abs(_targetPtr.ToInt64() - _proxyPtr.ToInt64()));

            if (LDasm.IsARM())
            {
                if (IntPtr.Size == 8)
                    _codePatcher = new CodePatcher_arm64_near(_targetPtr, _replacementPtr, _proxyPtr);
                else if (addrOffset < ((1 << 25) - 1))
                    _codePatcher = new CodePatcher_arm32_near(_targetPtr, _replacementPtr, _proxyPtr);
                else if (addrOffset < ((1 << 27) - 1))
                    _codePatcher = new CodePatcher_arm32_far(_targetPtr, _replacementPtr, _proxyPtr);
                else
                    throw new Exception("address of target method and replacement method are too far, can not hook");
            }
            else
            {
                if (IntPtr.Size == 8)
                {
                    if(addrOffset < 0x7fffffff) // 2G
                        _codePatcher = new CodePatcher_x64_near(_targetPtr, _replacementPtr, _proxyPtr);
                    else
                        _codePatcher = new CodePatcher_x64_far(_targetPtr, _replacementPtr, _proxyPtr);
                }
                else
                    _codePatcher = new CodePatcher_x86(_targetPtr, _replacementPtr, _proxyPtr);
            }
        }

        /// <summary>
        /// 获取对应函数jit后的native code的地址
        /// </summary>
        private bool GetFunctionAddr()
        {
            _targetPtr = GetFunctionAddr(targetMethod);
            _replacementPtr = GetFunctionAddr(replacementMethod);
            _proxyPtr = GetFunctionAddr(proxyMethod);

            if (_targetPtr == IntPtr.Zero || _replacementPtr == IntPtr.Zero)
                return false;

            if (proxyMethod != null && _proxyPtr == null)
                return false;

            if(_replacementPtr == _targetPtr)
            {
                throw new Exception($"the addresses of target method {targetMethod.Name} and replacement method {replacementMethod.Name} can not be same");
            }

            if (LDasm.IsThumb(_targetPtr) || LDasm.IsThumb(_replacementPtr))
            {
                throw new Exception("does not support thumb arch");
            }

            return true;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)] // 好像在 IL2CPP 里无效
        private struct __ForCopy
        {
            public long __dummy;
            public MethodBase method;
        }
        /// <summary>
        /// 获取方法指令地址
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private IntPtr GetFunctionAddr(MethodBase method)
        {
            if (method == null)
                return IntPtr.Zero;

            if (!LDasm.IsIL2CPP())
                return method.MethodHandle.GetFunctionPointer();
            else
            {
                /*
                    // System.Reflection.MonoMethod
                    typedef struct Il2CppReflectionMethod
                    {
                        Il2CppObject object;
                        const MethodInfo *method;
                        Il2CppString *name;
                        Il2CppReflectionType *reftype;
                    } Il2CppReflectionMethod;

                    typedef Il2CppClass Il2CppVTable;
                    typedef struct Il2CppObject
                    {
                        union
                        {
                            Il2CppClass *klass;
                            Il2CppVTable *vtable;
                        };
                        MonitorData *monitor;
                    } Il2CppObject;

                typedef struct MethodInfo
                {
                    Il2CppMethodPointer methodPointer; // this is the pointer to native code of method
                    InvokerMethod invoker_method;
                    const char* name;
                    Il2CppClass *klass;
                    const Il2CppType *return_type;
                    const ParameterInfo* parameters;
                // ...
                }
                 */

                __ForCopy __forCopy = new __ForCopy() { method = method };

                long* ptr = &__forCopy.__dummy;
                ptr++; // addr of _forCopy.method

                IntPtr methodAddr = IntPtr.Zero;
                if (sizeof(IntPtr) == 8)
                {
                    long methodDataAddr = *(long*)ptr;
                    byte* ptrData = (byte*)methodDataAddr + sizeof(IntPtr) * 2; // offset of Il2CppReflectionMethod::const MethodInfo *method;

                    long methodPtr = 0;
                    methodPtr = *(long*)ptrData;
                    methodAddr = new IntPtr(*(long*)methodPtr); // MethodInfo::Il2CppMethodPointer methodPointer;
                }
                else
                {
                    int methodDataAddr = *(int*)ptr;
                    byte* ptrData = (byte*)methodDataAddr + sizeof(IntPtr) * 2; // offset of Il2CppReflectionMethod::const MethodInfo *method;

                    int methodPtr = 0;
                    methodPtr = *(int*)ptrData;
                    methodAddr = new IntPtr(*(int*)methodPtr);
                }
                return methodAddr;
            }
        }

#if UNITY_EDITOR && !UNITY_2020_3_OR_NEWER
        private void OnEditorUpdate()
        {
            if (s_fi_GUISkin_current.GetValue(null) != null)
            {
                try
                {
                    DoInstall();
                }
                finally
                {
                    EditorApplication.update -= OnEditorUpdate;
                }
            }
        }
#endif

        #endregion
    }

}
