using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class ClientSceneFlagComponent: Entity, IAwake, IDestroy
    {
    }
}