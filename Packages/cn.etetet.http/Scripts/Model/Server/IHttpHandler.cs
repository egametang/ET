using System.Net;

namespace ET.Server
{
    public interface IHttpHandler
    {
        ETTask Handle(Scene scene, HttpListenerContext context);
    }
}