using System.Collections.Generic;
using System.Net;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class RouterAddressComponent: Entity, IAwake<string>
    {
        public string RouterManagerAddress;
        public HttpGetRouterResponse Info;
        public int RouterIndex;
    }
}