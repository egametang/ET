﻿namespace ET
{
    public static class EntitySceneFactory
    {
        public static Scene CreateScene(long id, long instanceId, int zone, SceneType sceneType, string name, Entity parent = null)
        {
            Scene scene = new Scene(id, instanceId, zone, sceneType, name, parent);

            return scene;
        }

        public static Scene CreateScene(int zone, SceneType sceneType, string name, Entity parent = null)
        {
            long instanceId = IdGenerater.Instance.GenerateInstanceId();
            Scene scene = new Scene(zone, instanceId, zone, sceneType, name, parent);
            return scene;
        }
    }
}