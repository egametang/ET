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

		public void Awake(string address)
		{
			NetOuterComponent networkComponent = Game.Scene.GetComponent<NetOuterComponent>();

			for (int i = 0; i < 400; i++)
			{
				TestAsync(networkComponent, address, i);
			}
		}

		private async void TestAsync(NetOuterComponent networkComponent, string address, int j)
		{
			using (Session session = networkComponent.Create(address))
			{
				int i = 0;
				while (i < 10000)
				{
					++i;
					try
					{
						R2C_Login s2CLogin = await session.Call<C2R_Login, R2C_Login>(new C2R_Login { Account = "abcdef", Password = "111111" });

						using (Session gateSession = networkComponent.Create(s2CLogin.Address))
						{
							await gateSession.Call<C2G_LoginGate, G2C_LoginGate>(new C2G_LoginGate(s2CLogin.Key));
						}

						++this.k;
						if (this.k % 1000 == 0)
						{
							Log.Info($"{j} Benchmark k: {k}");
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