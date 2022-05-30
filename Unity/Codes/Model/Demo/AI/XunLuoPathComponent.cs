using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class XunLuoPathComponent: Entity, IAwake
    {
        public Vector3[] path = new Vector3[] { new Vector3(0, 0, 0), new Vector3(20, 0, 0), new Vector3(20, 0, 20), new Vector3(0, 0, 20), };

        public int Index;
    }
}