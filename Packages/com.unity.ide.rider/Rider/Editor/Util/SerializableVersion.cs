using System;

namespace Packages.Rider.Editor.Util
{
  [Serializable]
  internal class SerializableVersion
  {
    public SerializableVersion(Version version)
    {

      Major = version.Major;
      Minor = version.Minor;
      if (version.Build >= 0)
        Build = version.Build;
      if (version.Revision >= 0)
        Revision = version.Revision;
    }

    public int Build;
    public int Major;
    public int Minor;
    public int Revision;
  }

  internal static class VersionExtension
  {
    public static SerializableVersion ToSerializableVersion(this Version version)
    {
      if (version == null)
        return null;
        
      return new SerializableVersion(version);
    }

    public static Version ToVersion(this SerializableVersion serializableVersion)
    {
      if (serializableVersion == null)
        return null;

      var build = serializableVersion.Build;
      if (build < 0)
          build = 0;
      var revision = serializableVersion.Revision;
      if (revision < 0)
          revision = 0;
      
      return new Version(serializableVersion.Major, serializableVersion.Minor, build,
          revision);
    }
  }
}