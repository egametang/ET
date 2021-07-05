using UnityEngine;

namespace ET
{
    public class XunLuoPathComponent: Entity
    {
        public Vector3[] path = new Vector3[] { new Vector3(0, 0, 0), new Vector3(20, 0, 0), new Vector3(20, 0, 20), new Vector3(0, 0, 20), };

        public int Index;
    }
}