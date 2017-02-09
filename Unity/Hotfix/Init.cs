using Base;
using Model;

namespace Hotfix
{
	public class Init
	{
		public void Start()
		{
			StartAsync();
		}

		public async void StartAsync()
		{
			while (true)
			{
				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(1000);
				Log.Debug("11111111111111111111111111111111111111");
			}
		}

		public void Update()
		{
		}
	}
}
