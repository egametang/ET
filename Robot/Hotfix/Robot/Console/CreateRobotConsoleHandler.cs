using System;
using System.Collections.Generic;
using CommandLine;
using NLog;

namespace ET
{
    [ConsoleHandler(ConsoleMode.CreateRobot)]
    public class CreateRobotConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContex contex, string content)
        {
            switch (content)
            {
                case ConsoleMode.CreateRobot:
                    Log.Console("CreateRobot args error!");
                    break;
                default:
                    CreateRobotArgs options = null;
                    Parser.Default.ParseArguments<CreateRobotArgs>(content.Split(' '))
                            .WithNotParsed(error => throw new Exception($"CreateRobotArgs error!"))
                            .WithParsed(o => { options = o; });

                    // 获取当前进程的RobotScene
                    using (ListComponent<StartSceneConfig> thisProcessRobotScenes = ListComponent<StartSceneConfig>.Create())
                    {
                        List<StartSceneConfig> robotSceneConfigs = StartSceneConfigCategory.Instance.Robots;
                        foreach (StartSceneConfig robotSceneConfig in robotSceneConfigs)
                        {
                            if (robotSceneConfig.Process != Game.Options.Process)
                            {
                                continue;
                            }
                            thisProcessRobotScenes.Add(robotSceneConfig);
                        }
                        
                        // 创建机器人
                        for (int i = 0; i < options.Num; ++i)
                        {
                            int index = i % thisProcessRobotScenes.Count;
                            StartSceneConfig robotSceneConfig = thisProcessRobotScenes[index];
                            Scene robotScene = Game.Scene.Get(robotSceneConfig.Id);
                            RobotManagerComponent robotManagerComponent = robotScene.GetComponent<RobotManagerComponent>();
                            Scene robot = await robotManagerComponent.NewRobot(Game.Options.Process * 10000 + i);
                            Log.Console($"create robot {robot.Zone}");
                            await TimerComponent.Instance.WaitAsync(2000);
                        }
                    }
                    break;
            }
            contex.Parent.RemoveComponent<ModeContex>();
            await ETTask.CompletedTask;
        }
    }
}