using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace HybridCLR
{
    [Preserve]
    public static class RuntimeApi
    {
        /// <summary>
        /// 加载补充元数据assembly
        /// </summary>
        /// <param name="dllBytes"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
#if UNITY_EDITOR
        public static unsafe LoadImageErrorCode LoadMetadataForAOTAssembly(byte[] dllBytes, HomologousImageMode mode)
        {
            return LoadImageErrorCode.OK;
        }
#else
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern LoadImageErrorCode LoadMetadataForAOTAssembly(byte[] dllBytes, HomologousImageMode mode);
#endif

        /// <summary>
        /// 获取解释器线程栈的最大StackObject个数(size*8 为最终占用的内存大小)
        /// </summary>
        /// <returns></returns>
        public static int GetInterpreterThreadObjectStackSize()
        {
            return GetRuntimeOption(RuntimeOptionId.InterpreterThreadObjectStackSize);
        }

        /// <summary>
        /// 设置解释器线程栈的最大StackObject个数(size*8 为最终占用的内存大小)
        /// </summary>
        /// <param name="size"></param>
        public static void SetInterpreterThreadObjectStackSize(int size)
        {
            SetRuntimeOption(RuntimeOptionId.InterpreterThreadObjectStackSize, size);
        }
        

        /// <summary>
        /// 获取解释器线程函数帧数量(sizeof(InterpreterFrame)*size 为最终占用的内存大小)
        /// </summary>
        /// <returns></returns>
        public static int GetInterpreterThreadFrameStackSize()
        {
            return GetRuntimeOption(RuntimeOptionId.InterpreterThreadFrameStackSize);
        }

        /// <summary>
        /// 设置解释器线程函数帧数量(sizeof(InterpreterFrame)*size 为最终占用的内存大小)
        /// </summary>
        /// <param name="size"></param>
        public static void SetInterpreterThreadFrameStackSize(int size)
        {
            SetRuntimeOption(RuntimeOptionId.InterpreterThreadFrameStackSize, size);
        }


#if UNITY_EDITOR

        private static readonly Dictionary<RuntimeOptionId, int> s_runtimeOptions = new Dictionary<RuntimeOptionId, int>();

        public static void SetRuntimeOption(RuntimeOptionId optionId, int value)
        {
            s_runtimeOptions[optionId] = value;
        }
#else
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetRuntimeOption(RuntimeOptionId optionId, int value);
#endif

#if UNITY_EDITOR
        public static int GetRuntimeOption(RuntimeOptionId optionId)
        {
            if (s_runtimeOptions.TryGetValue(optionId, out var value))
            {
                return value;
            }
            return 0;
        }
#else
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetRuntimeOption(RuntimeOptionId optionId);
#endif
    }
}
