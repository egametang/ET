using System;
using System.IO;

namespace ET.Server
{
    [Invoke]
    public class RecastFileReader: AInvokeHandler<NavmeshComponent.RecastFileLoader, byte[]>
    {
        public override byte[] Handle(NavmeshComponent.RecastFileLoader args)
        {
            return File.ReadAllBytes(Path.Combine("Packages/cn.etetet.statesync/Config/Recast", args.Name));
        }
    }
}