using System;

namespace Packages.Rider.Editor.ProjectGeneration
{
  [Flags]
  enum ProjectGenerationFlag
  {
    None = 0,
    Embedded = 1,
    Local = 2,
    Registry = 4,
    Git = 8,
    BuiltIn = 16,
    Unknown = 32,
    PlayerAssemblies = 64,
    LocalTarBall = 128,
  }
}
