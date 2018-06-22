using System.Threading;
using UnityEngine;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class StaticSpriteComponentLateUpdateSystem : AwakeSystem<StaticSpriteComponent, SpriteRenderer>
    {
        public override void Awake(StaticSpriteComponent self, SpriteRenderer render)
        {
            self.Awake(render);
        }
    }

    public class StaticSpriteComponent : SpriteBaseComponent
    {
        private bool breakReloadLoop;

        public void Awake(SpriteRenderer render)
        {
            this.spriteRenderer = render;
            this.cancellationToken = new CancellationTokenSource();
        }

        public override void Init(ImageConfig config)
        {
            base.Init(config);
            this.breakReloadLoop = false;
            Sprite sp = ResourceComponent.Instance.GetMapItemSprite(config.PngId);
            this.spriteRenderer.sprite = sp;
            NullCheck(config.PngId);
        }
        
        private async void NullCheck(string pngid)
        {
            //如果图片为空, 则需要循环访问得到图片
            while (this.spriteRenderer.sprite == null)
            {
                await ETModel.Game.Scene.GetComponent<TimerComponent>().WaitAsync(1000, this.cancellationToken.Token);
                this.spriteRenderer.sprite = ResourceComponent.Instance.GetMapItemSprite(pngid);
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.breakReloadLoop = true;
            base.Dispose();
        }
    }
}
