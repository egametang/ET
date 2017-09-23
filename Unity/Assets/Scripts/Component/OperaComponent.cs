using UnityEngine;

namespace Model
{
    [ObjectEvent]
    public class OperaComponentEvent : ObjectEvent<OperaComponent>, IUpdate
    {
        public void Update()
        {
            this.Get().Update();
        }
    }

    public class OperaComponent: Component
    {
        public Vector3 ClickPoint;

        public void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 1000);
                this.ClickPoint = hit.point;
                Log.Debug($"click {this.ClickPoint}");
                SessionComponent.Instance.Session.Send(new Frame_ClickMap() {X = (int)(this.ClickPoint.x * 1000), Z = (int)(this.ClickPoint.z * 1000)});
            }
        }
    }
}
