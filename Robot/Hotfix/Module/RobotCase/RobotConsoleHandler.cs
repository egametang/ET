using System;
using System.Reflection;

namespace ET
{
    [ConsoleHandler(ConsoleMode.Robot)]
    public class RobotConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContex contex, string content)
        {
            string[] ss = content.Split(" ");
            switch (ss[0])
            {
                case ConsoleMode.Robot:
                    break;

                case "Run":
                {
                    int caseType = int.Parse(ss[1]);

                    try
                    {
                        RobotLog.Debug($"run case start: {caseType}");
                        await RobotCaseDispatcherComponent.Instance.Run(caseType, content);
                        RobotLog.Debug($"run case finish: {caseType}");
                    }
                    catch (Exception e)
                    {
                        RobotLog.Debug($"run case error: {caseType}\n{e}");
                    }
                    break;
                }
                case "RunAll":
                {
                    FieldInfo[] fieldInfos = typeof (RobotCaseType).GetFields();
                    foreach (FieldInfo fieldInfo in fieldInfos)
                    {
                        int caseType = (int)fieldInfo.GetValue(null);
                        if (caseType > RobotCaseType.MaxCaseType)
                        {
                            RobotLog.Debug($"case > {RobotCaseType.MaxCaseType}: {caseType}");
                            break;
                        }
                        try
                        {
                            RobotLog.Debug($"run case start: {caseType}");
                            await RobotCaseDispatcherComponent.Instance.Run(caseType, content);
                            RobotLog.Debug($"---------run case finish: {caseType}");
                        }
                        catch (Exception e)
                        {
                            RobotLog.Debug($"run case error: {caseType}\n{e}");
                            break;
                        }
                    }
                    break;
                }
            }
            await ETTask.CompletedTask;
        }
    }
}