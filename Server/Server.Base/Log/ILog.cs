namespace Base
{
	public interface ILog
	{
		void Info(string message);
		void Debug(string message);
		void Error(string message);
	}
}