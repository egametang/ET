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
                    contex.Parent.RemoveComponent<ModeContex>();
                    Log.Console("CreateRobot args error!");
                    break;
                default:
                    CreateRobotArgs options = null;
                    Parser.Default.ParseArguments<CreateRobotArgs>(content.Split(' '))
                            .WithNotParsed(error => throw new Exception($"CreateRobotArgs error!"))
                            .WithParsed(o => { options = o; });

                    // 获取当前进程的RobotScene
                    List<StartSceneConfig> robotSceneConfigs = StartSceneConfigCategory.Instance.GetByProcess(Game.Options.Process);
                    // 创建机器人
                    for (int i = 0; i < options.Num; ++i)
                    {
                        int index = i % robotSceneConfigs.Count;
                        StartSceneConfig robotSceneConfig = robotSceneConfigs[index];
                        Scene robotScene = Game.Scene.Get(robotSceneConfig.Id);
                        RobotManagerComponent robotManagerComponent = robotScene.GetComponent<RobotManagerComponent>();
                        await robotManagerComponent.NewRobot(Game.Options.Process * 10000 + i);
                    }
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
}