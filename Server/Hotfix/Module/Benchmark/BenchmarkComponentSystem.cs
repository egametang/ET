using System;
using System.Net;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class BenchmarkComponentSystem : AwakeSystem<BenchmarkComponent, IPEndPoint>
	{
		public override void Awake(BenchmarkComponent self, IPEndPoint a)
		{
			self.Awake(a);
		}
	}

	public static class BenchmarkComponentEx
	{
		public static void Awake(this BenchmarkComponent self, IPEndPoint ipEndPoint)
		{
			try
			{
				NetOuterComponent networkComponent = Game.Scene.GetComponent<NetOuterComponent>();
				for (int i = 0; i < 100; i++)
				{
					self.TestAsync(networkComponent, ipEndPoint, i);
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static async void TestAsync(this BenchmarkComponent self, NetOuterComponent networkComponent, IPEndPoint ipEndPoint, int j)
		{
			try
			{
				using (Session session = networkComponent.Create(ipEndPoint))
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

		public static async Task Send(this BenchmarkComponent self, Session session, int j)
		{
			try
			{
				await session.Call(new C2R_Ping());
				++self.k;

				if (self.k % 100000 != 0)
				{
					return;
				}

				long time2 = TimeHelper.ClientNow();
				long time = time2 - self.time1;
				self.time1 = time2;
				Log.Info($"Benchmark k: {self.k} 每10W次耗时: {time} ms");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}