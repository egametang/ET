using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ET
{
    public static class GenLuaTool
    {
        [MenuItem("Tools/Gen Lua Scripts #S"),]
        public static void Geanerate()
        {
            Log.Debug("=========开始转换C#为lua，请耐心等待=========");

            if (!Directory.Exists("Assets/Bundles/Lua/Hotfix"))
            {
                Directory.CreateDirectory("Assets/Bundles/Lua/Hotfix");
            }

            if (!Directory.Exists("Assets/Bundles/Lua/HotfixView"))
            {
                Directory.CreateDirectory("Assets/Bundles/Lua/HotfixView");
            }

            if (!Directory.Exists("Assets/Bundles/Lua/Model"))
            {
                Directory.CreateDirectory("Assets/Bundles/Lua/Model");
            }

            if (!Directory.Exists("Assets/Bundles/Lua/ModelView"))
            {
                Directory.CreateDirectory("Assets/Bundles/Lua/ModelView");
            }

            if (!Directory.Exists("Assets/Res/Code"))
            {
                Directory.CreateDirectory("Assets/Res/Code");
            }

            FileHelper.CleanDirectory("Assets/Bundles/Lua/Hotfix");
            FileHelper.CleanDirectory("Assets/Bundles/Lua/HotfixView");
            FileHelper.CleanDirectory("Assets/Bundles/Lua/Model");
            FileHelper.CleanDirectory("Assets/Bundles/Lua/ModelView");
            FileHelper.CleanDirectory("Assets/Res/Code");
            
            GenLuaHelper.CopyDll("Unity.HotfixView");
            GenLuaHelper.CopyDll("Unity.Hotfix");
            GenLuaHelper.CopyDll("Unity.Model");
            GenLuaHelper.CopyDll("Unity.ModelView");
            
            GenLuaHelper.CompileLua(
                "Unity.Model", 
                "./Assets/Model", 
                "Model", 
                null,
                true);
            
            GenLuaHelper.CompileLua(
                "Unity.Hotfix", 
                "./Assets/Hotfix", 
                "Hotfix", 
                new List<string>() { "Unity.Model" }, 
                true);
            
            GenLuaHelper.CompileLua(
                "Unity.ModelView", 
                "./Assets/ModelView", 
                "ModelView", 
                new List<string>() { "Unity.Model", }, 
                true);
            
            GenLuaHelper.CompileLua(
                "Unity.HotfixView", 
                "./Assets/HotfixView", 
                "HotfixView", 
                new List<string>() { "Unity.Model", "Unity.Hotfix", "Unity.ModelView", },
                false);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Log.Debug("===============转换完成===============");
        }
    }
    
    
    public static class GenLuaHelper
    {
        public const string LuaDir = "./Lua/";
        public const string LuaTxtExtensionName = ".lua.txt";
        public const string LuaExtensionName = ".lua";

        private const string ScriptAssembliesDir = "Library/ScriptAssemblies";
        private const string CodeDir = "Assets/Res/Code/";

        public static bool CopyDll(string dllName)
        {
            var result = FileHelper.CopyFile(Path.Combine(ScriptAssembliesDir, $"{dllName}.dll"), Path.Combine(CodeDir, $"{dllName}.dll.bytes"),
                true);

            FileHelper.CopyFile(Path.Combine(ScriptAssembliesDir, $"{dllName}.pdb"), Path.Combine(CodeDir, $"{dllName}.pdb.bytes"), true);

            if (result)
            {
                Log.Info($"复制{dllName}.dll, {dllName}.pdb到Res/Code完成");
                AssetDatabase.Refresh();
            }

            return result;
        }

        public static void CompileLua(string dllName, string dllDir, string outDirName, List<string> referencedLuaAssemblies, bool isModule)
        {
            LuaCompiler.Compile(dllName, dllDir, outDirName, referencedLuaAssemblies, isModule);
            FileHelper.ReplaceExtensionName(LuaCompiler.outDir + outDirName, LuaExtensionName, LuaTxtExtensionName);
            ABNameEditor.SetFolderLuaABName(LuaCompiler.outDir + outDirName);
            AssetDatabase.Refresh();
        }

        //[MenuItem("Tools/Lua/Append the all lua file of the selected folder with \".txt\"")]
        public static void AppendSelectedFolder()
        {
            var assetGUIDs = Selection.assetGUIDs;

            foreach (var guid in assetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (Directory.Exists(assetPath))
                {
                    var outDirName = assetPath.Substring(assetPath.LastIndexOf("/") + 1);

                    FileHelper.ReplaceExtensionName(LuaCompiler.outDir + outDirName, LuaExtensionName, LuaTxtExtensionName);
                }
            }

            AssetDatabase.Refresh();
        }

        //[MenuItem("Tools/Lua/Replace the all \".lua.txt\" file of the selected folder with \".lua\"")]
        public static void ReplaceSelectedFolder()
        {
            var assetGUIDs = Selection.assetGUIDs;

            foreach (var guid in assetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (Directory.Exists(assetPath))
                {
                    var outDirName = assetPath.Substring(assetPath.LastIndexOf("/") + 1);

                    FileHelper.ReplaceExtensionName(LuaCompiler.outDir + outDirName, LuaTxtExtensionName, LuaExtensionName);
                }
            }

            AssetDatabase.Refresh();
        }
    }
}