using System.Net;
using ETModel;

namespace ETHotfix
{
	public class AccountInfo
	{
		public string name;
		public string pwd;
	}

	[HttpHandler(AppType.Gate, "/")]
	public class HttpTest: AHttpHandler
	{
		[Get] // url-> /Login?name=11&age=1111
		public string Login(string name, int age, HttpListenerRequest req, HttpListenerResponse resp)
		{
			Log.Info(name);
			Log.Info($"{age}");
			return "ok";
		}

		[Get("t")] // url-> /t
		public int Test()
		{
			return 1;
		}

		[Post] // url-> /Test1
		public int Test1(HttpListenerRequest req)
		{
			return 1;
		}

		[Get] // url-> /Test2
		public int Test2(HttpListenerResponse resp)
		{
			return 1;
		}

		[Post]
		public object Test3(HttpListenerResponse resp, HttpListenerRequest req, string postBody, AccountInfo accountInfo)
		{
			Log.Info(postBody);
			Log.Info(JsonHelper.ToJson(accountInfo));
			return new { name = "1111" };
		}
	}
}