using System.Collections.Generic;
using System.Net;

namespace ET.Server
{
    /// <summary>
    /// http请求分发器
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class HttpComponent: Entity, IAwake<string>, IDestroy, ILoad
    {
        public HttpListener Listener;
        public Dictionary<string, IHttpHandler> dispatcher;
    }
}