using System;
using Model;

namespace Hotfix
{
	public class BenchmarkComponent: HotfixComponent
	{
		private int k;

		private long time1 = TimeHelper.ClientNow();

		private async void Awake(string address)
		{
			try
			{
				NetOuterComponent networkComponent = Hotfix.Scene.GetComponent<NetOuterComponent>();

				for (int i = 0; i < 100; i++)
				{
					await Hotfix.Scene.GetComponent<TimerComponent>().WaitAsync(10);
					this.TestAsync(networkComponent, address, i);
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public async void TestAsync(NetOuterComponent networkComponent, string address, int j)
		{
			Session session = null;
			try
			{
				session = networkComponent.Create(address);
				int i = 0;
				while (i < 10000000)
				{
					++i;
					await session.Call<C2R_Ping, R2C_Ping>(new C2R_Ping());

					++this.k;

					if (this.k % 100000 != 0)
					{
						continue;
					}

					long time2 = TimeHelper.ClientNow();
					long time = time2 - this.time1;
					this.time1 = time2;
					Log.Info($"{j} Benchmark k: {this.k} 每10W次耗时: {time} ms");
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
			finally
			{
				session.Dispose();
			}
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}