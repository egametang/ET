using System.IO;

namespace ET
{
    public static class RecastFileReader
    {
        public static byte[] Read(string name)
        {
            return File.ReadAllBytes(Path.Combine("../Config/Recast", name));
        }
    }
}