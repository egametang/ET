using System.Net;
using ETModel;
using System.Threading.Tasks;

namespace ETHotfix
{
	[HttpHandler(AppType.Gate, "/")]
	public class HttpTest : AHttpHandler
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
			System.Console.WriteLine("");
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

		[Get] // url-> /GetRechargeRecord
		public async ETTask<HttpResult> GetRechargeRecord(long id)
		{
			// var db = Game.Scene.GetComponent<DBProxyComponent>();

			// var info = await db.Query<RechargeRecord>(id);

			await Task.Delay(1000); // 用于测试

			object info = null;
			if (info != null)
			{
				return Ok(data: info);
			}
			else
			{
				return Error("ID不存在！");
			}
		}
	}
}