using NLog;

namespace Common.Logger
{
	public class NLogAdapter: ALogDecorater, ILog
	{
		private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public NLogAdapter(ALogDecorater decorater = null): base(decorater)
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