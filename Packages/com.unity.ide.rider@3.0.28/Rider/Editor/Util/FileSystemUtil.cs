using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace Packages.Rider.Editor.Util
{
  internal static class FileSystemUtil
  {
    [NotNull]
    public static string GetFinalPathName([NotNull] string path)
    {
      if (path == null) throw new ArgumentNullException("path");

      // up to MAX_PATH. MAX_PATH on Linux currently 4096, on Mac OS X 1024
      // doc: http://man7.org/linux/man-pages/man3/realpath.3.html
      var sb = new StringBuilder(8192);
      var result = LibcNativeInterop.realpath(path, sb);
      if (result == IntPtr.Zero)
      {
        throw new Win32Exception($"{path} was not resolved.");
      }

      return new FileInfo(sb.ToString()).FullName;
    }

    public static string FileNameWithoutExtension(string path)
    {
      if (string.IsNullOrEmpty(path))
      {
        return "";
      }

      var indexOfDot = -1;
      var indexOfSlash = 0;
      for (var i = path.Length - 1; i >= 0; i--)
      {
        if (indexOfDot == -1 && path[i] == '.')
        {
          indexOfDot = i;
        }

        if (indexOfSlash == 0 && path[i] == '/' || path[i] == '\\')
        {
          indexOfSlash = i + 1;
          break;
        }
      }

      if (indexOfDot == -1)
      {
        indexOfDot = path.Length;
      }

      return path.Substring(indexOfSlash, indexOfDot - indexOfSlash);
    }
    
    public static bool EditorPathExists(string editorPath)
    {
      return SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX && new DirectoryInfo(editorPath).Exists 
             || SystemInfo.operatingSystemFamily != OperatingSystemFamily.MacOSX && new FileInfo(editorPath).Exists;
    }
  }
}