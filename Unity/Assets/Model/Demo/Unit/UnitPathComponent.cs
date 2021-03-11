using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ET
{
    public class UnitPathComponent: Entity
    {
        public List<Vector3> Path = new List<Vector3>();

        public Vector3 ServerPos;

        public ETCancellationToken ETCancellationToken;
    }
}