using log4net;

namespace Model
{
	public class Log4NetAdapter : ALogDecorater, ILog
	{
		private readonly log4net.ILog logger = LogManager.GetLogger("Logger");

		public Log4NetAdapter(ALogDecorater decorater = null): base(decorater)
		{
		}

		public void Warning(string message)
		{
			this.logger.Warn(this.Decorate(message));
		}

		public void Info(string message)
		{
			this.logger.Info(this.Decorate(message));
		}

		public void Debug(string message)
		{
			this.logger.Debug(this.Decorate(message));
		}

		public void Error(string message)
		{
			this.logger.Error(this.Decorate(message));
		}
	}
}