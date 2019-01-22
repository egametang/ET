using BehaviorDesigner.Runtime;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [Event(EventIdType.InitSceneStart)]
    public class InitSceneStart_TestBehaivorTree : AEvent
    {
        public override void Run()
        {
            Game.Scene.AddComponent<BehaviorTreeComponent, BehaviorTree>(GameObject.Find("TestBehaviorTree").GetComponent<BehaviorTree>());
        }
    }
}
