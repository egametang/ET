using System;
using System.Diagnostics;

namespace ET
{
    public class Logger: Singleton<Logger>
    {
        /// <summary>
        /// 日志接口
        /// </summary>
        private ILog iLog;

        public ILog ILog
        {
            set
            {
                this.iLog = value;
            }
        }
        
        private const int TraceLevel = 1;           // 定义Trace级别的常量
        private const int DebugLevel = 2;           // 定义Debug级别的常量
        private const int InfoLevel = 3;            // 定义Info级别的常量
        private const int WarningLevel = 4;         // 定义Warning级别的常量

        /// <summary>
        /// 检查日志的级别
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private bool CheckLogLevel(int level)
        {
            if (Options.Instance == null)
            {
                return true;
            }
            return Options.Instance.LogLevel <= level;
        }
        
        /// <summary>
        /// 显示堆栈
        /// </summary>
        /// <param name="msg"></param>
        public void Trace(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }

            // 创建一个StackTrace对象，从第二个栈帧开始，包含源文件信息
            StackTrace st = new StackTrace(2, true);
            this.iLog.Trace($"{msg}\n{st}");
        }

        /// <summary>
        /// 显示Debug信息
        /// </summary>
        /// <param name="msg"></param>
        public void Debug(string msg)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            this.iLog.Debug(msg);
        }

        /// <summary>
        /// 显示普通消息
        /// </summary>
        /// <param name="msg"></param>
        public void Info(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            this.iLog.Info(msg);
        }

        /// <summary>
        /// 显示追踪栈信息
        /// </summary>
        /// <param name="msg"></param>
        public void TraceInfo(string msg)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(2, true);
            this.iLog.Trace($"{msg}\n{st}");
        }

        /// <summary>
        /// 显示警告信息
        /// </summary>
        /// <param name="msg"></param>
        public void Warning(string msg)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }

            this.iLog.Warning(msg);
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="msg"></param>
        public void Error(string msg)
        {
            StackTrace st = new StackTrace(2, true);
            this.iLog.Error($"{msg}\n{st}");
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="e"></param>
        public void Error(Exception e)
        {
            if (e.Data.Contains("StackTrace"))
            {
                this.iLog.Error($"{e.Data["StackTrace"]}\n{e}");
                return;
            }
            string str = e.ToString();
            this.iLog.Error(str);
        }

        /// <summary>
        /// 显示栈追踪信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Trace(string message, params object[] args)
        {
            if (!CheckLogLevel(TraceLevel))
            {
                return;
            }
            StackTrace st = new StackTrace(2, true);
            this.iLog.Trace($"{string.Format(message, args)}\n{st}");
        }

        /// <summary>
        /// 显示警告信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Warning(string message, params object[] args)
        {
            if (!CheckLogLevel(WarningLevel))
            {
                return;
            }
            this.iLog.Warning(string.Format(message, args));
        }

        /// <summary>
        /// 显示普通信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Info(string message, params object[] args)
        {
            if (!CheckLogLevel(InfoLevel))
            {
                return;
            }
            this.iLog.Info(string.Format(message, args));
        }

        /// <summary>
        /// 显示调试信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Debug(string message, params object[] args)
        {
            if (!CheckLogLevel(DebugLevel))
            {
                return;
            }
            this.iLog.Debug(string.Format(message, args));
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Error(string message, params object[] args)
        {
            StackTrace st = new StackTrace(2, true);
            string s = string.Format(message, args) + '\n' + st;
            this.iLog.Error(s);
        }
        
        /// <summary>
        /// 在控制台显示日志
        /// </summary>
        /// <param name="message"></param>
        public void Console(string message)
        {
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(message);
            }
            this.iLog.Debug(message);
        }
        
        /// <summary>
        /// 在控制台显示日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Console(string message, params object[] args)
        {
            string s = string.Format(message, args);
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(s);
            }
            this.iLog.Debug(s);
        }
    }
}