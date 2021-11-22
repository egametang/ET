/**
 * 封装List，用于重用
 */

using System.Collections.Generic;

namespace ET
{
    public class ListComponent<T>: Object
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
            
            base.Dispose();
            
            this.List.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }

    public class ListComponentDisposeChildren<T>: Object where T : Object
    {
        private bool isDispose;
        
        public static ListComponentDisposeChildren<T> Create()
        {
            ListComponentDisposeChildren<T> listComponent = ObjectPool.Instance.Fetch<ListComponentDisposeChildren<T>>();
            listComponent.isDispose = false;
            return listComponent;
        }
        
        public List<T> List = new List<T>();

        public override void Dispose()
        {
            if (this.isDispose)
            {
                return;
            }
            
            this.isDispose = true;

            base.Dispose();

            foreach (T entity in this.List)
            {
                entity.Dispose();
            }

            this.List.Clear();
            
            ObjectPool.Instance.Recycle(this);
        }
    }
}