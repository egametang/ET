using System.Collections.Generic;
using PF;
using UnityEngine;

namespace ETModel
{
    [ObjectSystem]
    public class ABPathAwakeSystem : AwakeSystem<ABPathWrap, Vector3, Vector3>
    {
        public override void Awake(ABPathWrap self, Vector3 start, Vector3 end)
        {
            self.Awake(start, end);
        }
    }
    
    public class ABPathWrap: Component
    {
        public ABPath Path { get; private set; }

        public void Awake(Vector3 start, Vector3 end)
        {
            this.Path = ABPath.Construct(start, end);
            this.Path.Claim(this);
        }

        public List<Vector3> Result
        {
            get
            {
                return this.Path.vectorPath;
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            this.Path.Release(this);
        }
    }
}