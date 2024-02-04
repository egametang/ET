using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class UIGlobalComponent: Entity, IAwake
    {
        public Dictionary<int, Transform> UILayers = new Dictionary<int, Transform>();
    }
}