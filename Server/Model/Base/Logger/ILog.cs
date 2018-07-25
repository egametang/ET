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
    }
}