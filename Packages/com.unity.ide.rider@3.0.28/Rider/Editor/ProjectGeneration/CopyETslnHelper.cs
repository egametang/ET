using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ET
{
  public static class CopyETslnHelper
  {
    public static void Run()
    {
      string etslnPath = Path.Combine(Directory.GetCurrentDirectory(), "ET.sln");
      if (File.Exists(etslnPath))
      {
        return;
      }
            
      List<string> slns = new List<string>();
      FileHelper.GetAllFiles(slns, "./Packages", "ET.sln");

      if (slns.Count == 0)
      {
        return;
      }

      string sourcePath = slns[0];

      File.Copy(sourcePath, etslnPath, true);
    }
  }
}