using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class LuaCompiler
    {
        private sealed class CompiledFail: Exception
        {
            public CompiledFail(string message): base(message)
            {
            }
        }

        public static readonly string outDir = Application.dataPath + "/Bundles/Lua/";

        private const string dotnet = "dotnet";
        private static readonly string toolsDir = "./Tools/CSharpLua";
        private static readonly string csharpLua = toolsDir + "/CSharp.lua/CSharp.lua.Launcher.dll";

        /// <summary>
        /// 编译Dll为Lua
        /// </summary>
        /// <param name="dllName">Dll名</param>
        /// <param name="dllDir">Dll文件夹</param>
        /// <param name="outDirName">输出Lua文件夹名</param>
        /// <param name="referencedLuaAssemblies">需要被编译成Lua的Dll引用名</param>
        /// <param name="isModule">是否其他模块被引用</param>
        public static void Compile(string dllName, string dllDir, string outDirName, List<string> referencedLuaAssemblies, bool isModule)
        {
            if (!CheckDotnetInstall())
            {
                return;
            }

            if (!File.Exists(csharpLua))
            {
                throw new InvalidProgramException($"{csharpLua} not found");
            }

            var outputDir = outDir + outDirName;

            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }

            HashSet<string> libs = new HashSet<string>();
            FillUnityLibraries(libs);
            AssemblyName assemblyName = new AssemblyName(dllName);
            Assembly assembly = Assembly.Load(assemblyName);
            foreach (var referenced in assembly.GetReferencedAssemblies())
            {
                if (referenced.Name != "mscorlib" && !referenced.Name.StartsWith("System"))
                {
                    string libPath = Assembly.Load(referenced).Location;

                    if (referencedLuaAssemblies != null && referencedLuaAssemblies.Contains(referenced.Name))
                    {
                        libPath += "!";
                    }

                    libs.Add(libPath);
                }
            }

            string lib = string.Join(";", libs.ToArray());
            string args = $"{csharpLua}  -s \"{dllDir}\" -d \"{outputDir}\" -l \"{lib}\" -m -c -a -e -ei";
            UnityEngine.Debug.Log(args);
            if (isModule)
            {
                args += " -module";
            }

            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!string.IsNullOrEmpty(definesString))
            {
                args += $" -csc -define:{definesString}";
            }

            var info = new ProcessStartInfo()
            {
                FileName = dotnet,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow = true
            };
            using (var p = Process.Start(info))
            {
                p.WaitForExit();
                if (p.ExitCode == 0)
                {
                    UnityEngine.Debug.Log("compile success");
                }
                else
                {
                    string outString = p.StandardOutput.ReadToEnd();
                    string errorString = p.StandardError.ReadToEnd();
                    throw new CompiledFail($"Compile fail, {errorString}\n{outString}\n{dotnet} {args}");
                }
            }
        }

        private static void FillUnityLibraries(HashSet<string> libs)
        {
            string unityObjectPath = typeof (UnityEngine.Object).Assembly.Location;
            string baseDir = Path.GetDirectoryName(unityObjectPath);
            foreach (string path in Directory.EnumerateFiles(baseDir, "*.dll"))
            {
                libs.Add(path);
            }
        }

        private static bool CheckDotnetInstall()
        {
            bool has = InternalCheckDotnetInstall();
            if (!has)
            {
                UnityEngine.Debug.LogWarning("not found dotnet");
                if (EditorUtility.DisplayDialog("dotnet未安装", "未安装.NET Core 3.0+运行环境，点击确定前往安装", "确定", "取消"))
                {
                    Application.OpenURL("https://www.microsoft.com/net/download");
                }
            }

            return has;
        }

        private static bool InternalCheckDotnetInstall()
        {
            var info = new ProcessStartInfo()
            {
                FileName = dotnet,
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            try
            {
                using (var p = Process.Start(info))
                {
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        string version = p.StandardOutput.ReadToEnd();
                        UnityEngine.Debug.LogFormat("found dotnet {0}", version);
                        int major = version[0] - '0';
                        if (major >= 3)
                        {
                            return true;
                        }
                        else
                        {
                            UnityEngine.Debug.LogErrorFormat("dotnet verson {0} must >= 3.0", version);
                        }
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return false;
            }
        }
    }
}