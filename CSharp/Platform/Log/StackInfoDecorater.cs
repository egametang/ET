using System.Diagnostics;
using System.IO;

namespace Log
{
	class StackInfoDecorater: ALogDecorater
	{

		public StackInfoDecorater(ALogDecorater decorater = null)
		{
			this.decorater = decorater;
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

		public override string Decorate(string message)
		{
			if (decorater != null)
			{
				message = decorater.Decorate(message);
			}

			if (!this.FileLineNumber && !this.FileName)
			{
				return message;
			}

			string extraInfo = "";
			var stackTrace = new StackTrace(true);
			var frame = stackTrace.GetFrame(this.Level + 4);

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
			return extraInfo + message;
		}
	}
}
