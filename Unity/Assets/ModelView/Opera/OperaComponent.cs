using System;

using UnityEngine;

namespace ET
{
	public class OperaComponent: Entity
    {
        public Vector3 ClickPoint;

	    public int mapMask;

	    public readonly Frame_ClickMap frameClickMap = new Frame_ClickMap();
    }
}
