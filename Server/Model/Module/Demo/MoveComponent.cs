using System.Threading;
using System.Threading.Tasks;
using PF;

namespace ETModel
{
    public class MoveComponent: Component
    {
        public Vector3 Target;

        // 开启移动协程的时间
        public long StartTime;

        public long lastFrameTime;

        // 开启移动协程的Unit的位置
        public Vector3 StartPos;

        // 移动的方向标准化
        public Vector3 DirNormalized;

        // 当前的移动速度
        public float Speed = 5;

        public void Awake()
        {
            this.Target = this.GetParent<Unit>().Position;
        }
        
        // 开启协程移动,每100毫秒移动一次，并且协程取消的时候会计算玩家真实移动
        // 比方说玩家移动了2500毫秒,玩家有新的目标,这时旧的移动协程结束,将计算250毫秒移动的位置，而不是300毫秒移动的位置
        public async Task StartMove(CancellationToken cancellationToken)
        {
            Unit unit = this.GetParent<Unit>();
            this.StartPos = unit.Position;
            this.StartTime = TimeHelper.Now();
            this.DirNormalized = (this.Target - unit.Position).normalized;
            
            TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
            
            // 协程如果取消，将算出玩家的真实位置，赋值给玩家
            cancellationToken.Register(() =>
            {
                // 算出当前玩家的位置
                long timeNow = TimeHelper.Now();
                unit.Position = StartPos + this.DirNormalized * this.Speed * (timeNow - this.StartTime) / 1000f;
            });
            
            while (true)
            {
                Vector3 willPos = unit.Position + this.DirNormalized * this.Speed * 0.05f;
                if ((willPos - this.StartPos).magnitude > (this.Target - this.StartPos).magnitude - 0.1f)
                {
                    unit.Position = this.Target;
                    break;
                }
                
                await timerComponent.WaitAsync(50, cancellationToken);
                
                long timeNow = TimeHelper.Now();
                lastFrameTime = timeNow;
                unit.Position = StartPos + this.DirNormalized * this.Speed * (timeNow - this.StartTime) / 1000f;
            }
        }
        
        public async Task MoveToAsync(Vector3 target, CancellationToken cancellationToken)
        {
            // 新目标点离旧目标点太近，不设置新的
            if ((target - this.Target).sqrMagnitude < 0.01f)
            {
                return;
            }

            // 距离当前位置太近
            if ((this.GetParent<Unit>().Position - target).sqrMagnitude < 0.01f)
            {
                return;
            }
            
            this.Target = target;
            
            // 开启协程移动
            await StartMove(cancellationToken);
        }
    }
}