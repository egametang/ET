using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class WebSocketBenchmarkComponentSystem : AwakeSystem<WebSocketBenchmarkComponent>
	{
		public override void Awake(WebSocketBenchmarkComponent self)
		{
			self.Awake();
		}
	}

	public static class WebSocketBenchmarkComponentHelper
	{
		public static void Awake(this WebSocketBenchmarkComponent self)
		{
			try
			{
				NetOuterComponent networkComponent = Game.Scene.GetComponent<NetOuterComponent>();
				for (int i = 0; i < 1000; i++)
				{
					self.TestAsync(networkComponent, i);
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		
		public static async void TestAsync(this WebSocketBenchmarkComponent self, NetOuterComponent networkComponent, int j)
		{
			try
			{
				using (Session session = networkComponent.Create($"ws://127.0.0.1:8080"))
				{
					int i = 0;
					while (i < 100000000)
					{
						++i;
						await self.Send(session, j);
					}
				}
			}
			catch (RpcException e)
			{
				Log.Error(e);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		
		public static async Task Send(this WebSocketBenchmarkComponent self, Session session, int j)
		{
			try
			{
				await session.Call(new C2R_Ping());
				++self.k;

				if (self.k % 10000 != 0)
				{
					return;
				}

				long time2 = TimeHelper.ClientNow();
				long time = time2 - self.time1;
				self.time1 = time2;
				Log.Info($"Benchmark k: {self.k} 每1W次耗时: {time} ms {session.Network.Count}");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}