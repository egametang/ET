using System;
using System.Linq;

namespace ET.Server
{
    public static class RobotManagerComponentSystem
    {
        public static async ETTask<Scene> NewRobot(this RobotManagerComponent self, int zone)
        {
            Scene clientScene = null;
            try
            {
                clientScene = await Client.SceneFactory.CreateClientScene(zone, "Robot");
                await Client.LoginHelper.Login(clientScene, zone.ToString(), zone.ToString());
                await Client.EnterMapHelper.EnterMapAsync(clientScene);
                Log.Debug($"create robot ok: {zone}");
                return clientScene;
            }
            catch (Exception e)
            {
                clientScene?.Dispose();
                throw new Exception($"RobotSceneManagerComponent create robot fail, zone: {zone}", e);
            }
        }
        
        public static void RemoveAll(this RobotManagerComponent self)
        {
            foreach (Entity robot in self.Children.Values.ToArray())        
            {
                robot.Dispose();
            }
        }
        
        public static void Remove(this RobotManagerComponent self, long id)
        {
            self.GetChild<Scene>(id)?.Dispose();
        }

        public static void Clear(this RobotManagerComponent self)
        {
            foreach (Entity entity in self.Children.Values.ToArray())
            {
                entity.Dispose();
            }
        }
    }
}