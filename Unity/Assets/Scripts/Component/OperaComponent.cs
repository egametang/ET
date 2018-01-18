using UnityEngine;

namespace Model
{
    [ObjectEvent]
    public class OperaComponentSystem : ObjectSystem<OperaComponent>, IUpdate, IAwake
    {
        public void Update()
        {
            this.Get().Update();
        }

	    public void Awake()
	    {
		    this.Get().Awake();
	    }
    }

    public class OperaComponent: Component
    {
        public Vector3 ClickPoint;

	    public int mapMask;

	    public void Awake()
	    {
		    this.mapMask = LayerMask.GetMask("Map");
	    }

        public void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
	            if (Physics.Raycast(ray, out hit, 1000, this.mapMask))
	            {
					this.ClickPoint = hit.point;
		            SessionComponent.Instance.Session.Send(new Frame_ClickMap() { X = (int)(this.ClickPoint.x * 1000), Z = (int)(this.ClickPoint.z * 1000) });
				}
            }
        }
    }
}
