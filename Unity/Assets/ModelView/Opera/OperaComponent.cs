using System;

using UnityEngine;

namespace ET
{
	public class OperaComponent: Entity
    {
        public Vector3 ClickPoint;

	    public int mapMask;

	    public void Awake()
	    {
		    this.mapMask = LayerMask.GetMask("Map");
	    }

	    public readonly Frame_ClickMap frameClickMap = new Frame_ClickMap();
    }
}
