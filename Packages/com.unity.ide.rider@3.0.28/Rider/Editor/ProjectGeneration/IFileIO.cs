using System.IO;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal interface IFileIO
  {
    bool Exists(string path);

    TextReader GetReader(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string content);

    // rootDirectoryFullPath is assumed to be the result of Path.GetFullPath
    // Passing the directory with a trailing slash (Path.DirectorySeparatorChar) will avoid an allocation
    string EscapedRelativePathFor(string path, string rootDirectoryFullPath);
  }
}