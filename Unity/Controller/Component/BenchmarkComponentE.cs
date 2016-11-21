using System;
using Base;
using Model;

namespace Controller
{
	[DisposerEvent(typeof(BenchmakComponent))]
	public static class BenchmakComponentE
	{
		private static async void Awake(this BenchmakComponent component, string address)
		{
			try
			{
				NetOuterComponent networkComponent = Game.Scene.GetComponent<NetOuterComponent>();

				for (int i = 0; i < 100; i++)
				{
					await Game.Scene.GetComponent<TimerComponent>().WaitAsync(10);
					component.TestAsync(networkComponent, address, i);
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}
