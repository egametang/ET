using NLog;

namespace Base
{
	public class NLogAdapter: ALogDecorater, ILog
	{
		private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public NLogAdapter(ALogDecorater decorater = null): base(decorater)
		{
		}

		public void Info(string message)
		{
			this.logger.Info(this.Decorate(SEP + message));
		}

		public void Debug(string message)
		{
			this.logger.Debug(this.Decorate(SEP + message));
		}

		public void Error(string message)
		{
			this.logger.Error(this.Decorate(SEP + message));
		}
	}
}