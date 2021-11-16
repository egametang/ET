namespace ET
{
	public class MonoEntry : IEntry
	{
		public void Start()
		{
			GameStart.Start();
		}

		public void Update()
		{
			ThreadSynchronizationContext.Instance.Update();
			Game.EventSystem.Update();
		}

		public void LateUpdate()
		{
			Game.EventSystem.LateUpdate();
		}

		public void OnApplicationQuit()
		{
			Game.Close();
		}
	}
}