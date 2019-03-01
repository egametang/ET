using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ETModel
{
    public static class ProcessHelper
    {
        public static Process Run(string exe, string arguments, string workingDirectory = ".", bool waitExit = false)
        {
            try
            {
                bool redirectStandardOutput = true;
                bool redirectStandardError = true;
                bool useShellExecute = false;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    redirectStandardOutput = false;
                    redirectStandardError = false;
                    useShellExecute = true;
                }

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
                
                Process process = Process.Start(info);

                if (waitExit)
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new Exception(process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd() + "\n"
                            + $"请在terminal中执行，目录{workingDirectory}, 命令{exe} {arguments}");
                    }
                }

                return process;
            }
            catch (Exception e)
            {
                throw new Exception($"请在terminal中执行，目录{workingDirectory}, 命令{exe} {arguments}", e);
            }
        }
    }
}