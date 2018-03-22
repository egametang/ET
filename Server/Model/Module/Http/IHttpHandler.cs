using System.Net;

namespace ETModel
{
	public interface IHttpHandler
	{
		void Handle(HttpListenerContext context);
	}

	public abstract class AHttpHandler: IHttpHandler
	{
		public virtual void Handle(HttpListenerContext context)
		{
		}
	}
}