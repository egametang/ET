using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace ET
{
    public static class LinkSlnHelper
    {
        [MenuItem("ET/LinkSln")]
        public static void Run()
        {
            string etslnPath = Path.Combine(Directory.GetCurrentDirectory(), "ET.sln");
            if (File.Exists(etslnPath))
            {
                File.Delete(etslnPath);
            }
            
            List<string> slns = new List<string>();
            FileHelper.GetAllFiles(slns, "./Packages", "ET.sln");

            if (slns.Count == 0)
            {
                throw new Exception("not found ET.sln in et packages!");
            }
            
#if UNITY_EDITOR_WIN
            Process process = ProcessHelper.Run("powershell.exe", $"New-Item -ItemType HardLink -Target {slns[0]} ./ET.sln", waitExit: true);
#else
            Process process = ProcessHelper.Run("pwsh", $"New-Item -ItemType HardLink -Target {slns[0]} ./ET.sln", waitExit: true);
#endif
            UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
        }
    }
}