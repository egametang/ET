using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace ET
{
    public static class ShellHelper
    {
        public static void Run(string cmd, string workDirectory, List<string> environmentVars = null)
        {
            System.Diagnostics.Process process = new();
            try
            {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                string app = "bash";
                string splitChar = ":";
                string arguments = "-c";
#elif UNITY_EDITOR_WIN
                string app = "cmd.exe";
                string splitChar = ";";
                string arguments = "/c";
#endif
                ProcessStartInfo start = new ProcessStartInfo(app);

                if (environmentVars != null)
                {
                    foreach (string var in environmentVars)
                    {
                        start.EnvironmentVariables["PATH"] += (splitChar + var);
                    }
                }

                process.StartInfo = start;
                start.Arguments = arguments + " \"" + cmd + "\"";
                start.CreateNoWindow = true;
                start.ErrorDialog = true;
                start.UseShellExecute = false;
                start.WorkingDirectory = workDirectory;

                if (start.UseShellExecute)
                {
                    start.RedirectStandardOutput = false;
                    start.RedirectStandardError = false;
                    start.RedirectStandardInput = false;
                }
                else
                {
                    start.RedirectStandardOutput = true;
                    start.RedirectStandardError = true;
                    start.RedirectStandardInput = true;
                    start.StandardOutputEncoding = System.Text.Encoding.UTF8;
                    start.StandardErrorEncoding = System.Text.Encoding.UTF8;
                }

                bool endOutput = false;
                bool endError = false;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        UnityEngine.Debug.Log(args.Data);
                    }
                    else
                    {
                        endOutput = true;
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        UnityEngine.Debug.LogError(args.Data);
                    }
                    else
                    {
                        endError = true;
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!endOutput || !endError)
                {
                }

                process.CancelOutputRead();
                process.CancelErrorRead();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                process.Close();
            }
        }
    }
}