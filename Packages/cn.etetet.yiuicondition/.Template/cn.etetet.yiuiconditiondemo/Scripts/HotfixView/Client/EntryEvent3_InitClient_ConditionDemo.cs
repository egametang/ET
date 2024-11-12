using System;
using System.Collections.Generic;
using System.IO;

namespace ET.Client
{
    [Event(SceneType.StateSync)]
    public class EntryEvent3_InitClient_ConditionDemo : AEvent<Scene, EntryEvent3>
    {
        protected override async ETTask Run(Scene root, EntryEvent3 args)
        {
            root.AddComponent<ConditionMgr>();
            root.AddComponent<CondititonDemoComponent>();
            await ETTask.CompletedTask;
        }
    }
}