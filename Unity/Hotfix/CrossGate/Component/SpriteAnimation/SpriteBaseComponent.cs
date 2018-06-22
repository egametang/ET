using System.Threading;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [ObjectSystem]
    public class SpriteBaseComponentLateUpdateSystem : LateUpdateSystem<SpriteBaseComponent>
    {
        public override void LateUpdate(SpriteBaseComponent self)
        {
            self.LateUpdate();
        }
    }

    public class SpriteBaseComponent : Component
    {
        public SpriteRenderer spriteRenderer { get; set; }
        public CancellationTokenSource cancellationToken { get; set; }

        public virtual void LateUpdate()
        {
        }

        public virtual void Init(ImageConfig config)
        {
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            this.spriteRenderer.sprite = null;
            this.cancellationToken.Cancel();
            this.cancellationToken = null;
        }
    }
}