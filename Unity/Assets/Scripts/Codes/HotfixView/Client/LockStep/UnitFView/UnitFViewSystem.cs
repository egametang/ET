using System;
using UnityEngine;

namespace ET.Client
{
    public static class UnitFViewSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<UnitFView, GameObject>
        {
            protected override void Awake(UnitFView self, GameObject go)
            {
                self.GameObject = go;
            }
        }
    }
}