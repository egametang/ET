using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextTemplating.VSHost;

namespace ProtobufTool
{
	[ComVisible(true)]
	[Guid("9cf79956-6dfb-4d5b-8d29-7b43f7306acf")]
	public class ProtobufGenerator : BaseCodeGeneratorWithSite
    {
		public override string GetDefaultExtension()
		{
			return ".pb.cs";
		}

		protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
		{
			var processStartInfo = new ProcessStartInfo("protogen.exe")
			{
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = Environment.CurrentDirectory,
				RedirectStandardError = true,
				RedirectStandardOutput = true
			};
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

			var tempFile = Path.GetTempFileName();
			string inputDir = Path.GetFullPath(inputFileName);
			processStartInfo.Arguments = string.Format(
				" -q -writeErrors -t: csharp -i:{0} -w:{1} -o:{2} ",
				inputFileName, inputDir, tempFile);			
			using(var process = Process.Start(processStartInfo))
			{
				process.WaitForExit();
			}
			byte[] bytes = File.ReadAllBytes(tempFile);
			File.Delete(tempFile);
			return bytes;
		}
    }
}
