using NLog;

namespace ELog
{
	public class NLog: ILog
	{
		private readonly Logger logger = LogManager.GetCurrentClassLogger();

		public void Debug(string message)
		{
			logger.Debug(message);
		}
	}
}
