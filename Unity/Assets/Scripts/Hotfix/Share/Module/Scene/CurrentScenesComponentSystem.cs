using System;

namespace ET
{
    public static partial class CurrentScenesComponentSystem
    {
        public static Scene CurrentScene(this Fiber fiber)
        {
            return fiber.GetComponent<CurrentScenesComponent>()?.Scene;
        }
    }
}