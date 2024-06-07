namespace Packages.Rider.Editor.ProjectGeneration
{
  internal interface IGUIDGenerator
  {
    string ProjectGuid(string name);
  }
}