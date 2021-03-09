using UnityEngine;

namespace ET
{
    public class UnitPathComponent: Entity
    {
        public Vector3 Target;

        private RecastPath recastPath;

        public ETCancellationToken CancellationToken;

        public RecastPath RecastPath
        {
            get
            {
                return this.recastPath;
            }
            set
            {
                if (recastPath != null)
                    recastPath.Dispose();
                this.recastPath = value;
            }
        }
    }
}