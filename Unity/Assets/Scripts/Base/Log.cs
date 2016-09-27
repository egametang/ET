namespace Base
{
	public static class Log
	{
        public static void Warning(string msg)
		{
			Share.Scene.GetComponent<LogComponent>().Warning(msg);
		}

		public static void Info(string msg)
		{
			Share.Scene.GetComponent<LogComponent>().Info(msg);
		}

		public static void Error(string msg)
		{
			Share.Scene.GetComponent<LogComponent>().Error(msg);
		}

		public static void Debug(string msg)
		{
			Share.Scene.GetComponent<LogComponent>().Debug(msg);
		}

		public static void Flush()
		{
			Share.Scene.GetComponent<LogComponent>().Flush();
		}
	}
}