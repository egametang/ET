using System.Collections.Generic;
using PF;

namespace ETModel
{
    [ObjectSystem]
    public class ABPathAwakeSystem : AwakeSystem<ABPath, Vector3, Vector3>
    {
        public override void Awake(ABPath self, Vector3 start, Vector3 end)
        {
            self.Awake(start, end);
        }
    }
    
    public class ABPath: Component
    {
        public Path Path { get; private set; }

        public void Awake(Vector3 start, Vector3 end)
        {
            this.Path = PF.ABPath.Construct(start, end);
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