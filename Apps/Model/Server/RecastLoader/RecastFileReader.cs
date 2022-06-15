using System.IO;

namespace ET.Server
{
    [Callback(CallbackType.RecastFileLoader)]
    public class RecastFileReader: IFunc<string, byte[]>
    {
        public byte[] Handle(string name)
        {
            return File.ReadAllBytes(Path.Combine("../Config/Recast", name));
        }
    }
}