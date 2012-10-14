
namespace Log
{
    public interface ILogger
    {
		bool FileName
		{
			get;
			set;
		}

		bool FileLineNumber
		{
			get;
			set;
		}
		void Trace(string message);
	    void Debug(string message);
    }
}
