using System;
using Base;

namespace Model
{
	[ObjectEvent]
	public class BenchmakComponentEvent : ObjectEvent<BenchmakComponent>, IAwake<string>
	{
		public void Awake(string address)
		{
			BenchmakComponent component = this.GetValue();
			component.Awake(address);
		}
	}

	public class BenchmakComponent : Component
	{
		private int k;

		private long time1 = TimeHelper.ClientNowTicks();

		public async void Awake(string address)
		{
			NetOuterComponent networkComponent = Game.Scene.GetComponent<NetOuterComponent>();

			for (int i = 0; i < 100; i++)
			{
				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(10);
				TestAsync(networkComponent, address, i);
			}
		}

		private async void TestAsync(NetOuterComponent networkComponent, string address, int j)
		{
			using (Session session = networkComponent.Create(address))
			{
				int i = 0;
				while (i < 10000000)
				{
					++i;
					try
					{
						await session.Call<C2R_Ping, R2C_Ping>(new C2R_Ping());

						++this.k;
						if (this.k % 100000 == 0)
						{
							long time2 = TimeHelper.ClientNowTicks();
							long time = time2 - this.time1;
							this.time1 = time2;
							Log.Info($"{j} Benchmark k: {k} 每10W次耗时: {time}");
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