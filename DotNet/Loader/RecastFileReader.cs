using System.IO;

namespace ET
{
    [Invoke]
    public class RecastFileReader: AInvokeHandler<NavmeshComponent.RecastFileLoader, byte[]>
    {
        public override byte[] Handle(NavmeshComponent.RecastFileLoader args)
        {
            return File.ReadAllBytes(Path.Combine("../Config/Recast", args.Name));
        }
    }
}