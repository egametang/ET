using System.IO;

namespace ET.Server
{
    [Callback]
    public class RecastFileReader: ACallbackHandler<NavmeshComponent.RecastFileLoader, byte[]>
    {
        public override byte[] Handle(NavmeshComponent.RecastFileLoader args)
        {
            return File.ReadAllBytes(Path.Combine("../Config/Recast", args.Name));
        }
    }
}