using System.Net;

namespace Model
{
	public interface IHttpHandler
	{
		void Handle(HttpListenerContext context);
	}
}