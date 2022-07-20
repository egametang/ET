using System;

namespace ET
{
    public static class CurrentScenesComponentSystem
    {
        public static Scene CurrentScene(this Scene clientScene)
        {
            return clientScene.GetComponent<CurrentScenesComponent>()?.Scene;
        }
    }
}