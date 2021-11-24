using System.Collections.Generic;

namespace ET
{
    public class ListComponent<T>: DisposeObject
    {
        private bool isDispose;

        public static ListComponent<T> Create()
        {
            ListComponent<T> listComponent = ObjectPool.Instance.Fetch<ListComponent<T>>();
            listComponent.isDispose = false;
            return listComponent;
        }
        
        public List<T> List { get; } = new List<T>();

        public override void Dispose()
        {
            if (this.isDispose)
            {
                return;
            }

            this.isDispose = true;
            
            this.List.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}