namespace ETModel
{
	public interface ILog
	{
		void Trace(string message);
		void Warning(string message);
		void Info(string message);
		void Debug(string message);
		void Error(string message);
        void Fatal(string message);
		void Trace(string message, params object[] args);
		void Warning(string message, params object[] args);
		void Info(string message, params object[] args);
		void Debug(string message, params object[] args);
		void Error(string message, params object[] args);
		void Fatal(string message, params object[] args);
    }
}