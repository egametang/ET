using System.Collections.Generic;
using System.Net;

namespace ET
{
    public class RouterAddressComponent: Entity, IAwake<string>
    {
        public string RouterManagerAddress;
        public HttpGetRouterResponse Info;
        public int RouterIndex;
    }
}