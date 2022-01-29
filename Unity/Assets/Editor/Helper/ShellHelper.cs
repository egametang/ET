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

        public static void Run(string cmd, string workDirectory, List<string> environmentVars = null)
        {
            Process p = null;
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

                start.Arguments += (" \"" + cmd + " \"");
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

                p = Process.Start(start);
                p.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e) { UnityEngine.Debug.LogError(e.Data); };
                p.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e) { UnityEngine.Debug.LogError(e.Data); };
                p.Exited += delegate(object sender, System.EventArgs e) { UnityEngine.Debug.LogError(e.ToString()); };

                bool hasError = false;
                do
                {
                    string line = p.StandardOutput.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    line = line.Replace("\\", "/");

                    UnityEngine.Debug.Log(line);
                }
                while (true);

                while (true)
                {
                    string error = p.StandardError.ReadLine();
                    if (string.IsNullOrEmpty(error))
                    {
                        break;
                    }

                    hasError = true;
                    UnityEngine.Debug.LogError(error);
                }

                p.Close();
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