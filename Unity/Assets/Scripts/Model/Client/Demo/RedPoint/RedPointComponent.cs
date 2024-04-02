using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class RedPointComponent: Entity, IAwake, IDestroy
    {
        public Dictionary<string, long> RedPointDic;
    }
}