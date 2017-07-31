namespace Model
{
	public interface ILog
	{
		void Warning(string message);
		void Info(string message);
		void Debug(string message);
		void Error(string message);
	}
}