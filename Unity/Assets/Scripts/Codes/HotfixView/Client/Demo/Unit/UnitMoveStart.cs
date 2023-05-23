using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class UnitMoveStart: AEvent<Scene, EventType.MoveStart>
    {
        protected override async ETTask Run(Scene scene, EventType.MoveStart args)
        {
            Unit unit = args.Unit;
            unit.GetComponent<AnimationComponent>().Play(AnimClipType.Run);
            await ETTask.CompletedTask;
        }
    }
    
    [Event(SceneType.Current)]
    public class UnitMoveStop: AEvent<Scene, EventType.MoveStop>
    {
        protected override async ETTask Run(Scene scene, EventType.MoveStop args)
        {
            Unit unit = args.Unit;
            unit.GetComponent<AnimationComponent>().Play(AnimClipType.Idle);
            await ETTask.CompletedTask;
        }
    }
}