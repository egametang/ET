using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public class UIGlobalComponent: Entity, IAwake
    {
        public Dictionary<int, Transform> UILayers = new Dictionary<int, Transform>();
    }
}