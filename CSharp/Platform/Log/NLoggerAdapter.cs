using NLog;

namespace Log
{
	public class NLoggerAdapter: ALogDecorater, ILogger
	{
		private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public NLoggerAdapter(ALogDecorater decorater = null): base(decorater)
		{
		}

		public void Trace(string message)
		{
			this.logger.Trace(this.Decorate(SEP + message));
		}

		public void Debug(string message)
		{
			this.logger.Debug(this.Decorate(SEP + message));
		}
	}
}