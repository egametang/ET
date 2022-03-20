using System;

namespace ET.Client
{
    public static class CurrentScenesComponentSystem
    {
        public static Scene CurrentScene(this Scene zoneScene)
        {
            return zoneScene.GetComponent<CurrentScenesComponent>()?.Scene;
        }
    }
}