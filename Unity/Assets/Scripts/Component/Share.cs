namespace Base
{
	/// <summary>
	/// 游戏和扩展编辑器都需要用到的数据放在这个Scene上面
	/// </summary>
	public sealed class Share
	{
		private static Entity share;

		public static Entity Scene
		{
			get
			{
				if (share == null)
				{
					share = new Entity();
					share.AddComponent<Scene>();
					share.AddComponent<EventComponent>();
					share.AddComponent<LogComponent>();
					GlobalConfigComponent globalConfigComponent = share.AddComponent<GlobalConfigComponent>();
					share.AddComponent<NetworkComponent, NetworkProtocol>(globalConfigComponent.GlobalProto.Protocol);
				}
				return share;
			}
		}

		public static void Close()
		{
			Entity scene = share;
			share = null;
			scene?.Dispose();
		}
	}
}