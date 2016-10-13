namespace Base
{
	public sealed class Server
	{
		private static Entity server = new Entity();

		public static Entity Scene
		{
			get
			{
				return server;
			}
		}

		public static void Close()
		{
			Entity scene = server;
			server = null;
			scene?.Dispose();
		}
	}
}