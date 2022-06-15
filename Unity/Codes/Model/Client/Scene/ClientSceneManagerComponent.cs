using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class ClientSceneManagerComponent: Entity, IAwake, IDestroy
    {
        public static ClientSceneManagerComponent Instance;
        public Dictionary<int, Scene> ClientScenes = new Dictionary<int, Scene>();
    }
}