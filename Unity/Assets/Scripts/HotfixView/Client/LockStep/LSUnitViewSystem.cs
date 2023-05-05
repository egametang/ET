using System;
using UnityEngine;

namespace ET.Client
{
    public static class LSUnitViewSystem
    {
        public class AwakeSystem: AwakeSystem<LSUnitView, GameObject>
        {
            protected override void Awake(LSUnitView self, GameObject go)
            {
                self.GameObject = go;
                self.Transform = go.transform;
            }
        }
    }
}