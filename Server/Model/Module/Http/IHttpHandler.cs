using System.Net;

namespace ETModel
{
	public interface IHttpHandler
	{
		void Handle(HttpListenerContext context);
	}

	public abstract class AHttpHandler : IHttpHandler
	{
		public virtual void Handle(HttpListenerContext context)
		{
		}
		public virtual HttpResult Ok(string msg = "", object data = null)
		{
			return new HttpResult
			{
				code = HttpErrorCode.Success,
				msg = msg,
				status = true,
				data = data
			};
		}

		public virtual HttpResult Error(string msg = "")
		{
			return new HttpResult
			{
				code = HttpErrorCode.Exception,
				msg = msg,
				status = false
			};
		}
	}
}