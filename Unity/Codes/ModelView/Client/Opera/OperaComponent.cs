using System;

using UnityEngine;

namespace ET.Client
{
	public class OperaComponent: Entity, IAwake, IUpdate
    {
        public Vector3 ClickPoint;

	    public int mapMask;

	    public readonly C2M_PathfindingResult frameClickMap = new C2M_PathfindingResult();
    }
}
