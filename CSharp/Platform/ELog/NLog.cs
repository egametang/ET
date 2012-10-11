using NLog;

namespace ELog
{
	public class NLog: ILog
	{
		private readonly global::NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public void Trace(string message)
		{
			logger.Trace(message);
		}

		public void Debug(string message)
		{
			logger.Debug(message);
		}
	}
}
