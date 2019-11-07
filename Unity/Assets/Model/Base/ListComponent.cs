/**
 * 封装List，用于重用
 */

using System.Collections.Generic;

namespace ETModel
{
    public class ListComponent <T> : Entity
    {
        public List<T> List = new List<T>();

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            this.List.Clear();
        }
    }
    
    public class ListComponentDisposeChildren <T> : Entity where T : Entity
    {
        public List<T> List = new List<T>();

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();

            foreach (T entity in this.List)
            {
                entity.Dispose();
            }
            this.List.Clear();
        }
    }
}