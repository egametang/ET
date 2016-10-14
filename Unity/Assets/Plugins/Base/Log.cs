namespace Base
{
	public static class Log
	{
        public static void Warning(string msg)
		{
			Game.Scene.GetComponent<LogComponent>().Info(msg);
		}

		public static void Info(string msg)
		{
			Game.Scene.GetComponent<LogComponent>().Info(msg);
		}

		public static void Debug(string msg)
		{
			Game.Scene.GetComponent<LogComponent>().Info(msg);
		}

		public static void Error(string msg)
		{
			Game.Scene.GetComponent<LogComponent>().Error(msg);
		}

		public static void Flush()
		{
			Game.Scene.GetComponent<LogComponent>().Flush();
		}
	}
}