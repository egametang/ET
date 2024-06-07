using System.IO;

namespace Rider.Editor.Util
{
  internal static class RiderPathUtil
  {
    public static bool IsRiderDevEditor(string editorPath)
    {
      if (editorPath == null)
        return false;
      return "rider-dev".Equals(Path.GetFileNameWithoutExtension(editorPath));
    }
  }
}