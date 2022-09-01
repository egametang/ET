using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR
{
    public static class RuntimeApi
    {
#if UNITY_STANDALONE_WIN
        private const string dllName = "GameAssembly";
#elif UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_WEBGL
    private const string dllName = "__Internal";
#else
    private const string dllName = "il2cpp";
#endif

        [DllImport(dllName, EntryPoint = "RuntimeApi_LoadMetadataForAOTAssembly")]
        public static extern int LoadMetadataForAOTAssembly(IntPtr dllBytes, int dllSize);

        [DllImport(dllName, EntryPoint = "RuntimeApi_GetInterpreterThreadObjectStackSize")]
        public static extern int GetInterpreterThreadObjectStackSize();

        [DllImport(dllName, EntryPoint = "RuntimeApi_SetInterpreterThreadObjectStackSize")]
        public static extern void SetInterpreterThreadObjectStackSize(int size);

        [DllImport(dllName, EntryPoint = "RuntimeApi_GetInterpreterThreadFrameStackSize")]
        public static extern int GetInterpreterThreadFrameStackSize();

        [DllImport(dllName, EntryPoint = "RuntimeApi_SetInterpreterThreadFrameStackSize")]
        public static extern void SetInterpreterThreadFrameStackSize(int size);
    }
}
