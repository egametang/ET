using System.Diagnostics;
using System.IO;

namespace Logger
{
    internal class StackInfoDecorater: ALogDecorater
    {
        public StackInfoDecorater(ALogDecorater decorater = null): base(decorater)
        {
            this.FileName = true;
            this.FileLineNumber = true;
        }

        public bool FileName { get; set; }

        public bool FileLineNumber { get; set; }

        public override string Decorate(string message)
        {
            if (this.decorater != null)
            {
                message = this.decorater.Decorate(message);
            }

            if (!this.FileLineNumber && !this.FileName)
            {
                return message;
            }

            string extraInfo = "";
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(this.Level + 3);

            if (this.FileName)
            {
                var fileName = Path.GetFileName(frame.GetFileName());
                extraInfo += fileName + " ";
            }
            if (this.FileLineNumber)
            {
                var fileLineNumber = frame.GetFileLineNumber();
                extraInfo += fileLineNumber + " ";
            }
            return extraInfo + message;
        }
    }
}