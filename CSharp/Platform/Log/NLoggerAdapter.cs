using System.Diagnostics;
using System.IO;
using NLog;

namespace Log
{
	public class NLoggerAdapter: ILogger
	{
		private const string SEP = " ";

		private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public NLoggerAdapter()
		{
			this.FileName = true;
			this.FileLineNumber = true;
		}

		public bool FileName
		{
			get;
			set;
		}

		public bool FileLineNumber
		{
			get;
			set;
		}

		public string GetExtraInfo()
		{
			if (!this.FileLineNumber && !this.FileName)
			{
				return SEP;
			}

			string extraInfo = "";
			var stackTrace = new StackTrace(true);
			var frame = stackTrace.GetFrame(3);

			if (FileName)
			{
				var fileName = Path.GetFileName(frame.GetFileName());
				extraInfo += fileName + " ";
			}
			if (FileLineNumber)
			{
				var fileLineNumber = frame.GetFileLineNumber();
				extraInfo += fileLineNumber + " ";
			}
			extraInfo += SEP;

			return extraInfo;
		}

		public void Trace(string message)
		{
			logger.Trace(GetExtraInfo() + message);
		}

		public void Debug(string message)
		{
			logger.Debug(GetExtraInfo() + message);
		}
	}
}
