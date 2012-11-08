using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using EnvDTE;
using Process = System.Diagnostics.Process;

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
			var stream = File.Open("E:\\ProtobufTool.log", FileMode.OpenOrCreate);
			using (var streamWriter = new StreamWriter(stream))
			{
				streamWriter.WriteLine("input: {0}\r\n {1}", inputFileName, inputFileContent);
				var processStartInfo = new ProcessStartInfo("protogen.exe")
				{
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
				};

				var projectItem = this.GetService(typeof(ProjectItem)) as ProjectItem;
				processStartInfo.WorkingDirectory = Path.GetDirectoryName(projectItem.ContainingProject.FullName);
				streamWriter.WriteLine("WorkingDirectory: {0}", processStartInfo.WorkingDirectory);
				processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

				var tempFile = Path.GetTempFileName();
				processStartInfo.Arguments = string.Format(
						"-i:{0} -o:{1} -q -t:csharp", inputFileName, tempFile);
				streamWriter.WriteLine("Arguments: {0}", processStartInfo.Arguments);
				using (var process = Process.Start(processStartInfo))
				{
					process.WaitForExit();
					streamWriter.WriteLine("return code: {0}", process.ExitCode);
				}
				byte[] bytes = File.ReadAllBytes(tempFile);
				File.Delete(tempFile);
				return bytes;
			}
		}
    }
}
