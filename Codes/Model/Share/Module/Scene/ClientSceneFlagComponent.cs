using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ClientSceneFlagComponent: Entity, IAwake, IDestroy
    {
    }
}