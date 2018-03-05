using System;
using System.Net;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class BenchmarkComponentSystem : AwakeSystem<BenchmarkComponent, IPEndPoint>
	{
		public override void Awake(BenchmarkComponent self, IPEndPoint a)
		{
			self.Awake(a);
		}
	}

	public class BenchmarkComponent: Component
	{
		private int k;

		private long time1 = TimeHelper.ClientNow();

		public void Awake(IPEndPoint ipEndPoint)
		{
			try
			{
				NetOuterComponent networkComponent = Game.Scene.GetComponent<NetOuterComponent>();
				for (int i = 0; i < 100; i++)
				{
					this.TestAsync(networkComponent, ipEndPoint, i);
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public async void TestAsync(NetOuterComponent networkComponent, IPEndPoint ipEndPoint, int j)
		{
			try
			{
				using (Session session = networkComponent.Create(ipEndPoint))
				{
					int i = 0;
					while (i < 100000000)
					{
						++i;
						await this.Send(session, j);
					}
				}
			}
			catch (RpcException e)
			{
				Log.Error(e.ToString());
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public async Task Send(Session session, int j)
		{
			try
			{
				await session.Call(new C2R_Ping());
				++this.k;

				if (this.k % 100000 != 0)
				{
					return;
				}

				long time2 = TimeHelper.ClientNow();
				long time = time2 - this.time1;
				this.time1 = time2;
				Log.Info($"Benchmark k: {this.k} 每10W次耗时: {time} ms");
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}