using System.Net;

namespace ET.Server
{
    public interface IHttpHandler
    {
        ETTask Handle(Entity domain, HttpListenerContext context);
    }
}