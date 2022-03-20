using System.Net;

namespace ET
{
    public interface IHttpHandler
    {
        ETTask Handle(Entity domain, HttpListenerContext context);
    }
}