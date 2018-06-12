using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.AllServer)]
	public class C2R_PingHandler : AMRpcHandler<C2R_Ping, R2C_Ping>
	{
		private bool isStart = false;
		protected override void Run(Session session, C2R_Ping message, Action<R2C_Ping> reply)
		{
			R2C_Ping r2CPing = new R2C_Ping();
			reply(r2CPing);

			if (!this.isStart)
			{
				isStart = true;
				Start(session);
			}
		}
		
		public async void Start(Session session)
		{
			TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
			G2C_Test g2CTest = new G2C_Test();
			while (true)
			{
				await timerComponent.WaitAsync(1);
				for (int i = 0; i < 20; ++i)
				{
					session.Send(g2CTest);	
				}
			}
		}
	}
}