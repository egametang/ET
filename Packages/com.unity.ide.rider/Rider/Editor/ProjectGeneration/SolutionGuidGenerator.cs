using System;
using System.Security.Cryptography;
using System.Text;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal static class SolutionGuidGenerator
  {
    public static string GuidForProject(string projectName)
    {
      return ComputeGuidHashFor(projectName + "salt");
    }

    private static string ComputeGuidHashFor(string input)
    {
      using (var md5 = MD5.Create())
      {
        var hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
        return new Guid(hash).ToString();
      }
    }
  }
}