using System.Net;

namespace ETModel
{
	public interface IHttpHandler
	{
		void Handle(HttpListenerContext context);
	}
}