using NLog;

namespace Log
{
	public class NLoggerAdapter : ILogger
	{
		private const string SEP = " ";

		private readonly ALogDecorater decorater;

		private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public NLoggerAdapter(ALogDecorater decorater = null)
		{
			this.decorater = decorater;
			if (this.decorater != null)
			{
				this.decorater.Level = 0;
			}
		}

		public string Decorate(string message)
		{
			if (decorater == null)
			{
				return message;
			}
			return decorater.Decorate(message);
		}

		public void Trace(string message)
		{
			logger.Trace(Decorate(SEP + message));
		}

		public void Debug(string message)
		{
			logger.Debug(Decorate(SEP + message));
		}
	}
}
