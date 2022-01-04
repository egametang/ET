using System.Collections.Generic;
using System.Net;

namespace ET
{
    /// <summary>
    /// http请求分发器
    /// </summary>
    public class HttpComponent: Entity
    {
        public HttpListener Listener;
        public Dictionary<string, IHttpHandler> dispatcher;
    }
}