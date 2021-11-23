using System.IO;

namespace ET
{
    public class FileLogger: ILog
    {
        private readonly StreamWriter stream;

        public FileLogger(string path)
        {
            this.stream = new StreamWriter(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
            this.stream.AutoFlush = true;
        }

        public void Trace(string message)
        {
            this.stream.WriteLine(message);
            this.stream.Flush();
        }

        public void Warning(string message)
        {
            this.stream.WriteLine(message);
            this.stream.Flush();
        }

        public void Info(string message)
        {
            this.stream.WriteLine(message);
            this.stream.Flush();
        }

        public void Debug(string message)
        {
            this.stream.WriteLine(message);
            this.stream.Flush();
        }

        public void Error(string message)
        {
            this.stream.WriteLine(message);
            this.stream.Flush();
        }

        public void Trace(string message, params object[] args)
        {
            this.stream.WriteLine(message, args);
            this.stream.Flush();
        }

        public void Warning(string message, params object[] args)
        {
            this.stream.WriteLine(message, args);
            this.stream.Flush();
        }

        public void Info(string message, params object[] args)
        {
            this.stream.WriteLine(message, args);
            this.stream.Flush();
        }

        public void Debug(string message, params object[] args)
        {
            this.stream.WriteLine(message, args);
            this.stream.Flush();
        }

        public void Error(string message, params object[] args)
        {
            this.stream.WriteLine(message, args);
            this.stream.Flush();
        }

        public void Fatal(string message, params object[] args)
        {
            this.stream.WriteLine(message, args);
            this.stream.Flush();
        }
    }
}