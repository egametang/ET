using System;
using HybridCLR.Editor.Installer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using System.Reflection;
using HybridCLR.Editor.Settings;
#if UNITY_2019 && (UNITY_IOS || UNITY_TVOS)
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace HybridCLR.Editor.BuildProcessors
{
    public static class AddLil2cppSourceCodeToXcodeproj2019
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (!HybridCLRSettings.Instance.enable)
                return;
            /*
             *  1. 生成lump，并且添加到工程
                3. 将libil2cpp目录复制到 Library/. 删除旧的. search paths里修改 libil2cpp/include为libil2cpp
                3. Libraries/bdwgc/include -> Libraries/external/bdwgc/include
                4. 将external目录复制到 Library/external。删除旧目录
                5. 将Library/external/baselib/Platforms/OSX改名为 IOS 全大写
                6. 将 external/zlib下c 文件添加到工程
                7. 移除libil2cpp.a
                8. Include path add libil2cpp/os/ClassLibraryPAL/brotli/include
                9. add external/xxHash
                10. add "#include <stdio.h>" to Classes/Prefix.pch
             */
             
            string pbxprojFile = BuildProcessorUtil.GetXcodeProjectFile(pathToBuiltProject);
            string srcLibil2cppDir = $"{SettingsUtil.LocalIl2CppDir}/libil2cpp";
            string dstLibil2cppDir = $"{pathToBuiltProject}/Libraries/libil2cpp";
            string lumpDir = $"{pathToBuiltProject}/Libraries/lumps";
            string srcExternalDir = $"{SettingsUtil.LocalIl2CppDir}/external";
            string dstExternalDir = $"{pathToBuiltProject}/Libraries/external";
            //RemoveExternalLibil2cppOption(srcExternalDir, dstExternalDir);
            CopyLibil2cppToXcodeProj(srcLibil2cppDir, dstLibil2cppDir);
            CopyExternalToXcodeProj(srcExternalDir, dstExternalDir);
            var lumpFiles = CreateLumps(dstLibil2cppDir, lumpDir);
            var extraSources = GetExtraSourceFiles(dstExternalDir, dstLibil2cppDir);
            var cflags = new List<string>()
            {
                "-DIL2CPP_MONO_DEBUGGER_DISABLED",
            };
            ModifyPBXProject(pathToBuiltProject, pbxprojFile, lumpFiles, extraSources, cflags);
            AddSystemHeaderToPrefixPch(pathToBuiltProject);
        }

        private static void AddSystemHeaderToPrefixPch(string pathToBuiltProject)
        {
            // 如果不将 stdio.h 添加到 Prefix.pch, zutil.c会有编译错误
            string prefixPchFile = $"{pathToBuiltProject}/Classes/Prefix.pch";
            string fileContent = File.ReadAllText(prefixPchFile, Encoding.UTF8);
            if (!fileContent.Contains("stdio.h"))
            {
                string newFileContent = fileContent + "\n#include <stdio.h>\n";
                File.WriteAllText(prefixPchFile, newFileContent, Encoding.UTF8);
                UnityEngine.Debug.Log($"append header to {prefixPchFile}");
            }
        }

        private static string GetRelativePathFromProj(string path)
        {
            return path.Substring(path.IndexOf("Libraries", StringComparison.Ordinal)).Replace('\\', '/');
        }

        private static void ModifyPBXProject(string pathToBuiltProject, string pbxprojFile, List<LumpFile> lumpFiles, List<string> extraFiles, List<string> cflags)
        {
            var proj = new PBXProject();
            proj.ReadFromFile(pbxprojFile);
            string targetGUID = proj.GetUnityFrameworkTargetGuid();
            // 移除旧的libil2cpp.a
            var libil2cppGUID = proj.FindFileGuidByProjectPath("Libraries/libil2cpp.a");
            if (!string.IsNullOrEmpty(libil2cppGUID))
            {
                proj.RemoveFileFromBuild(targetGUID, libil2cppGUID);
                proj.RemoveFile(libil2cppGUID);
                File.Delete(Path.Combine(pathToBuiltProject, "Libraries", "libil2cpp.a"));
            }

            //var lumpGroupGuid = proj.AddFile("Lumps", $"Classes/Lumps", PBXSourceTree.Group);

            foreach (var lumpFile in lumpFiles)
            {
                string lumpFileName = Path.GetFileName(lumpFile.lumpFile);
                string projPathOfFile = $"Classes/Lumps/{lumpFileName}";
                string relativePathOfFile = GetRelativePathFromProj(lumpFile.lumpFile);
                string lumpGuid = proj.FindFileGuidByProjectPath(projPathOfFile);
                if (!string.IsNullOrEmpty(lumpGuid))
                {
                    proj.RemoveFileFromBuild(targetGUID, lumpGuid);
                    proj.RemoveFile(lumpGuid);
                }
                lumpGuid = proj.AddFile(relativePathOfFile, projPathOfFile, PBXSourceTree.Source);
                proj.AddFileToBuild(targetGUID, lumpGuid);
            }

            foreach (var extraFile in extraFiles)
            {
                string projPathOfFile = $"Classes/Extrals/{Path.GetFileName(extraFile)}";
                string extraFileGuid = proj.FindFileGuidByProjectPath(projPathOfFile);
                if (!string.IsNullOrEmpty(extraFileGuid))
                {
                    proj.RemoveFileFromBuild(targetGUID, extraFileGuid);
                    proj.RemoveFile(extraFileGuid);
                    //Debug.LogWarning($"remove exist extra file:{projPathOfFile} guid:{extraFileGuid}");
                }
                var lumpGuid = proj.AddFile(GetRelativePathFromProj(extraFile), projPathOfFile, PBXSourceTree.Source);
                proj.AddFileToBuild(targetGUID, lumpGuid);
            }

            foreach(var configName in proj.BuildConfigNames())
            {
                //Debug.Log($"build config:{bcn}");
                string configGuid = proj.BuildConfigByName(targetGUID, configName);
                string headerSearchPaths = "HEADER_SEARCH_PATHS";
                string hspProp = proj.GetBuildPropertyForConfig(configGuid, headerSearchPaths);
                //Debug.Log($"config guid:{configGuid} prop:{hspProp}");
                string newPro = hspProp.Replace("libil2cpp/include", "libil2cpp")
                    .Replace("Libraries/bdwgc", "Libraries/external/bdwgc");

                //if (!newPro.Contains("Libraries/libil2cpp/os/ClassLibraryPAL/brotli/include"))
                //{
                //    newPro += " $(SRCROOT)/Libraries/libil2cpp/os/ClassLibraryPAL/brotli/include";
                //}
                if (!newPro.Contains("Libraries/external/xxHash"))
                {
                    newPro += " $(SRCROOT)/Libraries/external/xxHash";
                }
                newPro += " $(SRCR00T)/Libraries/external/mono";
                //Debug.Log($"config:{bcn} new prop:{newPro}");
                proj.SetBuildPropertyForConfig(configGuid, headerSearchPaths, newPro);

                string cflagKey = "OTHER_CFLAGS";
                string cfProp = proj.GetBuildPropertyForConfig(configGuid, cflagKey);
                foreach (var flag in cflags)
                {
                    if (!cfProp.Contains(flag))
                    {
                        cfProp += " " + flag;
                    }
                }
                if (configName.Contains("Debug") && !cfProp.Contains("-DIL2CPP_DEBUG="))
                {
                    cfProp += " -DIL2CPP_DEBUG=1 -DDEBUG=1";
                }
                proj.SetBuildPropertyForConfig(configGuid, cflagKey, cfProp);

            }
            proj.WriteToFile(pbxprojFile);
        }

        private static void CopyLibil2cppToXcodeProj(string srcLibil2cppDir, string dstLibil2cppDir)
        {
            BashUtil.RemoveDir(dstLibil2cppDir);
            BashUtil.CopyDir(srcLibil2cppDir, dstLibil2cppDir, true);
        }


        private static void CopyExternalToXcodeProj(string srcExternalDir, string dstExternalDir)
        {
            BashUtil.RemoveDir(dstExternalDir);
            BashUtil.CopyDir(srcExternalDir, dstExternalDir, true);

            //string baselibPlatfromsDir = $"{dstExternalDir}/baselib/Platforms";
            //BashUtil.RemoveDir($"{baselibPlatfromsDir}/IOS");
            //BashUtil.CopyDir($"{baselibPlatfromsDir}/OSX", $"{baselibPlatfromsDir}/IOS", true);
        }

        class LumpFile
        {
            public List<string> cppFiles = new List<string>();

            public readonly string lumpFile;

            public readonly string il2cppConfigFile;

            public LumpFile(string lumpFile, string il2cppConfigFile)
            {
                this.lumpFile = lumpFile;
                this.il2cppConfigFile = il2cppConfigFile;
                this.cppFiles.Add(il2cppConfigFile);
            }

            public void SaveFile()
            {
                var lumpFileContent = new List<string>();
                foreach (var file in cppFiles)
                {
                    lumpFileContent.Add($"#include \"{GetRelativePathFromProj(file)}\"");
                }
                File.WriteAllLines(lumpFile, lumpFileContent, Encoding.UTF8);
                Debug.Log($"create lump file:{lumpFile}");
            }
        }

        private static List<LumpFile> CreateLumps(string libil2cppDir, string outputDir)
        {
            BashUtil.RecreateDir(outputDir);

            string il2cppConfigFile = $"{libil2cppDir}/il2cpp-config.h";
            var lumpFiles = new List<LumpFile>();
            int lumpFileIndex = 0;
            foreach (var cppDir in Directory.GetDirectories(libil2cppDir, "*", SearchOption.AllDirectories).Concat(new string[] {libil2cppDir}))
            {
                var lumpFile = new LumpFile($"{outputDir}/lump_{Path.GetFileName(cppDir)}_{lumpFileIndex}.cpp", il2cppConfigFile);
                foreach (var file in Directory.GetFiles(cppDir, "*.cpp", SearchOption.TopDirectoryOnly))
                {
                    lumpFile.cppFiles.Add(file);
                }
                lumpFile.SaveFile();
                lumpFiles.Add(lumpFile);
                ++lumpFileIndex;
            }

            var mmFiles = Directory.GetFiles(libil2cppDir, "*.mm", SearchOption.AllDirectories);
            if (mmFiles.Length > 0)
            {
                var lumpFile = new LumpFile($"{outputDir}/lump_mm.mm", il2cppConfigFile);
                foreach (var file in mmFiles)
                {
                    lumpFile.cppFiles.Add(file);
                }
                lumpFile.SaveFile();
                lumpFiles.Add(lumpFile);
            }
            return lumpFiles;
        }

        private static List<string> GetExtraSourceFiles(string externalDir, string libil2cppDir)
        {
            var files = new List<string>();
            foreach (string extraDir in new string[]
            {
                $"{externalDir}/zlib",
                $"{externalDir}/xxHash",
                $"{libil2cppDir}/os/ClassLibraryPAL/brotli",
            })
            {
                if (!Directory.Exists(extraDir))
                {
                    continue;
                }
                files.AddRange(Directory.GetFiles(extraDir, "*.c", SearchOption.AllDirectories));
            }
            return files;
        }
    }
}
#endif