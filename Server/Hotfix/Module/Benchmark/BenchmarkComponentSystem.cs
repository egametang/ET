using System;
using System.Net;
using System.Threading.Tasks;


namespace ET
{
	public class BenchmarkComponentSystem : AwakeSystem<BenchmarkComponent, string>
	{
		public override void Awake(BenchmarkComponent self, string a)
		{
			self.Awake(a);
		}
	}

	public static class BenchmarkComponentHelper
	{
		public static void Awake(this BenchmarkComponent self, string address)
		{
			try
			{
				IPEndPoint ipEndPoint = NetworkHelper.ToIPEndPoint(address);
				NetOuterComponent networkComponent = Game.Scene.GetComponent<NetOuterComponent>();
				for (int i = 0; i < 2000; i++)
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
				Log.Info($"Benchmark k: {self.k} 每10W次耗时: {time} ms {session.Network.Count}");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}