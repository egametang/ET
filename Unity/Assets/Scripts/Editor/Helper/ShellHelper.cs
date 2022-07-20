using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace ET
{
    public static class ShellHelper
    {
        private static string shellApp
        {
            get
            {
#if UNITY_EDITOR_WIN
			    string app = "cmd.exe";
#elif UNITY_EDITOR_OSX
                string app = "bash";
#endif
                return app;
            }
        }

        private static volatile bool isFinish;

        public static void Run(string cmd, string workDirectory, List<string> environmentVars = null)
        {
            Process p = new();
            try
            {
                ProcessStartInfo start = new ProcessStartInfo(shellApp);

#if UNITY_EDITOR_OSX
                string splitChar = ":";
                start.Arguments = "-c";
#elif UNITY_EDITOR_WIN
                string splitChar = ";";
                start.Arguments = "/c";
#endif

                if (environmentVars != null)
                {
                    foreach (string var in environmentVars)
                    {
                        start.EnvironmentVariables["PATH"] += (splitChar + var);
                    }
                }

                p.StartInfo = start;
                start.Arguments += (" \"" + cmd + "\"");
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

                bool hasError = false;
                bool endOutput = false;
                bool endError = false;

                p.OutputDataReceived += (sender, args) =>
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

                p.ErrorDataReceived += (sender, args) =>
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

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                while (!endOutput || !endError) { }

                p.CancelOutputRead();
                p.CancelErrorRead();

                if (hasError)
                {
                    UnityEngine.Debug.LogError("has error!");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                if (p != null)
                {
                    p.Close();
                }
            }
        }
    }
}