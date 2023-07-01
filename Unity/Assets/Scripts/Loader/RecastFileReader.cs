using System;
using System.IO;

namespace ET
{
    [Invoke]
    public class RecastFileReader: AInvokeHandler<NavmeshComponent.RecastFileLoader, byte[]>
    {
        public override byte[] Handle(NavmeshComponent.RecastFileLoader args)
        {
            if (Define.IsEditor)
            {
                return File.ReadAllBytes(Path.Combine("../Config/Recast", args.Name));
            }

            throw new Exception("not load");
        }
    }
}