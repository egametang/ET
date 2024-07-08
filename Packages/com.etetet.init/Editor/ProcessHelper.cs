using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;
using Path = System.IO.Path;

namespace ET
{
    internal static class ProcessHelper
    {
        public static System.Diagnostics.Process PowerShell(string arguments, string workingDirectory = ".", bool waitExit = false)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Run("powershell.exe", arguments, workingDirectory, waitExit);
            }
            else
            {
                return Run("/usr/local/bin/pwsh", arguments, workingDirectory, waitExit);
            }
        }

        public static System.Diagnostics.Process Run(string exe, string arguments, string workingDirectory = ".", bool waitExit = false)
        {
            //Log.Debug($"Process Run exe:{exe} ,arguments:{arguments} ,workingDirectory:{workingDirectory}");
            try
            {
                bool redirectStandardOutput = false;
                bool redirectStandardError = false;
                bool useShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                
                if (waitExit)
                {
                    redirectStandardOutput = true;
                    redirectStandardError = true;
                    useShellExecute = false;
                }
                
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = useShellExecute,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = redirectStandardOutput,
                    RedirectStandardError = redirectStandardError,
                };

                System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);

                if (waitExit)
                {
                    process.WaitForExit();
                }

                return process;
            }
            catch (Exception e)
            {
                throw new Exception($"dir: {Path.GetFullPath(workingDirectory)}, command: {exe} {arguments}", e);
            }
        }
    }
}