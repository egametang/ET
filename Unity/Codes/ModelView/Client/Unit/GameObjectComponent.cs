using UnityEngine;

namespace ET.Client
{
    public class GameObjectComponent: Entity, IAwake, IDestroy
    {
        public GameObject GameObject { get; set; }
    }
}