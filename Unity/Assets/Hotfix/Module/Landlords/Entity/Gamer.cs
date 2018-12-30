using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 玩家对象
    /// </summary>
    public sealed class Gamer : Entity
    {
        //玩家唯一ID
        public long UserID { get; set; }

        //是否准备
        public bool IsReady { get; set; }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            this.UserID = 0;
            this.IsReady = false;
        }
    }
}
