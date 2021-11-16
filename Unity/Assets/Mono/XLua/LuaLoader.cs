using System.IO;
using UnityEngine;

namespace ET
{
    public static class LuaLoader
    {
        public const string manifestFileName = "manifest.lua";
        private const string luaExtensionName = ".lua";
        private const string txtExtensionName = ".txt";
        private const string luaTxtExtensionName = ".lua.txt";
        private const string luaDir = "Bundles/Lua/";
        private const string luaSuffixName = "/lua";
        private const char dot = '.';
        private const char backSlash = '/';

        public static byte[] Load(ref string filepath)
        {
            if (Define.IsEditor)
            {
                if (string.IsNullOrWhiteSpace(filepath))
                {
                    return null;
                }

                if (filepath.EndsWith(luaExtensionName))
                {
                    filepath = Path.Combine(UnityEngine.Application.dataPath,
                        luaDir + filepath.Replace(dot, backSlash).Replace(luaSuffixName, luaExtensionName) + txtExtensionName);

                    if (File.Exists(filepath))
                    {
                        return File.ReadAllBytes(filepath);
                    }
                }
                else
                {
                    filepath = Path.Combine(UnityEngine.Application.dataPath, luaDir + filepath.Replace(dot, backSlash) + luaTxtExtensionName);

                    if (File.Exists(filepath))
                    {
                        return File.ReadAllBytes(filepath);
                    }
                }
            }
            else
            {
                // 打包加载
            }

            return null;
        }
    }
}